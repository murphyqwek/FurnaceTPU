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

        private const double FrameTimeoutMs = 5;        // для 115200 — идеально
        private const int DefaultResponseTimeoutMs = 300; // таймаут для команд с ответом

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
            if (!_serialPort.IsOpen) return;

            _serialPort.Write(data, 0, data.Length);
            string hex = BitConverter.ToString(data).Replace("-", " ");
            LogInformation?.Invoke($"Отправлено без ответа: {hex}");

            Thread.Sleep(3); // ← только тишина, без блокировки
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
            byte[] data;
            lock (_responseLock)
            {
                if (_isWaitingForResponse) return;

                if (!_awaitResponseQueue.TryDequeue(out data))
                    return;

                _isWaitingForResponse = true;

                // Запускаем таймаут
                _responseTimeoutCts?.Dispose();
                _responseTimeoutCts = new CancellationTokenSource();
                _responseTimeoutCts.CancelAfter(DefaultResponseTimeoutMs);
                _responseTimeoutCts.Token.Register(() =>
                {
                    lock (_responseLock)
                    {
                        if (!_isWaitingForResponse) return;
                        LogWarning?.Invoke($"Таймаут {DefaultResponseTimeoutMs} мс — ответа нет. Продолжаем...");
                        _isWaitingForResponse = false;
                        TrySendNextAwaitedCommand();
                    }
                });
            }

            _serialPort.Write(data, 0, data.Length);
            string hex = BitConverter.ToString(data).Replace("-", " ");
            LogInformation?.Invoke($"[С ОТВЕТОМ] Отправлено: {hex}");
            Thread.Sleep(3);
        }
        private void OnFrameTimeout(object sender, ElapsedEventArgs e)
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