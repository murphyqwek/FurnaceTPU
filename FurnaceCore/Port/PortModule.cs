using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;

namespace FurnaceCore.Port
{
    public class PortModule(string name) : IPort
    {
        private string _name = name;
        private SerialPort _serialPort = new SerialPort(name);

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

        private void SerialPortDataReceivedHandler(object sender, SerialDataReceivedEventArgs e)
        {
            string receivedData = _serialPort.ReadLine();
        }
    }
}
