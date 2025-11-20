using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FurnaceTest.Mock
{
    internal class HandleFurnaceModule : FurnaceCore.Model.IFurnaceHandleDataModule
    {
        public string LastReceived { get; private set; } = string.Empty;
        public void HandleData(string data)
        {
            LastReceived = data;
        }
    }
}
