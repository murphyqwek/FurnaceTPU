using FurnaceCore.IOManager;
using FurnaceCore.Utils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO.Ports;
using System.Threading;
using System.Timers;
using static System.Runtime.InteropServices.JavaScript.JSType;
namespace FurnaceCore.Port
{
    public class PortModule : IPort, IDisposable
    {
        private readonly SerialPort _serialPort;
        private readonly IOManager.IOManager _ioManager;
        private readonly ConcurrentQueue<byte> _receiveQueue = new();
        private readonly System.Timers.Timer _frameTimer;
        private readonly object _frameTimerLock = new();
        // Очередь ТОЛЬКО для команд с ожиданием ответа
        private readonly ConcurrentQueue<byte[]> _awaitResponseQueue = new();
        private readonly object _responseLock = new object();
        private bool _isWaitingForResponse = false;
        private CancellationTokenSource _responseTimeoutCts;
        private const double FrameTimeoutMs = 25; // для 115200 — идеально
        private const int DefaultResponseTimeoutMs = 300; // таймаут для команд с ответом

        private byte[] _currentCommand;
        private int _currentRetryCount = 0;
        private const int MaxRetries = 3;

        public string Name { get => _serialPort.PortName; set => _serialPort.PortName = value; }
        public bool IsOpen => _serialPort?.IsOpen == true;
        public event Action<string> LogInformation;
        public event Action<string> LogWarning;
        public event Action<string> LogError;
        public PortModule(SerialPort serialPort, IOManager.IOManager ioManager)
        {
            _serialPort = serialPort ?? throw new ArgumentNullException(nameof(serialPort));
            _ioManager = ioManager ?? throw new ArgumentNullException(nameof(ioManager));
            _serialPort.DataReceived += OnDataReceived;
            _frameTimer = new System.Timers.Timer(FrameTimeoutMs);
            _frameTimer.AutoReset = false;
            _frameTimer.Elapsed += OnFrameTimeout;
        }
        /// <summary>
        /// Отправить команду СРАЗУ, БЕЗ ожидания ответа и БЕЗ блокировки очереди
        /// </summary>
        public void SendData(byte[] data)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));
            _awaitResponseQueue.Enqueue(data);
            TrySendNextAwaitedCommand();
            /*if (data == null) throw new ArgumentNullException(nameof(data));
            if (!_serialPort.IsOpen) return;
            _serialPort.Write(data, 0, data.Length);
            string hex = BitConverter.ToString(data).Replace("-", " ");
            LogInformation?.Invoke($"Отправлено без ответа: {hex}");
            Thread.Sleep(3); // ← только тишина, без блокировки*/
        }
        /// <summary>
        /// Отправить команду и ЖДАТЬ ответа (или таймаут)
        /// Блокирует очередь до получения ответа
        /// </summary>
        public void SendDataWithResponse(byte[] data)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));
            _awaitResponseQueue.Enqueue(data);
            TrySendNextAwaitedCommand();
        }
        // ================================================
        // ВНУТРЕННЯЯ ЛОГИКА ДЛЯ КОМАНД С ОТВЕТОМ
        // ================================================
        private void TrySendNextAwaitedCommand()
        {
            byte[] dataToSend = null;

            lock (_responseLock)
            {
                if (!_serialPort.IsOpen)
                {
                    LogWarning?.Invoke("Порт закрыт — очищаем очередь команд с ответом и сбрасываем состояние.");
                    _awaitResponseQueue.Clear();
                    _currentCommand = null;
                    _currentRetryCount = 0;
                    _isWaitingForResponse = false;
                    _responseTimeoutCts?.Dispose();
                    _responseTimeoutCts = null;
                    return;
                }

                while (true)
                {
                    if (_isWaitingForResponse) return;  // ← всё так же, выходим если ждём

                    // Если есть текущая команда на retry
                    if (_currentCommand != null)
                    {
                        if (_currentRetryCount < MaxRetries)
                        {
                            _currentRetryCount++;
                            dataToSend = _currentCommand;
                            LogInformation?.Invoke($"Переотправка команды... (попытка {_currentRetryCount}/{MaxRetries})");
                        }
                        else
                        {
                            // Все попытки исчерпаны
                            string hex = BitConverter.ToString(_currentCommand).Replace("-", " ");
                            LogError?.Invoke($"Команда не ответила после {MaxRetries} попыток: {hex}");

                            // Сбрасываем и берём следующую
                            _currentCommand = null;
                            _currentRetryCount = 0;
                            continue;  // ← цикл продолжит, чтобы взять новую команду
                        }
                    }
                    else
                    {
                        // Берём новую из очереди
                        if (!_awaitResponseQueue.TryDequeue(out byte[] nextData))
                            return;  // очередь пуста — выходим

                        _currentCommand = nextData;
                        _currentRetryCount = 1;
                        dataToSend = _currentCommand;
                    }

                    // Если дошли сюда — есть dataToSend, устанавливаем ожидание
                    _isWaitingForResponse = true;

                    // Запускаем таймаут (один раз на отправку)
                    _responseTimeoutCts?.Dispose();
                    _responseTimeoutCts = new CancellationTokenSource();
                    _responseTimeoutCts.CancelAfter(DefaultResponseTimeoutMs);

                    _responseTimeoutCts.Token.Register(() =>
                    {
                        lock (_responseLock)
                        {
                            if (!_isWaitingForResponse) return;

                            LogWarning?.Invoke($"Таймаут ответа на команду (попытка {_currentRetryCount}/{MaxRetries})");

                            _isWaitingForResponse = false;  // ← сброс для следующей итерации

                            // Вызываем метод заново — но без рекурсии, он сам зациклится
                            TrySendNextAwaitedCommand();
                        }
                    });

                    // Выходим из while, чтобы отправить вне lock
                    break;
                }
            }

            // === ВНЕ lock — отправляем ===
            if (dataToSend != null)
            {
                try
                {
                    _serialPort.Write(dataToSend, 0, dataToSend.Length);
                    string hex = BitConverter.ToString(dataToSend).Replace("-", " ");
                    LogInformation?.Invoke($"[Попытка {_currentRetryCount}/{MaxRetries}] Отправлено: {hex}");
                    Thread.Sleep(3);
                }
                catch (Exception ex)
                {
                    LogError?.Invoke($"Ошибка отправки: {ex.Message}");
                    lock (_responseLock)
                    {
                        _isWaitingForResponse = false;
                        // Сброс текущей команды, если нужно, но для простоты — просто retry через вызов
                        TrySendNextAwaitedCommand();
                    }
                }
            }
        }
        
        /*private void OnFrameTimeout(object sender, ElapsedEventArgs e)
        {
            lock (_frameTimerLock)
                _frameTimer.Stop();
            var buffer = new List<byte>();
            while (_receiveQueue.TryDequeue(out byte b))
                buffer.Add(b);
            if (buffer.Count == 0) return;
            var frame = buffer.ToArray();
            string hex = BitConverter.ToString(frame).Replace("-", " ");
            LogInformation?.Invoke("Текущий кадр: " + hex);
            if (frame.Length >= 4 && IsValidCrc(frame))
            {
                LogInformation?.Invoke($"Валидный ответ: {hex}");
                _ioManager.HandleData(hex);
                // ВАЖНО: завершаем ожидание только если мы его ждали
                lock (_responseLock)
                {
                    if (_isWaitingForResponse)
                    {
                        _isWaitingForResponse = false;
                        _responseTimeoutCts?.Dispose();
                        _responseTimeoutCts = null;
                        TrySendNextAwaitedCommand(); // ← следующая команда с ответом
                    }
                }
            }
            else
            {
                LogWarning?.Invoke($"Битый кадр: {hex}");
            }
        }*/
        private void OnFrameTimeout(object sender, ElapsedEventArgs e)
        {
            lock (_frameTimerLock)
                _frameTimer.Stop();
            var buffer = new List<byte>();
            while (_receiveQueue.TryDequeue(out byte b))
                buffer.Add(b);
            if (buffer.Count == 0) return;
            var bytes = buffer.ToArray();
            string hexAll = BitConverter.ToString(bytes).Replace("-", " ");
            LogInformation?.Invoke("Текущий буфер: " + hexAll);
            // Пытаемся вытащить один или несколько валидных кадров
            bool hasAnyValidFrame = ExtractAndHandleFrames(bytes);
            // Если у нас была команда с ожиданием ответа — считаем её выполненной
            // как только получили хотя бы один валидный кадр
            if (hasAnyValidFrame)
            {
                lock (_responseLock)
                {
                    if (_isWaitingForResponse)
                    {
                        _isWaitingForResponse = false;
                        _responseTimeoutCts?.Dispose();
                        _responseTimeoutCts = null;
                        _currentCommand = null;
                        _currentRetryCount = 0;
                        TrySendNextAwaitedCommand();
                    }
                }
            }
        }
        private void OnResponseTimeout(object sender, ElapsedEventArgs e)
        {
            LogWarning?.Invoke($"Таймаут {DefaultResponseTimeoutMs} мс — ответа нет. Продолжаем...");
            TrySendNextAwaitedCommand();
        }
        private bool IsValidCrc(byte[] frame)
        {
            var frameWithoutCrc = new byte[frame.Length - 2];
            byte[] frameCrc = new byte[2];
            Array.Copy(frame, 0, frameWithoutCrc, 0, frame.Length - 2);
            Array.Copy(frame, frame.Length - 2, frameCrc, 0, 2);
            byte[] calculatedCrc = ModBusCRC.CalculateCRC(frameWithoutCrc);
            if (calculatedCrc[0] == frameCrc[0] && calculatedCrc[1] == frameCrc[1])
            {
                return true;
            }
            string calculatedCRCHex = BitConverter.ToString(calculatedCrc).Replace("-", " ");
            string frameCRCHex = BitConverter.ToString(frameCrc).Replace("-", " ");
            LogWarning?.Invoke($"Неверный CRC. Ожидалось {calculatedCRCHex}, получено {frameCRCHex}");
            return false;
        }
        /// <summary>
        /// Разобрать буфер на несколько Modbus-кадров по CRC и отправить каждый в IOManager.
        /// Возвращает true, если найден хотя бы один валидный кадр.
        /// </summary>
        private bool ExtractAndHandleFrames(byte[] buffer)
        {
            bool anyValidFrame = false;
            int offset = 0;
            // Минимальная длина кадра: адрес + функция + минимум 1 байт данных + CRC(2)
            const int MinFrameLength = 4;
            while (offset <= buffer.Length - MinFrameLength)
            {
                int frameLength = FindNextFrameLengthByCrc(buffer, offset);
                if (frameLength <= 0)
                {
                    // Не нашли валидный кадр с этого байта — двигаемся на байт вперёд
                    offset++;
                    continue;
                }
                // Нашли валидный подкадр
                var frame = new byte[frameLength];
                Array.Copy(buffer, offset, frame, 0, frameLength);
                string hexFrame = BitConverter.ToString(frame).Replace("-", " ");
                LogInformation?.Invoke($"Валидный подкадр: {hexFrame}");
                _ioManager.HandleData(hexFrame);
                anyValidFrame = true;
                offset += frameLength;
            }
            if (!anyValidFrame)
            {
                string allHex = BitConverter.ToString(buffer).Replace("-", " ");
                LogWarning?.Invoke($"Не удалось выделить ни одного валидного кадра в буфере: {allHex}");
            }
            return anyValidFrame;
        }
        /// <summary>
        /// Найти длину ближайшего валидного кадра по CRC, начиная с offset.
        /// Если не найден — вернуть 0.
        /// </summary>
        private int FindNextFrameLengthByCrc(byte[] buffer, int offset)
        {
            const int MinFrameLength = 4;
            // Начинаем с минимального возможного кадра
            for (int end = offset + MinFrameLength - 1; end < buffer.Length; end++)
            {
                int length = end - offset + 1;
                var candidate = new byte[length];
                Array.Copy(buffer, offset, candidate, 0, length);
                if (IsValidCrc(candidate))
                {
                    return length;
                }
            }
            return 0;
        }
        private void OnDataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                lock (_frameTimerLock)
                    _frameTimer.Stop();
                List<byte> frame = new List<byte>();
                while (_serialPort.BytesToRead > 0)
                {
                    int b = _serialPort.ReadByte();
                    if (b < 0) break;
                    _receiveQueue.Enqueue((byte)b);
                    frame.Add((byte)b);
                }
                string hex = BitConverter.ToString(frame.ToArray()).Replace("-", " ");
                LogInformation?.Invoke("Получил байты: " + hex);
                RestartFrameTimer();
            }
            catch (Exception ex)
            {
                LogError?.Invoke($"Ошибка чтения: {ex.Message}");
            }
        }
        private void RestartFrameTimer()
        {
            lock (_frameTimerLock)
            {
                _frameTimer.Stop();
                _frameTimer.Interval = FrameTimeoutMs;
                _frameTimer.Start();
            }
        }
        // ================================================
        // СЛУЖЕБНЫЕ МЕТОДЫ
        // ================================================
        public void OpenPort()
        {
            if (!_serialPort.IsOpen)
                _serialPort.Open();
            LogInformation?.Invoke($"Порт {Name} открыт");
        }
        public void ClosePort()
        {
            if (_serialPort.IsOpen)
                _serialPort.Close();
            LogInformation?.Invoke($"Порт {Name} закрыт");
        }
        public void Dispose()
        {
            _frameTimer?.Stop();
            _frameTimer?.Dispose();
            if (_serialPort != null)
            {
                _serialPort.DataReceived -= OnDataReceived;
                if (_serialPort.IsOpen)
                    _serialPort.Close();
                _serialPort.Dispose();
            }
        }
        bool IPort.IsOpen() => _serialPort.IsOpen;
        public void SendData(string data)
        {
            _serialPort.WriteLine(data);
            Thread.Sleep(3);
        }
    }
}