using FurnaceCore.Model;
using FurnaceWPF.ViewModels;
using FurnaceWPF.Views.Controls;
using Microsoft.Extensions.Logging;
using pechka4._8;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Converters;

namespace FurnaceWPF.Models.Controllers.Zone
{
    public class ZoneController : BaseObservable
    {
        private Timer _tempreaturePollingTimer;
        private Timer _heaterPollingTimer;

        private TemperatureModule _temperatureModule;
        private HeaterModule _heaterModule;
        private ILogger<ZoneController> _logger;
        private Settings _settings;
        private double _targetTemperature;
        
        public double CurrentTemperature { get; private set; }
        public bool IsHeating { get; private set; }
        public bool IsPollingTemperature { get; private set; }
        

        public ZoneController(TemperatureModule temperatureModule, HeaterModule heaterModule, ILogger<ZoneController> logger, Settings settings) 
        { 
            this._temperatureModule = temperatureModule;
            this._heaterModule = heaterModule;
            this._logger = logger;
            this._settings = settings;
            this.IsHeating = false;
            this.IsPollingTemperature = false;
        }

        public void StartPollingTemperature(double targetTemperature)
        {
            this.IsPollingTemperature = true;
            _targetTemperature = targetTemperature;
            _logger.LogInformation($"Начат опрос датчиков с интервалом {_settings.ZonePollingInterval} мс до {targetTemperature} градусов");
            _tempreaturePollingTimer = new Timer(PollTemperature, null, 0, _settings.ZonePollingInterval);
        }

        public void StopPollingTemperature()
        {
            this.IsPollingTemperature = false;
            _tempreaturePollingTimer?.Change(Timeout.Infinite, Timeout.Infinite);
            _logger.LogInformation("Опрос датчиков остановлен");
        }

        private void PollTemperature(object? o)
        {
            double currentTemperature = _temperatureModule.GetTemperatureAsync().Result;
            CurrentTemperature = currentTemperature;

            App.Current.Dispatcher.Invoke(() => OnPropertyChanged(nameof(CurrentTemperature)));
        }

        public void StartPollingHeater()
        {
            this.IsHeating = true;
            _logger.LogInformation($"Начат нагрев с интервалом {_settings.ZoneHeatCheckingInterval} мс");
            _heaterModule.TurnOnHeater();
            _heaterPollingTimer = new Timer(PollHeater, null, 0, _settings.ZoneHeatCheckingInterval);
        }

        public void StopPollingHeater()
        {
            this.IsHeating = false;
            _heaterModule.TurnOffHeater();
            _heaterPollingTimer?.Change(Timeout.Infinite, Timeout.Infinite);
            _logger.LogInformation("Нагрев остановлен");
        }

        private void PollHeater(object? o)
        {
            if(CurrentTemperature - _settings.ZoneTreshold <= _targetTemperature)
            {
                _logger.LogInformation($"Температура прошла порог, дальше нагрев до {_targetTemperature} градусов пойдёт по инецрии");
                StopPollingHeater();
                return;
            }

            _logger.LogInformation($"Температура не прошла порог, продолжаем нагрев");
        }

    }

}
