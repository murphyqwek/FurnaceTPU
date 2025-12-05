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
        private readonly Func<HeaterModule> _heaterModuleFactor;
        private readonly ILogger<ZoneController> _zoneLogger;
        private readonly TemperatureController _temperatureController;
        
        public ZoneViewModelFactory(IPort port, IOManager ioManager, Func<HeaterModule> heaterModuleFactor, Settings settings, ILogger<ZoneController> zoneLogger, TemperatureController temperatureController)
        {
            _port = port;
            _ioManager = ioManager;
            _settings = settings;
            _heaterModuleFactor = heaterModuleFactor;
            _zoneLogger = zoneLogger;
            _temperatureController = temperatureController;
        }

        public ZoneViewModel GetZone(string name, Func<byte> channelTemperatureByte, Func<byte> addressHeaterByte)
        {
            var heaterModule = _heaterModuleFactor();

            heaterModule.SetChannelByte(addressHeaterByte());

            ZoneController zoneController = new ZoneController(channelTemperatureByte, heaterModule, _zoneLogger, _settings, _temperatureController);

            _settings.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(Settings.ZoneHeaterOneChannel) ||
                    e.PropertyName == nameof(Settings.ZoneHeaterTwoChannel) ||
                    e.PropertyName == nameof(Settings.ZoneHeaterThreeChannel))
                {
                    heaterModule.SetChannelByte(addressHeaterByte());
                }

            };

            return new ZoneViewModel(name, 0, zoneController, _settings);
        }

        public ZoneViewModel GetFirstZone()
        {
            return GetZoneAndSubscribeToAddressChagned("Зона 1", () => _settings.ZoneOneChannel, 1, () => _settings.ZoneHeaterOneChannel);
        }

        public ZoneViewModel GetSecondZone()
        {
            return GetZoneAndSubscribeToAddressChagned("Зона 2", () => _settings.ZoneTwoChannel, 2, () => _settings.ZoneHeaterTwoChannel);
        }

        public ZoneViewModel GetThirdZone()
        {
            return GetZoneAndSubscribeToAddressChagned("Зона 3",() => _settings.ZoneThreeChannel, 3, () => _settings.ZoneHeaterThreeChannel);
        }
        
        private ZoneViewModel GetZoneAndSubscribeToAddressChagned(string name, Func<byte> channelTemperatureByte, int zoneNumber, Func<byte> channelHeaterByte)
        {
            var zone = GetZone(name, channelTemperatureByte, channelHeaterByte);

            return zone;
        }
       
    }
}
