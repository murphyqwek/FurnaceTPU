using FurnaceCore.Model;
using FurnaceCore.Port;
using FurnaceWPF.Commands;
using FurnaceWPF.Models;
using FurnaceWPF.ViewModels;
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
    public class ZoneViewModel : BaseObservable
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

        private TemperatureModule temperatureModule;

        public RemoteCommand remoteCommand { get; }

        public ZoneViewModel(string name, double initialTemperature, TemperatureModule temperatureModule)
        {
            var port = App.Services.GetRequiredService<IPort>();
            this.temperatureModule = temperatureModule;
            Name = name;
            Temperature = initialTemperature;

            this.temperatureModule.OnTemperatureGet += TemperatureModule_OnTemperatureGet;
        }

        private void TemperatureModule_OnTemperatureGet(double temperature)
        {
            Application.Current.Dispatcher.Invoke(() => { Temperature = temperature; });
        }

        public void UpdateZoneAddress(byte newAddress)
        {
            this.temperatureModule.SetAddressByte(newAddress);
        }
    }

}
