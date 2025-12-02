using FurnaceCore.Filters;
using FurnaceCore.IOManager;
using FurnaceCore.Model;
using FurnaceCore.Port;
using FurnaceWPF.Models;
using FurnaceWPF.Models.Controllers.Zone;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
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
        private readonly IPort _port;
        private readonly Settings _settings;
        private readonly IOManager _ioManager;
        private readonly HeaterModule _heaterModule;
        private readonly ILogger<ZoneController> _zoneLogger;
        private readonly TemperatureController _temperatureController;
        
        public ZoneViewModelFactory(IPort port, IOManager ioManager, HeaterModule heaterModule, Settings settings, ILogger<ZoneController> zoneLogger, TemperatureController temperatureController)
        {
            _port = port;
            _ioManager = ioManager;
            _settings = settings;
            _heaterModule = heaterModule;
            _zoneLogger = zoneLogger;
            _temperatureController = temperatureController;
        }

        public ZoneViewModel GetZone(string name, byte channelByte)
        {
            ZoneController zoneController = new ZoneController(channelByte, _heaterModule, _zoneLogger, _settings, _temperatureController);


            return new ZoneViewModel(name, 0, zoneController, _settings);
        }

        public ZoneViewModel GetFirstZone()
        {
            return GetZoneAndSubscribeToAddressChagned("Зона 1", _settings.ZoneOneChannel, 1);
        }

        public ZoneViewModel GetSecondZone()
        {
            return GetZoneAndSubscribeToAddressChagned("Зона 2", _settings.ZoneTwoChannel, 2);
        }

        public ZoneViewModel GetThirdZone()
        {
            return GetZoneAndSubscribeToAddressChagned("Зона 3", _settings.ZoneThreeChannel, 3);
        }
        
        private ZoneViewModel GetZoneAndSubscribeToAddressChagned(string name, byte channelByte, int zoneNumber)
        {
            var zone = GetZone(name, channelByte);
            SubscribeToZoneAddressChanged(zoneNumber, zone);

            return zone;
        }

        private void SubscribeToZoneAddressChanged(int zoneNumber, ZoneViewModel zoneViewModel)
        {
            string propertyName = zoneNumber switch
            {
                1 => nameof(Settings.ZoneOneChannel),
                2 => nameof(Settings.ZoneTwoChannel),
                3 => nameof(Settings.ZoneThreeChannel),
                _ => throw new Exception("Not all zones could be subcribed")
            };

            _settings.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == propertyName)
                {
                    byte newChanneel = zoneNumber switch
                    {
                        1 => _settings.ZoneOneChannel,
                        2 => _settings.ZoneTwoChannel,
                        3 => _settings.ZoneThreeChannel,
                        _ => throw new InvalidOperationException("Property name validation failed during update.")
                    };

                    zoneViewModel.UpdateZoneChannel(newChanneel);
                }
            };
        }
    }
}
