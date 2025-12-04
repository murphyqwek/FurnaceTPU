using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FurnaceCore.IOManager;
using FurnaceCore.Model;
using FurnaceCore.Port;

namespace FurnaceTest.Mock
{
    internal class MockPort : IPort
    {
        private IOManager _manager;
        public MockPort(IOManager manager)
        {
            _manager = manager;
        }

        public string Name { get; set; }
        public bool IsOpenFlag { get; private set; } = false;
        public List<string> SentData { get; } = new List<string>();
        public void OpenPort() => IsOpenFlag = true;
        public void ClosePort() => IsOpenFlag = false;
        public bool IsOpen() => IsOpenFlag;
        public void SendData(string data) => SentData.Add(data);
        public void ReceiveData(string data) => _manager.HandleData(data);

        public void SendData(byte[] data)
        {
            SentData.Add(BitConverter.ToString(data).Replace("-", " "));
        }

        public void Dispose()
        {

        }

        public void SendDataWithResponse(byte[] data)
        {
            SendData(data);
        }
    }
}
