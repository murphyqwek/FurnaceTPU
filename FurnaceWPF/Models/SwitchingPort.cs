using FurnaceCore.IOManager;
using FurnaceCore.Port;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.RightsManagement;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Converters;

namespace FurnaceWPF.Models
{
    class SwitchingPort : IPort
    {
        public string Name { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        
        private IPort currentPort;
        private IOManager _ioManager;

        public SwitchingPort(IOManager ioManager)
        {
            this._ioManager = ioManager;
            SwitchToMockPort();
        }

        public SwitchingPort(IOManager ioManager, bool isMockPort)
        {
            this._ioManager = ioManager;

            if(isMockPort)
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
            this.currentPort.ClosePort();
        }

        public bool IsOpen()
        {
            return this.currentPort.IsOpen();
        }

        public void OpenPort()
        {
            this.currentPort.OpenPort();
        }

        public void SendData(string data)
        {
            this.currentPort.SendData(data);
        }

        public void SendData(byte[] data)
        {
            this.currentPort.SendData(data);
        }

        public void SwitchToSerialPort()
        {
            var serialPort = new PortModule(new System.IO.Ports.SerialPort(Name), this._ioManager);

            this.SetPort(serialPort);
        }

        public void SwitchToMockPort()
        {
            var mockPort = new MockPort(this._ioManager);
            mockPort.Name = Name;

            SetPort(mockPort);
        }

        private void SetPort(IPort port)
        {
            if (IsOpen())
            {
                ClosePort();
                port.OpenPort();
            }

            this.currentPort = port;
        }
    }
}
