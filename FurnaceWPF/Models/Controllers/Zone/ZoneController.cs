using FurnaceCore.Model;
using FurnaceWPF.ViewModels;
using FurnaceWPF.Views.Controls;
using Microsoft.Extensions.Logging;
using pechka4._8;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media.Converters;
using System.Windows.Threading;

namespace FurnaceWPF.Models.Controllers.Zone
{
    public class ZoneController : BaseObservable, IDisposable
    {
        private Timer _heaterPollingTimer;
        private bool _heatModuleStatus = false;
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
                        _logger.LogInformation($"Текущая температура: {currentTemperature}");
                        Dispatcher.CurrentDispatcher.Invoke(() => CurrentTemperature = currentTemperature);
                    }
                    catch (TimeoutException)
                    {
                        _logger.LogWarning($"Таймаут чтения температуры ({_settings.ZonePollingTimeout} мс) истек");
                        Dispatcher.CurrentDispatcher.Invoke(StopPollingTemperature);
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
            this._heatModuleStatus = false;
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
            try
            {
                if (this.IsPollingTemperature)
                {
                    _logger.LogInformation("Опрос температуры остановлен, прекращаем нагрев");
                    Dispatcher.CurrentDispatcher.Invoke(StopPollingHeater);
                    return;
                }

                if (CurrentTemperature < _targetTemperature + _settings.ZoneTreshold && CurrentTemperature > _targetTemperature - _settings.ZoneTreshold)
                {
                    _logger.LogInformation("Текущая температура в допустимых пределах заданной");
                    _heaterModule.TurnOffHeater();
                    this._heatModuleStatus = false;
                }
                else
                {
                    HeatOrCold();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                this._heatModuleStatus = false;
                _heaterPollingTimer?.Change(Timeout.Infinite, Timeout.Infinite);
                _logger.LogInformation("Нагрев остановлен");
            }
        }

        private void HeatOrCold()
        {
            if(_targetTemperature - _settings.ZoneTreshold >= CurrentTemperature)
            {
                _logger.LogInformation($"Текущая температура ниже установленной. Идёт нагрев");
                if (!this._heatModuleStatus)
                {
                    _heaterModule.TurnOnHeater();
                    this._heatModuleStatus = true;
                }
            }

            if (_targetTemperature + _settings.ZoneTreshold <= CurrentTemperature)
            {
                _logger.LogInformation($"Текущая температура выше установленной. Идёт охлаждение");
                if (this._heatModuleStatus)
                {
                    _heaterModule.TurnOffHeater();
                    this._heatModuleStatus = false;
                }
            }
        }

        public void SetAddressByte(byte newAddress)
        {
            _temperatureModule.SetAddressByte(newAddress);
        }

        public void Dispose()
        {
            _pollingCts?.Cancel();
            _pollingCts?.Dispose();
            _heaterPollingTimer?.Dispose();
            _heaterPollingTimer = null;
        }
    }

}
