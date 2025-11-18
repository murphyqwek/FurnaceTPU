using FurnaceCore.IOManager;
using FurnaceCore.Model;
using FurnaceTest.Mock;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FurnaceTest
{
    public class HeaterModuleTest
    {
        [Fact]
        public void TurnOnAndOffTest()
        {
            IOManager ioManager = new IOManager();
            MockPort mockPort = new MockPort(ioManager);
            HeaterModule heaterModule = new HeaterModule(0x01, 0x01, ioManager);


            ioManager.RegisterModulePort(heaterModule, mockPort);

            heaterModule.TurnOnHeater();

            Assert.True(mockPort.SentData.Last() == "01 05 00 01 FF 00 DD FA");

            heaterModule.TurnOffHeater();

            Assert.True(mockPort.SentData.Last() == "01 05 00 01 00 00 9C 0A");
        }
    }
}
