using FurnaceCore.Port;
using FurnaceWPF.Commands;
using FurnaceWPF.Factories;
using FurnaceWPF.Models;
using FurnaceWPF.Views;
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
    public class FurnaceWindowViewModel : BaseObservable
    {
        #region Properties
        public DriverViewModel DriveA { get; }
        public DriverWithArrowViewModel DriveB { get; }
        public DriverWithArrowViewModel DriveC { get; }

        public ZoneViewModel Zone1 { get; }
        public ZoneViewModel Zone2 { get; }
        public ZoneViewModel Zone3 { get; }

        public bool IsSettingsAvalable
        {
            get => !_settings.IsRunning;
        }
        #endregion

        private readonly Settings _settings;
        private readonly IServiceProvider _service;

        private SettingsWindow _settingsWindow;

        public RemoteCommand SettingsCommand { get; }

        public FurnaceWindowViewModel(DriverViewModelFactory driverFactory, ZoneViewModelFactory zoneFactory, Settings settings, IServiceProvider service, SettingsWindow settingsWindow)
        {
            DriveA = driverFactory.GetDriverA();
            DriveB = driverFactory.GetDriverB();
            DriveC = driverFactory.GetDriverC();

            Zone1 = zoneFactory.GetFirstZone();
            Zone2 = zoneFactory.GetSecondZone();
            Zone3 = zoneFactory.GetThirdZone();

            this._settings = settings;
            this._service = service;

            this._settingsWindow = settingsWindow;

            this._settings.PropertyChanged += OnSettingPropertiesChanged;

            SettingsCommand = new RemoteCommand(OnSettingsButtonClicked);
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

            if(e.PropertyName == nameof(Settings.IsRunning))
            {
                OnPropertyChanged(nameof(Settings.IsRunning));
            }
        }

        private void OnSettingsButtonClicked()
        {
            _settingsWindow.ShowDialog();
        }
    }
}
