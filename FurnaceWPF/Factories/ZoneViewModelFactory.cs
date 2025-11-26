using FurnaceCore.Filters;
using FurnaceCore.IOManager;
using FurnaceCore.Model;
using FurnaceCore.Port;
using FurnaceWPF.Models;
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
        private readonly Settings _settings;

        public ZoneViewModelFactory(IServiceProvider serviceProvider, Settings settings)
        {
            _serviceProvider = serviceProvider;
            _settings = settings;
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
            return GetZoneAndSubscribeToAddressChagned("Зона 1", 0x01, 0x01, 1);
        }

        public ZoneViewModel GetSecondZone()
        {
            return GetZoneAndSubscribeToAddressChagned("Зона 2", 0x01, 0x01, 2);
        }

        public ZoneViewModel GetThirdZone()
        {
            return GetZoneAndSubscribeToAddressChagned("Зона 3", 0x01, 0x01, 3);
        }
        
        private ZoneViewModel GetZoneAndSubscribeToAddressChagned(string name, byte addressByte, byte channelByte, int zoneNumber)
        {
            var zone = GetZone(name, addressByte, channelByte);
            SubscribeToZoneAddressChanged(zoneNumber, zone);

            return zone;
        }

        private void SubscribeToZoneAddressChanged(int zoneNumber, ZoneViewModel zoneViewModel)
        {
            string propertyName = zoneNumber switch
            {
                1 => nameof(Settings.ZoneOneAddress),
                2 => nameof(Settings.ZoneTwoAddress),
                3 => nameof(Settings.ZoneThreeAddress),
                _ => throw new Exception("Not all zones could be subcribed")
            };

            _settings.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == propertyName)
                {
                    byte newAddress = zoneNumber switch
                    {
                        1 => _settings.ZoneOneAddress,
                        2 => _settings.ZoneTwoAddress,
                        3 => _settings.ZoneThreeAddress,
                        _ => throw new InvalidOperationException("Property name validation failed during update.")
                    };

                    zoneViewModel.UpdateZoneAddress(newAddress);
                }
            };
        }
    }
}
