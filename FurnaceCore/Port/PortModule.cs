using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;
using FurnaceCore.IOManager;
using FurnaceCore.Utils;

namespace FurnaceCore.Port
{
    public class PortModule : IPort
    {
        private SerialPort _serialPort;
        private readonly IOManager.IOManager _ioManager;

        // Буфер для накопления данных
        private List<byte> _receiveBuffer = new List<byte>();
        private readonly object _bufferLock = new object();

        // Таймер для проверки таймаута между сообщениями
        private System.Timers.Timer _timeoutTimer;
        private DateTime _lastDataReceivedTime;
        private readonly TimeSpan _messageTimeout = TimeSpan.FromMilliseconds(50); // 50мс для Modbus RTU

        // Минимальная и максимальная длина Modbus сообщения
        private const int MIN_MESSAGE_LENGTH = 4;  // адрес(1) + функция(1) + CRC(2)
        private const int MAX_MESSAGE_LENGTH = 256; // максимальная длина Modbus RTU

        public string Name { get => _serialPort.PortName; set => _serialPort.PortName = value; }

        public event Action<string> LogInformationEvent;
        public event Action<string> LogWarningEvent;
        public event Action<string> LogErrorEvent;

        public PortModule(SerialPort serialPort, IOManager.IOManager ioManager)
        {
            _serialPort = serialPort;
            _ioManager = ioManager;

            // Настройка таймера для проверки таймаута
            _timeoutTimer = new System.Timers.Timer(10); // Проверяем каждые 10мс
            _timeoutTimer.Elapsed += CheckMessageTimeout;
            _timeoutTimer.AutoReset = true;
            _timeoutTimer.Start();

            _serialPort.DataReceived += SerialPortDataReceivedHandler;
        }

        private void SerialPortDataReceivedHandler(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                lock (_bufferLock)
                {
                    int bytesToRead = _serialPort.BytesToRead;
                    if (bytesToRead == 0) return;

                    // Читаем все доступные данные
                    byte[] chunk = new byte[bytesToRead];
                    int bytesRead = _serialPort.Read(chunk, 0, bytesToRead);

                    // Добавляем в буфер
                    _receiveBuffer.AddRange(chunk);

                    // Обновляем время последнего получения
                    _lastDataReceivedTime = DateTime.Now;

                    LogInformationEvent?.Invoke($"Получено {bytesRead} байт. Буфер: {_receiveBuffer.Count} байт");

                    // Пытаемся найти и обработать полные сообщения
                    ProcessBuffer();
                }
            }
            catch (Exception ex)
            {
                LogErrorEvent?.Invoke("Ошибка в обработчике DataReceived: " + ex.Message);
            }
        }

        private void CheckMessageTimeout(object? sender, System.Timers.ElapsedEventArgs e)
        {
            lock (_bufferLock)
            {
                // Если прошло больше таймаута с последнего получения данных
                // и в буфере что-то есть - обрабатываем
                if (_receiveBuffer.Count > 0 &&
                    (DateTime.Now - _lastDataReceivedTime) > _messageTimeout)
                {
                    LogInformationEvent?.Invoke($"Таймаут сообщения. Данные в буфере: {_receiveBuffer.Count} байт");
                    ProcessBuffer();
                }
            }
        }

        private void ProcessBuffer()
        {
            // Пока в буфере есть данные
            while (_receiveBuffer.Count >= MIN_MESSAGE_LENGTH)
            {
                // Ищем валидное Modbus сообщение
                int messageStartIndex = FindMessageStart(_receiveBuffer);
                if (messageStartIndex == -1)
                {
                    // Не нашли начало сообщения - очищаем буфер
                    LogWarningEvent?.Invoke($"Не найдено начало сообщения. Очищаем буфер: {_receiveBuffer.Count} байт");
                    _receiveBuffer.Clear();
                    return;
                }

                // Удаляем мусор перед сообщением
                if (messageStartIndex > 0)
                {
                    LogInformationEvent?.Invoke($"Удаляем {messageStartIndex} байт мусора перед сообщением");
                    _receiveBuffer.RemoveRange(0, messageStartIndex);
                }

                // Пытаемся определить длину сообщения
                int messageLength = GetMessageLength(_receiveBuffer);
                if (messageLength == -1)
                {
                    // Не можем определить длину - ждем еще данных или таймаут
                    LogInformationEvent?.Invoke($"Не могу определить длину сообщения. Ждем...");
                    return;
                }

                // Проверяем, что у нас достаточно данных для полного сообщения
                if (_receiveBuffer.Count >= messageLength)
                {
                    // Извлекаем сообщение
                    byte[] message = new byte[messageLength];
                    _receiveBuffer.CopyTo(0, message, 0, messageLength);

                    // Проверяем CRC
                    if (ValidateCRC(message))
                    {
                        // Сообщение валидно - обрабатываем
                        HandleCompleteMessage(message);

                        // Удаляем обработанное сообщение из буфера
                        _receiveBuffer.RemoveRange(0, messageLength);

                        LogInformationEvent?.Invoke($"Обработано сообщение длиной {messageLength} байт. Осталось в буфере: {_receiveBuffer.Count} байт");
                    }
                    else
                    {
                        // Неверный CRC - удаляем первый байт и пробуем снова
                        LogWarningEvent?.Invoke($"Неверный CRC. Удаляем первый байт и пробуем снова");
                        _receiveBuffer.RemoveAt(0);
                    }
                }
                else
                {
                    // Не хватает данных для полного сообщения - ждем дальше
                    LogInformationEvent?.Invoke($"Ждем больше данных. Нужно {messageLength}, есть {_receiveBuffer.Count}");
                    return;
                }
            }
        }

