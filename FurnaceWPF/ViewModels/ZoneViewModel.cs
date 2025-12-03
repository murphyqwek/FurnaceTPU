using FurnaceCore.Model;
using FurnaceCore.Port;
using FurnaceWPF.Commands;
using FurnaceWPF.Models;
using FurnaceWPF.Models.Controllers.Zone;
using FurnaceWPF.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Security.RightsManagement;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Converters;

namespace pechka4._8.ViewModels
{
    public class ZoneViewModel : BaseObservable
    {
        private double _inputTemperature = 0;

        private ZoneController _zoneController;
        private Settings _settings;

        #region Properties

        public bool IsWorking 
        { 
            get => _zoneController.IsPollingTemperature;
            set
            {
                UpdateTemperaturePolling(value);
                OnPropertyChanged();
            }
        }

        public bool IsEnabled
        {
            get => _settings.IsPortOpen;
        }

        public double InputTemperature
        {
            get => _inputTemperature;
            set
            {
                // Ограничение диапазона и округление
                double val = Math.Max(0, Math.Min(700, Math.Round(value, 1)));
                if (_inputTemperature != val)
                {
                    _inputTemperature = val;
                    OnPropertyChanged();
                }
            }
        }

        public double CurrentTemperature
        {
            get => _zoneController.CurrentTemperature;
        }

        public string TemperatureText => $"{CurrentTemperature:0.#}°C";

        public Brush TempBrush => InterpolateTempColor(CurrentTemperature);

        public string Name { get; set; }

        // Градиент между синим и жёлтым
        public SolidColorBrush InterpolateTempColor(double temp)
        {
            Color cold = (Color)ColorConverter.ConvertFromString("#197692");
            Color hot = (Color)ColorConverter.ConvertFromString("#E8E26B");
            byte r = (byte)(cold.R + (hot.R - cold.R) * temp / 700);
            byte g = (byte)(cold.G + (hot.G - cold.G) * temp / 700);
            byte b = (byte)(cold.B + (hot.B - cold.B) * temp / 700);
            return new SolidColorBrush(Color.FromRgb(r, g, b));
        }
        #endregion
        

        public RemoteCommand SetTemperature { get; }



        public ZoneViewModel(string name, double initialTemperature, ZoneController zoneController, Settings settings)
        {
            this._zoneController = zoneController;
            this.Name = name;
            this.InputTemperature = initialTemperature;
            this._settings = settings;


            SetTemperature = new RemoteCommand(SetTemperatureHandler);
            _zoneController.ErrorEvent += PollingErrorEventHandler;
            _zoneController.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(ZoneController.CurrentTemperature))
                {
                    OnPropertyChanged(nameof(CurrentTemperature));
                    OnPropertyChanged(nameof(TemperatureText));
                    OnPropertyChanged(nameof(TempBrush));
                }

                if (e.PropertyName == nameof(ZoneController.IsPollingTemperature))
                {
                    OnPropertyChanged(nameof(IsWorking));
                }
            };

            this._settings.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(Settings.IsPortOpen))
                {
                    OnPropertyChanged(nameof(IsWorking));
                    OnPropertyChanged(nameof(IsEnabled));
                }
            };
        }

        public void UpdateZoneChannel(byte newChannel)
        {
            this._zoneController.SetChannelByte(newChannel);
        }

        private void UpdateTemperaturePolling(bool isWorking)
        {
            if(!isWorking)
            {
                _zoneController.StopPollingHeater();
                _zoneController.StopPollingTemperature();
            }

            if(!_settings.IsPortOpen)
            {
                return;
            }

            if (isWorking)
            {
                _zoneController.StartPollingTemperature();
            }
        }

        private void PollingErrorEventHandler(string errorMessage)
        {
            MessageBox.Show(errorMessage, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        public void SetTemperatureHandler()
        {
            this._zoneController.StartPollingHeater(InputTemperature);
        }

        public void Dispose()
        {
            this._zoneController?.Dispose();
        }
    }

}
