using FurnaceWPF.Models;
using FurnaceWPF.Models.Controllers.Cooling;
using FurnaceWPF.Models.Controllers.Out;
using FurnaceWPF.ViewModels;
using pechka4._8.Helpers;
using System;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace pechka4._8.ViewModels
{
    public class OutSystemVievModel : BaseObservable
    {
        #region Properties
        public string CoolantTemperature
        {
            get => $"{_controller.CurrentTemperature:0.#}°C";
        }

        public double CoolantTemperatureDouble
        {
            get => _controller.CurrentTemperature;
        }

        public bool IsPumpOn
        {
            get => _controller.IsWorking;
            set
            {
                if (value)
                {
                    _controller.StartPollingTemperature();
                }
                else
                {
                    _controller.StopPollingTemperature();
                }

                OnPropertyChanged();
            }
        }

        public bool IsEnabled
        {
            get => _settings.IsPortOpen;
        }

        public string Name { get => "Выходящие газы"; }

        public Brush CoolantBrush => InterpolateBrush(_controller.CurrentTemperature);
        #endregion

        private static Brush InterpolateBrush(double temp)
        {
            Color cold = Colors.Teal;
            Color hot = Colors.Red;
            byte r = (byte)(cold.R + (hot.R - cold.R) * temp / 100);
            byte g = (byte)(cold.G + (hot.G - cold.G) * temp / 100);
            byte b = (byte)(cold.B + (hot.B - cold.B) * temp / 100);
            return new SolidColorBrush(Color.FromRgb(r, g, b));
        }

        private OutConroller _controller;
        private Settings _settings;

        public OutSystemVievModel(OutConroller outConroller, Settings settings)
        {
            this._controller = outConroller;
            this._settings = settings;

            this._controller.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(OutConroller.CurrentTemperature))
                {
                    OnPropertyChanged(nameof(CoolantTemperature));
                    OnPropertyChanged(nameof(CoolantBrush));
                    OnPropertyChanged(nameof(CoolantTemperatureDouble));
                }

                if (e.PropertyName == nameof(OutConroller.IsWorking))
                {
                    OnPropertyChanged(nameof(IsPumpOn));
                }
            };



            _settings.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(Settings.IsPortOpen))
                {
                    OnPropertyChanged(nameof(IsEnabled));

                    if (!_settings.IsPortOpen)
                    {
                        _controller.StopPollingTemperature();
                    }
                }
            };
        }

        public void TurnOn()
        {
            _controller.StartPollingTemperature();
        }

        public void TurnOff()
        {
            _controller.StopPollingTemperature();
        }
    }
}