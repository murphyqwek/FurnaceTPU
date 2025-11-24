using FurnaceCore.Model;
using FurnaceWPF.Commands;
using FurnaceWPF.Converters;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace pechka4._8.ViewModels
{
    public class ZoneViewModel : INotifyPropertyChanged
    {
        private double _temperature;
        public double Temperature
        {
            get => _temperature;
            set
            {
                // Ограничение диапазона и округление
                double val = Math.Max(0, Math.Min(700, Math.Round(value, 1)));
                if (_temperature != val)
                {
                    _temperature = val;
                    OnPropertyChanged(nameof(Temperature));
                    OnPropertyChanged(nameof(TemperatureText));
                    OnPropertyChanged(nameof(TempBrush));
                }
            }
        }

        public string TemperatureText => $"{Temperature:0.#}°C";

        public Brush TempBrush => InterpolateTempColor(Temperature);

        public string Name { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string prop)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }

        // Градиент между синим и жёлтым
        public static SolidColorBrush InterpolateTempColor(double temp)
        {
            Color cold = (Color)ColorConverter.ConvertFromString("#197692");
            Color hot = (Color)ColorConverter.ConvertFromString("#E8E26B");
            byte r = (byte)(cold.R + (hot.R - cold.R) * temp / 700);
            byte g = (byte)(cold.G + (hot.G - cold.G) * temp / 700);
            byte b = (byte)(cold.B + (hot.B - cold.B) * temp / 700);
            return new SolidColorBrush(Color.FromRgb(r, g, b));
        }

        private TemperatureModule temperatureModule;

        public RemoteCommand remoteCommand { get; }

        public ZoneViewModel(string name, double initialTemperature)
        {
            remoteCommand = new RemoteCommand(() => 
                { 
                    App.Services.GetRequiredService<MockPort>().ReceiveData("01 04 02 01 16 00 00"); 
                }
            );
            this.temperatureModule = App.Services.GetRequiredService<TemperatureModule>();
            Name = name;
            Temperature = initialTemperature;

            this.temperatureModule.OnTemperatureGet += TemperatureModule_OnTemperatureGet;
        }

        private void TemperatureModule_OnTemperatureGet(double temperature)
        {
            Application.Current.Dispatcher.Invoke(() => { Temperature = temperature; });
        }
    }

}
