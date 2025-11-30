using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;
using FurnaceCore.IOManager;

namespace FurnaceCore.Port
{
    public class PortModule : IPort
    {
        private IOManager.IOManager _ioManager;
        private string _name;
        private SerialPort _serialPort;

        public PortModule(SerialPort serialPort, IOManager.IOManager ioManager)
        {
            _name = serialPort.PortName;
            _ioManager = ioManager;
            _serialPort = serialPort;
            _serialPort.DataReceived += SerialPortDataReceivedHandler;
        }

        public string Name 
        { 
            get => _name;
            set
            {
                if (_serialPort.IsOpen)
                    throw new InvalidOperationException("Cannot change port name while the port is open.");
                _name = value;
                _serialPort.PortName = value;
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
        }

        public void SendData(byte[] data)
        {
            _serialPort.Write(data, 0, data.Length);
        }

        private void SerialPortDataReceivedHandler(object sender, SerialDataReceivedEventArgs e)
        {
            int bytesToRead = _serialPort.BytesToRead;
            byte[] buffer = new byte[bytesToRead];

            // Читаем данные
            int bytesRead = _serialPort.Read(buffer, 0, bytesToRead);

            string receivedData = BitConverter.ToString(buffer).Replace("-", " ");
            _ioManager.HandleData(receivedData);
        }

        public void Dispose()
        {
            _serialPort.Dispose();
        }
    }
}
