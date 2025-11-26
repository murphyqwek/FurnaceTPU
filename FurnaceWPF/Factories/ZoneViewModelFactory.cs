using FurnaceCore.Filters;
using FurnaceCore.IOManager;
using FurnaceCore.Model;
using FurnaceCore.Port;
using FurnaceWPF.Converters;
using Microsoft.Extensions.DependencyInjection;
using pechka4._8.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FurnaceWPF.Factories
{
    public class ZoneViewModelFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public ZoneViewModelFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public ZoneViewModel GetZone(string name, byte addressByte, byte channelByte)
        {
            var ioManager = _serviceProvider.GetRequiredService<IOManager>();
            TemperatureModule temperatureModule = new TemperatureModule(addressByte, channelByte, ioManager);
            AddressFilter temperatureFilter = new AddressFilter(temperatureModule.GetAddressByte, temperatureModule);

            ioManager.RegisterFilter(temperatureFilter);
            ioManager.RegisterModulePort(temperatureModule, _serviceProvider.GetRequiredService<IPort>());

            return new ZoneViewModel(name, 0, temperatureModule);
        }

        public ZoneViewModel GetFirstZone()
        {
            return GetZone("Зона 1", 0x01, 0x01);
        }

        public ZoneViewModel GetSecondZone()
        {
            return GetZone("Зона 2", 0x01, 0x01);
        }

        public ZoneViewModel GetThirdZone()
        {
            return GetZone("Зона 3", 0x01, 0x01);
        }
    }
}
