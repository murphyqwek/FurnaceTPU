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
        private Dictionary<byte, CancellationTokenSource> _channelCtsDict; // Новый словарь
        private ILogger<TemperatureController> _logger;
        private CancellationTokenSource _pollingCts;

        public bool IsPollingTemperature { get; private set; }
        public event Action<string> GlobalErrorEvent;

        public TemperatureController(TemperatureModule temperatureModule, Settings settings, ILogger<TemperatureController> logger)
        {
            this._temperatureModule = temperatureModule;
            this._settings = settings;
            this._logger = logger;
            this._callers = new Dictionary<byte, TemperatureEvent>();
            this._channelCtsDict = new Dictionary<byte, CancellationTokenSource>();
        }

        private async Task PollTemperatureLoop(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    byte[] channels = new byte[_callers.Keys.Count];
                    _callers.Keys.CopyTo(channels, 0);

                    var pollingTasks = channels.Select(channel =>
                        PollingTemeperature(channel, GetChannelToken(channel))
                    ).ToArray();

                    await Task.WhenAll(pollingTasks);

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
                    Dispatcher.CurrentDispatcher.Invoke(() => GlobalErrorEvent?.Invoke("Критическая ошибка при опросе температуры (см. логги)"));
                    break;
                }
            }
        }

        private async Task PollingTemeperature(byte channel, CancellationToken token)
        {
            if (!_callers.ContainsKey(channel) || token.IsCancellationRequested)
            {
                _logger.LogDebug($"Опрос канала {channel} пропущен - канал удален или отменен");
                return;
            }

            this._temperatureModule.SetChannelByte(channel);
            var temperatureActions = this._callers.GetValueOrDefault(channel);

            var reciveTemperature = temperatureActions.reciveTemperatue;
            var reciveError = temperatureActions.reciveError;

            _logger.LogDebug($"Считываем температуру по каналу: {channel}");

            try
            {
                var currentTemperature = await _temperatureModule.GetTemperatureAsync(
                    _settings.ZonePollingTimeout,
                    token
                );

                if (!_callers.ContainsKey(channel))
                {
                    _logger.LogDebug($"Канал {channel} был удален во время опроса");
                    return;
                }

                if (currentTemperature.Success)
                {
                    _logger.LogInformation($"Текущая температура для канала {channel}: {currentTemperature.Value}");
                    Dispatcher.CurrentDispatcher.Invoke(() => reciveTemperature?.Invoke(currentTemperature.Value));
                }
                else
                {
                    _logger.LogWarning($"Произошла ошибка получения данных температуры для канала {channel}: {currentTemperature.ErrorMessage}");
                    Dispatcher.CurrentDispatcher.Invoke(() => reciveError?.Invoke(currentTemperature.ErrorMessage));
                }
            }
            catch (OperationCanceledException) when (token.IsCancellationRequested)
            {
                _logger.LogDebug($"Опрос канала {channel} был отменен");
            }
            catch (TimeoutException)
            {
                if (!_callers.ContainsKey(channel))
                {
                    return;
                }
                _logger.LogWarning($"Таймаут чтения температуры для канала {channel} ({_settings.ZonePollingTimeout} мс) истек");
                Dispatcher.CurrentDispatcher.Invoke(() => reciveError?.Invoke($"Таймаут чтения температуры ({_settings.ZonePollingTimeout} мс) истек"));
            }
        }

        private CancellationToken GetChannelToken(byte channel)
        {
            if (!_channelCtsDict.TryGetValue(channel, out var cts))
            {
                cts = new CancellationTokenSource();
                _channelCtsDict[channel] = cts;
            }
            return cts.Token;
        }

        public void AddCaller(byte channel, TemperatureEvent caller)
        {
            this._callers.Add(channel, caller);

            if (!_channelCtsDict.ContainsKey(channel))
            {
                _channelCtsDict[channel] = new CancellationTokenSource();
            }

            if (!this.IsPollingTemperature)
            {
                StartPollingTemperature();
            }
        }

        public void RemoveCaller(byte channel)
        {
            if (_channelCtsDict.TryGetValue(channel, out var cts))
            {
                cts.Cancel();
                cts.Dispose();
                _channelCtsDict.Remove(channel);
                _logger.LogDebug($"CTS для канала {channel} отменен и удален");
            }
            this._callers.Remove(channel);

            if (this._callers.Count == 0)
            {
                StopPollingTemperature();
            }
        }

        public void ChangeChannel(byte oldChannel, byte newChannel)
        {
            if (_callers.TryGetValue(oldChannel, out var caller))
            {
                RemoveCaller(oldChannel);

                AddCaller(newChannel, caller);
            }
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
            _pollingCts?.Cancel();

            foreach (var cts in _channelCtsDict.Values)
            {
                cts.Cancel();
                cts.Dispose();
            }
            _channelCtsDict.Clear();

            _logger.LogInformation("Опрос температуры прекращен");
            IsPollingTemperature = false;
            OnPropertyChanged(nameof(IsPollingTemperature));
        }
    }
}
