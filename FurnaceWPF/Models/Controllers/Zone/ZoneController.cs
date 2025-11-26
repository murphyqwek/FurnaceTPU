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
using System.Windows.Threading;

namespace FurnaceWPF.Models.Controllers.Zone
{
    public class ZoneController : BaseObservable
    {
        private Timer _tempreaturePollingTimer;
        private Timer _heaterPollingTimer;

        private double _currentTemperature;
        private bool _isHeating;
        private bool _isPollingTemperature;

        private TemperatureModule _temperatureModule;
        private HeaterModule _heaterModule;
        private ILogger<ZoneController> _logger;
        private Settings _settings;
        private double _targetTemperature;
        private CancellationTokenSource _pollingCts;

        #region Properties
        public double CurrentTemperature 
        { 
            get => _currentTemperature;
            private set
            {
                if (_currentTemperature != value)
                {
                    _currentTemperature = value;
                    OnPropertyChanged();
                }
            }
        }
        public bool IsHeating
        {
            get => _isHeating;
            private set
            {
                if (_isHeating != value)
                {
                    _isHeating = value;
                    OnPropertyChanged();
                }
            }
        }
        public bool IsPollingTemperature
        {
            get => _isPollingTemperature;
            private set
            {
                if (_isPollingTemperature != value)
                {
                    _isPollingTemperature = value;
                    OnPropertyChanged();
                }
            }
        }
        #endregion

        public event Action<string>? ErrorEvent;

        public ZoneController(TemperatureModule temperatureModule, HeaterModule heaterModule, ILogger<ZoneController> logger, Settings settings) 
        { 
            this._temperatureModule = temperatureModule;
            this._heaterModule = heaterModule;
            this._logger = logger;
            this._settings = settings;
            this.IsHeating = false;
            this.IsPollingTemperature = false;
        }

        public void StartPollingTemperature()
        {
            if (IsPollingTemperature) return;

            IsPollingTemperature = true;
            _logger.LogInformation($"Начат опрос температуры с интервалом {_settings.ZonePollingInterval} мс. Таймаут чтения: {_settings.ZonePollingTimeout} мс.");

            _pollingCts = new CancellationTokenSource();

            Task.Run(() => PollTemperatureLoop(_pollingCts.Token));
        }

        public void StopPollingTemperature()
        {
            if(!IsPollingTemperature) return;

            _pollingCts.Cancel();
            _logger.LogInformation($"Опрос температуры преркащён");
            IsPollingTemperature = false;
            
        }

        private async Task PollTemperatureLoop(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    double currentTemperature = 0;

                    try
                    {
                        currentTemperature = await _temperatureModule.GetTemperatureAsync(_settings.ZonePollingTimeout, token);
                        Dispatcher.CurrentDispatcher.Invoke(() => CurrentTemperature = currentTemperature);
                    }
                    catch (TimeoutException)
                    {
                        _logger.LogWarning($"Таймаут чтения температуры ({_settings.ZonePollingTimeout} мс) истек");
                        Dispatcher.CurrentDispatcher.Invoke(() => StopPollingTemperature());
                        Dispatcher.CurrentDispatcher.Invoke(() => ErrorEvent?.Invoke($"Таймаут чтения температуры ({_settings.ZonePollingTimeout} мс) истек"));
                        
                        break;
                    }

                    await Task.Delay(_settings.ZonePollingInterval, token);
                }
                catch (TaskCanceledException) when (token.IsCancellationRequested)
                {
                    _logger.LogDebug("Опрос температуры отменен");
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Критическая ошибка при опросе температуры. " + ex.Message);
                    Dispatcher.CurrentDispatcher.Invoke(() => StopPollingTemperature());
                    Dispatcher.CurrentDispatcher.Invoke(() => ErrorEvent?.Invoke("Критическая ошибка при опросе температуры (см. логги)"));
                    break;
                }
            }
        }
    

        public void StartPollingHeater(double targetTemperature)
        {
            this.IsHeating = true;
            _targetTemperature = targetTemperature;
            _logger.LogInformation($"Начат нагрев с интервалом {_settings.ZoneHeatCheckingInterval} мс до {_targetTemperature}");
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

        public void SetAddressByte(byte newAddress)
        {
            _temperatureModule.SetAddressByte(newAddress);
        }

    }

}
