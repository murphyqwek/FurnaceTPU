using FurnaceCore.Model;
using FurnaceWPF.ViewModels;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace FurnaceWPF.Models.Controllers.Driver
{
    public class RotationController : BaseObservable
    {
        private DriverModule _driverModule;
        private Settings _settings;
        private ILogger<RotationController> _logger;
        private bool _isPollingRotation;
        private CancellationTokenSource _pollingCts;

        public event Action<string> RotationErrorEvent;

        private event Action<RotationData> RotationDataUpdate;

        public bool IsPollingRotation
        {
            get => _isPollingRotation;

            set
            {
                if(value != _isPollingRotation)
                {
                    _isPollingRotation = value;
                    OnPropertyChanged(nameof(IsPollingRotation));
                }
            }
        }

        public RotationController(DriverModule driverModule, Settings settings, ILogger<RotationController> logger)
        {
            _driverModule = driverModule;
            _settings = settings;
            _logger = logger;
        }

        public void StartPollingRotation()
        {
            if (IsPollingRotation) return;

            IsPollingRotation = true;
            _logger.LogInformation($"Начат опрос вращения шагового двигателя {_settings.RotationPollingInterval} мс. Таймаут чтения: {_settings.RotationTimeout} мс.");

            _pollingCts = new CancellationTokenSource();

            Task.Run(() => PollRotationLoop(_pollingCts.Token));
        }

        public void AddSubscriberToRotationUpdate(Action<RotationData> callback)
        {
            RotationDataUpdate += callback;

            StartPollingRotation();
        }

        public void RemoveSubscriberToRotationUpdate(Action<RotationData> callback)
        {
            RotationDataUpdate -= callback;

            if(RotationDataUpdate == null)
            {
                StopPollingRotation();
            }
        }

        public void StopPollingRotation()
        {
            if (!IsPollingRotation) return;
            _pollingCts?.Cancel();

            _logger.LogInformation("Опрос вращения шагового двигателя прекращен");
            IsPollingRotation = false;
        }

        private async Task PollRotationLoop(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    var result = await _driverModule.GetRotationDataAsync(_settings.RotationTimeout, token);

                    if(!result.Success)
                    {
                        _logger.LogWarning("Ошибка получение данных шагового двигателя: " + result.ErrorMessage);
                        Dispatcher.CurrentDispatcher.Invoke(() => RotationErrorEvent?.Invoke("Ошибка получение данных шагового двигателя: " + result.ErrorMessage));
                        return;
                    }

                    string resultString = "";

                    foreach(var rotation in result.Value.rotations)
                    {
                        resultString += $"{rotation.Key}:{rotation.Value}\n";
                    }


                    _logger.LogInformation("Получил данные по вращению: " + resultString);
                    RotationDataUpdate?.Invoke(result.Value);

                    await Task.Delay(_settings.RotationPollingInterval, token);
                }
                catch (TimeoutException)
                {
                    _logger.LogWarning($"Таймаут чтения данных шагового двигателя ({_settings.ZonePollingTimeout} мс) истек");
                    Dispatcher.CurrentDispatcher.Invoke(() => RotationErrorEvent?.Invoke($"Таймаут чтения данных шагового двигателя ({_settings.ZonePollingTimeout} мс) истек"));
                    break;
                }
                catch (TaskCanceledException) when (token.IsCancellationRequested)
                {
                    _logger.LogDebug("Опрос шагового двигателя отменён");
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Критическая ошибка при опросе шагового двигателя: " + ex.Message);
                    Dispatcher.CurrentDispatcher.Invoke(() => StopPollingRotation());
                    Dispatcher.CurrentDispatcher.Invoke(() => RotationErrorEvent?.Invoke("Критическая ошибка при опросе шагового двигателя (см. логги)"));
                    break;
                }
            }
        }

    }
}
