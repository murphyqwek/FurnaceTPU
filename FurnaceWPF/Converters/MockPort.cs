using FurnaceCore.IOManager;
using FurnaceCore.Port;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FurnaceWPF.Converters
{
    public class MockPort : IPort
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
        public void SendData(string data) 
        {
            SentData.Add(data);
            Console.WriteLine($"MockPort Sent Data: {data}");
        }
        public void ReceiveData(string data) 
        { 
            _manager.HandleData(data);
            Console.WriteLine($"MockPort Received Data: {data}");
        }

        public void SendData(byte[] data)
        {
            SentData.Add(BitConverter.ToString(data).Replace("-", " "));
            Console.WriteLine($"MockPort Sent Data: {SentData.Last()}");
        }
    }
}
