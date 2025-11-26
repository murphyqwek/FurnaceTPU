using FurnaceCore.Port;
using FurnaceWPF.Factories;
using FurnaceWPF.Models;
using Microsoft.Extensions.DependencyInjection;
using pechka4._8;
using pechka4._8.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FurnaceWPF.ViewModels
{
    public class FurnaceWindowViewModel
    {
        public DriverViewModel DriveA { get; }
        public DriverWithArrowViewModel DriveB { get; }
        public DriverWithArrowViewModel DriveC { get; }

        public ZoneViewModel Zone1 { get; }
        public ZoneViewModel Zone2 { get; }
        public ZoneViewModel Zone3 { get; }

        private readonly Settings _settings;
        private readonly IServiceProvider _service;

        public FurnaceWindowViewModel(DriverViewModelFactory driverFactory, ZoneViewModelFactory zoneFactory, Settings settings, IServiceProvider service)
        {
            DriveA = driverFactory.GetDriverA();
            DriveB = driverFactory.GetDriverB();
            DriveC = driverFactory.GetDriverC();

            Zone1 = zoneFactory.GetFirstZone();
            Zone2 = zoneFactory.GetSecondZone();
            Zone3 = zoneFactory.GetThirdZone();

            _settings = settings;
            _service = service;

            _settings.PropertyChanged += OnSettingPropertiesChanged;
        }

        private void OnIsDebugChanged()
        {
            var port = _service.GetRequiredService<IPort>();

            if(port is not SwitchingPort)
            {
                return;
            }

            var switchingPort = port as SwitchingPort;

            if(_settings.IsDebug)
            {
                switchingPort?.SwitchToMockPort();
            }
            else
            {
                switchingPort?.SwitchToSerialPort();
            }
        }

        private void OnSettingPropertiesChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Settings.IsDebug))
            {
                OnIsDebugChanged();
            }
        }
    }
}
