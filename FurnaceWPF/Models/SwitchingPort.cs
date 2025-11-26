using FurnaceCore.IOManager;
using FurnaceCore.Port;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Security.RightsManagement;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Converters;

namespace FurnaceWPF.Models
{
    class SwitchingPort : IPort
    {
        public string Name { get => _currentPort.Name; set => _currentPort.Name = value; }
        
        private IPort _currentPort;
        private IOManager _ioManager;

        public SwitchingPort(IOManager ioManager)
        {
            this._ioManager = ioManager;
            _currentPort = new MockPort(ioManager);
            SwitchToMockPort();
        }

        public SwitchingPort(IOManager ioManager, bool isMockPort)
        {
            this._ioManager = ioManager;
            _currentPort = new MockPort(ioManager);
            if (isMockPort)
            {
                SwitchToMockPort();
            }
            else
            {
                SwitchToSerialPort();
            }
        }

        public void ClosePort()
        {
            this._currentPort.ClosePort();
        }

        public bool IsOpen()
        {
            return this._currentPort.IsOpen();
        }

        public void OpenPort()
        {
            this._currentPort.OpenPort();
        }

        public void SendData(string data)
        {
            this._currentPort.SendData(data);
        }

        public void SendData(byte[] data)
        {
            this._currentPort.SendData(data);
        }

        public void SwitchToSerialPort()
        {
            var serialPort = new SerialPort();

            var portModule = new PortModule(serialPort, this._ioManager);

            this.SetPort(portModule);
        }

        public void SwitchToMockPort()
        {
            var mockPort = new MockPort(this._ioManager);

            SetPort(mockPort);
        }

        private void SetPort(IPort port)
        {
            port.Name = _currentPort.Name;

            if (IsOpen())
            {
                ClosePort();
                port.OpenPort();
            }

            this._currentPort = port;
        }
    }
}