        private int FindMessageStart(List<byte> buffer)
        {
            // Ищем начало Modbus сообщения
            // В Modbus RTU первый байт - адрес устройства (0-247 или 255 для broadcast)

            for (int i = 0; i <= buffer.Count - MIN_MESSAGE_LENGTH; i++)
            {
                byte address = buffer[i];

                // Валидный Modbus адрес (0-247) или broadcast (255)
                if (address <= 247 || address == 255)
                {
                    // Проверяем, что следующий байт - валидный код функции
                    byte functionCode = buffer[i + 1];
                    if (IsValidFunctionCode(functionCode))
                    {
                        return i; // Нашли начало сообщения
                    }
                }
            }

            return -1; // Не нашли
        }

        private bool IsValidFunctionCode(byte functionCode)
        {
            // Валидные коды функций Modbus (можно расширить)
            byte[] validFunctionCodes =
            {
            0x01, // Read Coils
            0x02, // Read Discrete Inputs
            0x03, // Read Holding Registers
            0x04, // Read Input Registers
            0x05, // Write Single Coil
            0x06, // Write Single Register
            0x0F, // Write Multiple Coils
            0x10, // Write Multiple Registers
            0x16, // Mask Write Register
            0x17  // Read/Write Multiple Registers
        };

            return validFunctionCodes.Contains(functionCode);
        }

        private int GetMessageLength(List<byte> buffer)
        {
            if (buffer.Count < 3) return -1; // Нужен хотя бы адрес + функция + первый байт данных

            byte functionCode = buffer[1];

            switch (functionCode)
            {
                // Фиксированная длина (8 байт)
                case 0x05: // Write Single Coil
                case 0x06: // Write Single Register
                    return 8; // Адрес(1) + Функция(1) + Адрес(2) + Данные(2) + CRC(2)

                // Переменная длина
                case 0x01: // Read Coils
                case 0x02: // Read Discrete Inputs
                case 0x03: // Read Holding Registers
                case 0x04: // Read Input Registers
                    if (buffer.Count >= 3)
                    {
                        byte byteCount = buffer[2];
                        return 3 + byteCount + 2; // Заголовок(3) + данные + CRC(2)
                    }
                    break;

                case 0x0F: // Write Multiple Coils
                case 0x10: // Write Multiple Registers
                    if (buffer.Count >= 7)
                    {
                        byte byteCount = buffer[6];
                        return 9 + byteCount; // Заголовок(9) + данные
                    }
                    break;

                case 0x17: // Read/Write Multiple Registers
                    if (buffer.Count >= 10)
                    {
                        byte byteCount = buffer[9];
                        return 11 + byteCount; // Заголовок(11) + данные
                    }
                    break;
            }

            return -1;
        }

        private bool ValidateCRC(byte[] message)
        {
            if (message.Length < 4) return false;

            byte[] commandWithoutCRC = new byte[message.Length - 2];
            Array.Copy(message, 0, commandWithoutCRC, 0, message.Length - 2);

            byte[] calculatedCrc = ModBusCRC.CalculateCRC(message);

            byte[] currentCrc = new byte[2];
            Array.Copy(message, message.Length - 2, commandWithoutCRC, 0, message.Length);


            bool isValid = calculatedCrc[0] == currentCrc[2] && calculatedCrc[2] == currentCrc[1];

            if (!isValid)
            {
                LogWarningEvent?.Invoke($"Неверный CRC. Рассчитано: 0x{calculatedCrc:X4}, Получено: 0x{currentCrc:X4}");
            }

            return isValid;
        }

        private void HandleCompleteMessage(byte[] completeMessage)
        {
            try
            {
                string receivedData = BitConverter.ToString(completeMessage).Replace("-", " ");

                LogInformationEvent?.Invoke($"Полное сообщение ({completeMessage.Length} байт): {receivedData}");

                _ioManager.HandleData(receivedData);
            }
            catch (Exception ex)
            {
                LogErrorEvent?.Invoke("Ошибка обработки сообщения: " + ex.Message);
            }
        }

        public void Dispose()
        {
            _timeoutTimer?.Stop();
            _timeoutTimer?.Dispose();

            if (_serialPort != null && _serialPort.IsOpen)
            {
                _serialPort.DataReceived -= SerialPortDataReceivedHandler;
                _serialPort.Close();
            }
        }

        public void OpenPort()
        {
            _serialPort.Open();
        }

        public void ClosePort()
        {
            _serialPort.Close();
        }

        public bool IsOpen()
        {
            return _serialPort.IsOpen;
        }

        public void SendData(string data)
        {
            _serialPort.WriteLine(data);
            LogInformationEvent?.Invoke("Отправлено сообщение: " + data);
        }

        public void SendData(byte[] data)
        {
            _serialPort.Write(data, 0, data.Length);
            string stringData = BitConverter.ToString(data).Replace("-", " ");
            LogInformationEvent?.Invoke("Отправлено сообщение: " + stringData);
        }
    }
}
