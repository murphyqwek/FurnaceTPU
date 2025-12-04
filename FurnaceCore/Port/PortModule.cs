using FurnaceCore.IOManager;
using FurnaceCore.Utils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Timers;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace FurnaceCore.Port
{
    public class PortModule : IPort, IDisposable
    {
        private readonly SerialPort _serialPort;
        private readonly IOManager.IOManager _ioManager;

        // Потокобезопасная очередь байтов
        private readonly ConcurrentQueue<byte> _receiveQueue = new ConcurrentQueue<byte>();

        // Таймер для определения конца сообщения (3.5 символа ~ 50 мс при 19200)
        private readonly System.Timers.Timer _frameTimer;

        private volatile bool _frameInProgress = false;
        private readonly object _frameTimerLock = new object();

        private const double FrameTimeoutMs = 2.5; // под 9600–19200 бод — безопасно

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
            _frameTimer.AutoReset = false; // запускаем вручную после каждого байта
            _frameTimer.Elapsed += OnFrameTimeout;
        }

        private void OnDataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                while (_serialPort.BytesToRead > 0)
                {
                    int b = _serialPort.ReadByte();
                    if (b < 0) break;

                    _receiveQueue.Enqueue((byte)b);

                    // Перезапускаем таймер ожидания конца кадра
                    RestartFrameTimer();
                }
            }
            catch (Exception ex)
            {
                LogError?.Invoke($"Ошибка чтения из порта: {ex.Message}");
            }
        }

        private void RestartFrameTimer()
        {
            lock (_frameTimerLock)
            {
                _frameTimer.Stop();
                _frameTimer.Start();
                _frameInProgress = true;
            }
        }

        private void OnFrameTimeout(object sender, ElapsedEventArgs e)
        {
            lock (_frameTimerLock)
            {
                _frameTimer.Stop();
                _frameInProgress = false;
            }

            // Собираем всё, что накопилось и обрабатываем
            var buffer = new List<byte>();
            while (_receiveQueue.TryDequeue(out byte b))
            {
                buffer.Add(b);
            }

            if (buffer.Count == 0)
                return;

            LogInformation?.Invoke($"Принято {buffer.Count} байт: {BitConverter.ToString(buffer.ToArray()).Replace("-", " ")}");

            ProcessFrame(buffer.ToArray());
        }

        private void ProcessFrame(byte[] frame)
        {
            if (frame.Length < 4)
            {
                LogWarning?.Invoke($"Кадр слишком короткий ({frame.Length} байт) — отбрасываем");
                return;
            }

            if (frame.Length > 256)
            {
                LogWarning?.Invoke($"Кадр слишком длинный ({frame.Length} байт) — отбрасываем");
                return;
            }

            // Проверка CRC
            byte[] frameWithoutCrc = new byte[frame.Length - 2];
            byte[] frameCrc = new byte[2];
            Array.Copy(frame, 0, frameWithoutCrc, 0, frame.Length - 2);
            Array.Copy(frame, frame.Length - 2, frameCrc, 0, 2);

            byte[] calculatedCrc = ModBusCRC.CalculateCRC(frameWithoutCrc);
            
            if (calculatedCrc[0] == frameCrc[0] && calculatedCrc[1] == frameCrc[1])
            {
                // Валидное сообщение — отправляем дальше
                string hex = BitConverter.ToString(frame).Replace("-", " ");
                LogInformation?.Invoke($"Валидное Modbus RTU сообщение: {hex}");

                _ioManager.HandleData(hex);
                return;
            }
            string calculatedCRCHex = BitConverter.ToString(calculatedCrc).Replace("-", " ");
            string frameCRCHex = BitConverter.ToString(frameCrc).Replace("-", " ");

            LogWarning?.Invoke($"Неверный CRC. Ожидалось {calculatedCRCHex}, получено {frameCRCHex}");
        }

        public void OpenPort()
        {
            try
            {
                if (!_serialPort.IsOpen)
                    _serialPort.Open();

                LogInformation?.Invoke($"Порт {Name} открыт");
            }
            catch (Exception ex)
            {
                LogError?.Invoke($"Не удалось открыть порт {Name}: {ex.Message}");
                throw;
            }
        }

        public void ClosePort()
        {
            if (_serialPort.IsOpen)
            {
                _serialPort.Close();
                LogInformation?.Invoke($"Порт {Name} закрыт");
            }
        }

        /// <summary>
        /// Отправка бинарных данных в порт (только так для Modbus RTU!)
        /// </summary>
        public void SendData(byte[] data)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));
            if (!_serialPort.IsOpen)
            {
                LogWarning?.Invoke("Попытка отправить данные в закрытый порт");
                return;
            }

            try
            {
                _serialPort.Write(data, 0, data.Length);
                string hex = BitConverter.ToString(data).Replace("-", " ");
                LogInformation?.Invoke($"Отправлено {data.Length} байт: {hex}");
                LogInformation?.Invoke("Задержка 5 милисекунд перед отправкой новых данных");
                System.Threading.Thread.Sleep(5);
                LogInformation?.Invoke("Задержка закончилась");
            }
            catch (Exception ex)
            {
                LogError?.Invoke($"Ошибка отправки данных: {ex.Message}");
            }
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

        bool IPort.IsOpen()
        {
            return _serialPort.IsOpen;
        }

        public void SendData(string data)
        {
            _serialPort.WriteLine(data);
        }
    }
}