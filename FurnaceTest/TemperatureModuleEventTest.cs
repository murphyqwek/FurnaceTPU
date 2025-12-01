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
    public class TemperatureModuleEventTest
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

            bool eventTriggered = false;
            double? eventValue = null;

            temperatureModule.OnTemperatureGet += (value) =>
            {
                eventTriggered = true;
                eventValue = value.Value;
            };

            var task = temperatureModule.GetTemperatureAsync(10_000, CancellationToken.None);

            mockPort.ReceiveData($"01 04 02 {hexData} 00 00");

            Result<double>? result = await task;

            Assert.True(eventTriggered);
            Assert.Equal(result.Value, temperature);
            Assert.Equal(eventValue, temperature);
        }
    }
}
