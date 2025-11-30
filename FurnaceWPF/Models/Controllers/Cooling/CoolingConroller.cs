using FurnaceCore.Model;
using FurnaceWPF.Models.Controllers.Zone;
using FurnaceWPF.ViewModels;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace FurnaceWPF.Models.Controllers.Cooling
{
    public class CoolingConroller : BaseObservable
    {
        private double _currentTemperature;
        private bool _isWorking;

        private TemperatureModule _temperatureModule;
        private CoolingModule _coolingModule;
        private ILogger<CoolingConroller> _logger;
        private Settings _settings;
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
        public bool IsWorking
        {
            get => _isWorking;
            private set
            {
                if (_isWorking != value)
                {
                    _isWorking = value;
                    OnPropertyChanged();
                }
            }
        }
        #endregion

        public event Action<string>? ErrorEvent;

        public CoolingConroller(TemperatureModule temperatureModule, CoolingModule coolingModule, ILogger<CoolingConroller> logger, Settings settings)
        {
            this._temperatureModule = temperatureModule;
            this._logger = logger;
            this._settings = settings;
            this._coolingModule = coolingModule;
        }

        public void StartPollingTemperature()
        {
            IsWorking = true;

            _coolingModule.TurnOnCooling();
            _logger.LogInformation($"Начат опрос температуры холодильника с интервалом {_settings.CoolingPollingTemperatureIntervall} мс. Таймаут чтения: {_settings.ZonePollingTimeout} мс.");

            _pollingCts = new CancellationTokenSource();

            Task.Run(() => PollTemperatureLoop(_pollingCts.Token));
        }

        public void StopPollingTemperature()
        {
            _coolingModule.TurnOffCooling();
            _pollingCts?.Cancel();
            _pollingCts = null;
            _logger.LogInformation($"Опрос температуры холодильника преркащён");
            IsWorking = false;
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
                        currentTemperature = await _temperatureModule.GetTemperatureAsync(_settings.CoolingPollingTimeout, token);
                        Dispatcher.CurrentDispatcher.Invoke(() => CurrentTemperature = currentTemperature);
                    }
                    catch (TimeoutException)
                    {
                        _logger.LogWarning($"Таймаут чтения температуры ({_settings.CoolingPollingTimeout} мс) истек");
                        Dispatcher.CurrentDispatcher.Invoke(StopPollingTemperature);
                        Dispatcher.CurrentDispatcher.Invoke(() => ErrorEvent?.Invoke($"Таймаут чтения температуры ({_settings.CoolingPollingTimeout} мс) истек"));

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


        public void SetAddressByte(byte newAddress)
        {
            _temperatureModule.SetAddressByte(newAddress);
        }
    }
}
