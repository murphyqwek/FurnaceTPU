using FurnaceCore.Filters;
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
    public class DriverModuleTest
    {
        [Theory]
        [InlineData(DriversPortEnum.Zero, "01")]
        [InlineData(DriversPortEnum.One, "02")]
        [InlineData(DriversPortEnum.Two, "04")]
        [InlineData(DriversPortEnum.Three, "08")]
        [InlineData(DriversPortEnum.Four, "10")]
        [InlineData(DriversPortEnum.Zero | DriversPortEnum.One, "03")]
        [InlineData(DriversPortEnum.Two | DriversPortEnum.Three, "0C")]
        [InlineData(DriversPortEnum.One | DriversPortEnum.Four, "12")]
        public void DriverTurningOnOffTest(DriversPortEnum driverPort, string expectedChannel)
        {
            IOManager ioManager = new IOManager();
            MockPort mockPort = new MockPort(ioManager);
            DriverModule driverModule = new DriverModule(ioManager, 0);

            ioManager.RegisterModulePort(driverModule, mockPort);

            driverModule.StartDriver(driverPort);

            string[] temp = mockPort.SentData.Last().Split(' ');

            string startCommand = temp[7] + temp[8];

            Assert.True(startCommand == expectedChannel + "01");

            driverModule.StopDriver(driverPort);

            temp = mockPort.SentData.Last().Split(' ');

            string stopCommand = temp[7] + temp[8];

            Assert.True(stopCommand == expectedChannel + "00");
        }

        [Theory]
        [InlineData(0, 10_000, "E0", "2710")]
        [InlineData(1, 5_000, "E2", "1388")]
        [InlineData(2, 3000, "E4", "0BB8")]
        [InlineData(3, 4, "E6", "0004")]
        [InlineData(4, 5555, "E8", "15B3")]
        [InlineData(5, 1234, "EA", "04D2")]
        [InlineData(6, 7456, "EC", "1D20")]
        [InlineData(7, 123, "EE", "007B")]
        public void SetDriverFrequencyTest(int channel, ushort frequency, string channelHex, string frequencyHex)
        {
            IOManager ioManager = new IOManager();
            MockPort mockPort = new MockPort(ioManager);
            DriverModule driverModule = new DriverModule(ioManager, 0);

            ioManager.RegisterModulePort(driverModule, mockPort);

            driverModule.SetDriverFrequency(channel, frequency);

            string[] temp = mockPort.SentData.Last().Split(' ');

            string commandChannel = temp[3];
            string commandFrequency = temp[7] + temp[8];

            Assert.True(commandChannel == channelHex);
            Assert.True(commandFrequency == frequencyHex);
        }


        [Theory]
        [InlineData(0, 6, "00 01 01 06")]
        public async Task GetRotationDataTest(byte channel, byte inputFlags, string inputCommand)
        {
            IOManager ioManager = new IOManager();
            MockPort mockPort = new MockPort(ioManager);
            DriverModule driverModule = new DriverModule(ioManager, channel);
            AddressFilter addressFilter = new AddressFilter(driverModule.GetAddress, driverModule);

            ioManager.RegisterModulePort(driverModule, mockPort);
            ioManager.RegisterFilter(addressFilter);

            var task = driverModule.GetRotationDataAsync(10000, CancellationToken.None);


            mockPort.ReceiveData(inputCommand);

            var result = await task;

            Assert.True(result.Success);

            if(!result.Success)
            {
                return;
            }

            byte resultFlags = 0;

            foreach(var port in result.Value.rotations.Keys)
            {
                var rotation = (byte)(result.Value.rotations.GetValueOrDefault(port) == RotationEnum.Right ? 1 : 0);

                if(rotation == 0)
                {
                    continue;
                }

                switch(port)
                {
                    case DriversPortEnum.Three:
                        resultFlags += 1;
                        break;
                    case DriversPortEnum.Two:
                        resultFlags += 2;
                        break;
                    case DriversPortEnum.One:
                        resultFlags += 4;
                        break;
                }
            }


            Assert.True(resultFlags == inputFlags);
        }
    }
}
