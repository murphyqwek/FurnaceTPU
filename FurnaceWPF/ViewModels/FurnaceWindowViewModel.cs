using FurnaceCore.Port;
using FurnaceWPF.Commands;
using FurnaceWPF.Factories;
using FurnaceWPF.Helpers;
using FurnaceWPF.Models;
using FurnaceWPF.Models.Controllers.Driver;
using FurnaceWPF.Models.Controllers.Zone;
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
using System.Windows;

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

        public PortViewModel PortControlViewMovel { get; }

        public CoolingSystemViewModel CoolingSystem { get; }

        public bool IsSettingsAvalable
        {
            get => !_settings.IsPortOpen;
        }
        #endregion

        private readonly Settings _settings;
        private readonly IServiceProvider _service;

        private Func<SettingsWindow> _getSettingsWindow;

        public RemoteCommand SettingsCommand { get; }

        public FurnaceWindowViewModel(DriverViewModelFactory driverFactory, ZoneViewModelFactory zoneFactory, Settings settings, IServiceProvider service, Func<SettingsWindow> settingsWindow, CoolingSystemViewModel coolingSystem, PortViewModel portViewModel)
        {
            DriveA = driverFactory.GetDriverA();
            DriveB = driverFactory.GetDriverB();
            DriveC = driverFactory.GetDriverC();

            Zone1 = zoneFactory.GetFirstZone();
            Zone2 = zoneFactory.GetSecondZone();
            Zone3 = zoneFactory.GetThirdZone();

            PortControlViewMovel = portViewModel;

            CoolingSystem = coolingSystem;

            this._settings = settings;
            this._service = service;

            this._getSettingsWindow = settingsWindow;

            this._settings.PropertyChanged += OnSettingPropertiesChanged;

            SettingsCommand = new RemoteCommand(OnSettingsButtonClicked);

            App.Services.GetRequiredService<TemperatureController>().GlobalErrorEvent += (m) => NoBlockingMessageBox.ShowError(m);
            App.Services.GetRequiredService<RotationController>().RotationErrorEvent += (m) => NoBlockingMessageBox.ShowError(m);

            _settings.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(Settings.IsPortOpen))
                {
                    if (_settings.IsPortOpen == true)
                    {
                        SetIsWorkingToControls(true);
                    }

                    OnPropertyChanged(nameof(IsSettingsAvalable));
                }
            };

            PortControlViewMovel.PortClosing += () => SetIsWorkingToControls(false);
        }

        private void SetIsWorkingToControls(bool isWorking)
        {
            DriveA.IsWorking = isWorking;
            DriveB.IsWorking = isWorking;
            DriveC.IsWorking = isWorking;

            Zone1.IsWorking = isWorking;
            Zone2.IsWorking = isWorking;
            Zone3.IsWorking = isWorking;
            CoolingSystem.IsPumpOn = isWorking;
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
            _getSettingsWindow().ShowDialog();
        }
    }
}
