using FurnaceCore.Model;
using FurnaceCore.utlis;
using FurnaceWPF.ViewModels;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.RightsManagement;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace FurnaceWPF.Models.Controllers.Zone
{
    public struct TemperatureEvent
    {
        public Action<double> reciveTemperatue;
        public Action<string> reciveError;
    }

    public class TemperatureController : BaseObservable
    {
        private TemperatureModule _temperatureModule;
        private Settings _settings;
        private Dictionary<byte, TemperatureEvent> _callers;
        private ILogger<TemperatureController> _logger;
        private CancellationTokenSource _pollingCts;

        public bool IsPollingTemperature { get; private set; }
        public event Action<string> ErrorEvent;

        public TemperatureController(TemperatureModule temperatureModule, Settings settings, ILogger<TemperatureController> logger)
        {
            this._temperatureModule = temperatureModule;
            this._settings = settings;
            this._logger = logger;
            this._callers = new Dictionary<byte, TemperatureEvent>();
        }

        public void StartPollingTemperature()
        {
            if (IsPollingTemperature) return;

            IsPollingTemperature = true;
            OnPropertyChanged(nameof(IsPollingTemperature));
            _logger.LogInformation($"Начат опрос температуры с интервалом {_settings.ZonePollingInterval} мс. Таймаут чтения: {_settings.ZonePollingTimeout} мс.");

            _pollingCts = new CancellationTokenSource();

            Task.Run(() => PollTemperatureLoop(_pollingCts.Token));
        }

        public void StopPollingTemperature()
        {
            if (!IsPollingTemperature) return;

            _pollingCts.Cancel();
            _logger.LogInformation($"Опрос температуры преркащён");
            IsPollingTemperature = false;

        }

        private async Task PollTemperatureLoop(CancellationToken token)
        {
            int errorCount = 0;
            while (!token.IsCancellationRequested)
            {
                try
                {
                    foreach(var channel in _callers.Keys)
                    {
                        await PollingTemeperature(channel, token);
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

        private async Task PollingTemeperature(byte channel, CancellationToken token)
        {
            this._temperatureModule.SetChannelByte(channel);
            var temperatureActions = this._callers.GetValueOrDefault(channel);
            int errorCount = 0;

            var reciveTemperature = temperatureActions.reciveTemperatue;
            var reciveError = temperatureActions.reciveError;

            try
            {
                var currentTemperature = await _temperatureModule.GetTemperatureAsync(_settings.ZonePollingTimeout, token);

                if (currentTemperature.Success)
                {
                    _logger.LogInformation($"Текущая температура: {currentTemperature.Value}");
                    Dispatcher.CurrentDispatcher.Invoke(() => reciveTemperature(currentTemperature.Value));
                    errorCount = 0;
                }
                else
                {
                    errorCount++;
                    _logger.LogWarning($"Произошла ошибка ({errorCount}) получния данных темепратуры: {currentTemperature.ErrorMessage}");

                    if (errorCount == 5)
                    {
                        _logger.LogError($"Прошёл порог ошибок получения данных. Прекращаем опрос");
                        Dispatcher.CurrentDispatcher.Invoke(StopPollingTemperature);
                        Dispatcher.CurrentDispatcher.Invoke(() => reciveError($"Модуль температуры получает данные с ошибками"));
                        return;
                    }

                    _logger.LogWarning("Попытка получить данные ещё раз");
                }
            }
            catch (TimeoutException)
            {
                _logger.LogWarning($"Таймаут чтения температуры ({_settings.ZonePollingTimeout} мс) истек");
                Dispatcher.CurrentDispatcher.Invoke(StopPollingTemperature);
                Dispatcher.CurrentDispatcher.Invoke(() => reciveError($"Таймаут чтения температуры ({_settings.ZonePollingTimeout} мс) истек"));

                return;
            }

        }

        public void ChangeChannel(byte oldChannel, byte newChannel)
        {
            var caller = this._callers.GetValueOrDefault(oldChannel);

            this._callers.Remove(oldChannel);
            this._callers.Add(newChannel, caller);
        }

        public void AddCaller(byte channel, TemperatureEvent caller)
        {
            this._callers.Add(channel, caller);
        }
    }
}
