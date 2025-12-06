using FurnaceCore.Filters;
using FurnaceCore.IOManager;
using FurnaceCore.Model;
using FurnaceCore.utlis;
using FurnaceTest.Mock;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FurnaceTest
{
    public class TemperatureTest
    {
        [Theory]
        [InlineData("01 16", 27.8)]
        [InlineData("01 20", 28.8)]
        [InlineData("01 30", 30.4)]
        [InlineData("02 30", 56.0)]
        public async Task GetTemperatureTest(string hexData, double temperature)
        {
            IOManager ioManager = new IOManager();
            MockPort mockPort = new MockPort(ioManager);
            TemperatureModule temperatureModule = new TemperatureModule(0x01, 0x01, ioManager);
            ModbusAddressFilter modbusAddressFilter = new ModbusAddressFilter(0x01, 0x04, temperatureModule);

            ioManager.RegisterModulePort(temperatureModule, mockPort);
            ioManager.RegisterFilter(modbusAddressFilter);

            var task = temperatureModule.GetTemperatureAsync(10000, CancellationToken.None,1);

            mockPort.ReceiveData($"01 04 02 {hexData} 00 00");

            Result<double>? testTemp = await task;

            Assert.True(testTemp.Success);
            Assert.True(testTemp.Value == temperature);
        }

        [Fact]
        public async Task GetUnsuccessResultTest()
        {
            IOManager ioManager = new IOManager();
            MockPort mockPort = new MockPort(ioManager);
            TemperatureModule temperatureModule = new TemperatureModule(0x01, 0x01, ioManager);
            ModbusAddressFilter modbusAddressFilter = new ModbusAddressFilter(0x01, 0x04, temperatureModule);

            ioManager.RegisterModulePort(temperatureModule, mockPort);
            ioManager.RegisterFilter(modbusAddressFilter);

            var task = temperatureModule.GetTemperatureAsync(10000, CancellationToken.None, 1);

            mockPort.ReceiveData($"01 04 02");

            Result<double>? testTemp = await task;

            Assert.True(!testTemp.Success);
        }
    }
}
