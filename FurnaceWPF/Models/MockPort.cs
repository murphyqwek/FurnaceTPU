using FurnaceCore.IOManager;
using FurnaceCore.Port;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using pechka4._8;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FurnaceWPF.Models
{
    public class MockPort : IPort
    {
        private IOManager _manager;
        private readonly ILogger<MockPort> _logger;
        public MockPort(IOManager manager, ILogger<MockPort> logger)
        {
            this._manager = manager;
            this._logger = logger;
        }

        public string Name { get; set; } = "COM1";
        public bool IsOpenFlag { get; private set; } = false;
        public List<string> SentData { get; } = new List<string>();
        public void OpenPort() => IsOpenFlag = true;
        public void ClosePort() => IsOpenFlag = false;
        public bool IsOpen() => IsOpenFlag;
        public void SendData(string data) 
        {
            _logger.LogInformation($"MockPort Sent Data: {data}");
            SentData.Add(data);
        }
        public void ReceiveData(string data) 
        {
            _logger.LogInformation($"MockPort Received Data: {data}");
            _manager.HandleData(data);
        }

        public void SendData(byte[] data)
        {
            SentData.Add(BitConverter.ToString(data).Replace("-", " "));
            _logger.LogInformation($"MockPort Sent Data: {SentData.Last()}");
            ReceiveData("03 ");
        }

        public void Dispose()
        {
            _logger.LogInformation("Логгер is disposed");
        }
    }
}
