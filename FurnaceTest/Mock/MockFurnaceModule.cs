using FurnaceCore.Filters;
using FurnaceCore.IOManager;
using FurnaceCore.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FurnaceTest.Mock
{
    internal class MockFurnaceModule : IFurnaceModule, IFilter
    {
        private IOManager _iOManager;
        public string Tag = "TEST";

        public MockFurnaceModule(IOManager manager)
        {
            _iOManager = manager;
        }

        public string LastReceived { get; private set; } = "";

        public bool CanHandle(string data)
        {
            return data.StartsWith(Tag);
        }

        public void Handle(string data)
        {
            LastReceived = data;
        }

        public void sendData(string data)
        {
            _iOManager.SendDataToModule(this, data);
        }

    }
}
