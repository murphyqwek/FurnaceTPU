using FurnaceWPF.ViewModels;
using pechka4._8.Helpers;
using System;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace pechka4._8.ViewModels
{
    public class CoolingSystemViewModel : BaseObservable
    {
        private double _temperature;
        private bool _isPumpOn;

        public double CoolantTemperature
        {
            get => _temperature;
            set
            {
                _temperature = value;
                OnPropertyChanged(nameof(CoolantBrush));
            }
        }

        public bool IsPumpOn
        {
            get => _isPumpOn;
            set => _isPumpOn = value;
        }

        public Brush CoolantBrush => InterpolateBrush(CoolantTemperature);

        public ICommand TogglePumpCommand => new RelayCommand(_ => IsPumpOn = !IsPumpOn);

        private static Brush InterpolateBrush(double temp)
        {
            Color cold = Colors.Teal;
            Color hot = Colors.Red;
            byte r = (byte)(cold.R + (hot.R - cold.R) * temp / 100);
            byte g = (byte)(cold.G + (hot.G - cold.G) * temp / 100);
            byte b = (byte)(cold.B + (hot.B - cold.B) * temp / 100);
            return new SolidColorBrush(Color.FromRgb(r, g, b));
        }
    }
}