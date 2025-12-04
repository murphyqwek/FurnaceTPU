using FurnaceCore.IOManager;
using FurnaceCore.Port;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
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
        public string Name 
        { 
            get => _currentPort.Name; 
            set => _currentPort.Name = value; 
        }
        
        private IPort _currentPort;
        private Func<MockPort> _mockPortFactory;
        private Func<PortModule> _portModuleFactory;
        private ILogger<SwitchingPort> _logger;

        public SwitchingPort(Func<MockPort> mockPortFactory, Func<PortModule> _portModuleFactory, ILogger<SwitchingPort> logger)
        {
            this._mockPortFactory = mockPortFactory;
            this._portModuleFactory = _portModuleFactory;
            this._currentPort = mockPortFactory();
            this._logger = logger;
            SwitchToMockPort();
        }

        public SwitchingPort(bool isMockPort, Func<MockPort> mockPortFactory, Func<PortModule> portModuleFactory, ILogger<SwitchingPort> logger)
        {
            this._mockPortFactory = mockPortFactory;
            this._portModuleFactory = portModuleFactory;
            this._currentPort = mockPortFactory();
            this._logger = logger;
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
            _logger.LogInformation("Open port");
            this._currentPort.OpenPort();
        }

        public void SendData(string data)
        {
            _logger.LogInformation("Sending to port: " + data);
            this._currentPort.SendData(data);
        }

        public void SendData(byte[] data)
        {
            _logger.LogInformation($"Sending data: {BitConverter.ToString(data).Replace("-", " ")}");
            this._currentPort.SendData(data);
        }

        public void SwitchToSerialPort()
        {
            _logger.LogInformation("Switching port to serial");
            var portModule = _portModuleFactory();

            this.SetPort(portModule);
        }

        public void SwitchToMockPort()
        {
            _logger.LogInformation("Switching port to mock");
            var mockPort = this._mockPortFactory();

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

        public void Dispose()
        {
            this._currentPort.Dispose();
        }

        public void SendDataWithResponse(byte[] data)
        {
            this._currentPort.SendDataWithResponse(data);
        }
    }
}
