using FurnaceCore.Model;
using Microsoft.Extensions.Logging;
using pechka4._8;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.RightsManagement;
using System.Text;
using System.Threading.Tasks;

namespace FurnaceWPF.Models.Controllers
{
    public class DriverContoller : INotifyPropertyChanged, IDisposable
    {
        private Timer _rampingTimer;

        private ushort _targetFrequence;
        private ushort _savedTarget;
        private DriverModule _driver;
        private Func<int> _channel;
        private Func<DriversPortEnum> _driversPort;
        private Settings _settings;
        private ILogger<DriverContoller> _logger;
        public event PropertyChangedEventHandler? PropertyChanged;
        private CancellationTokenSource _directionCts;


        public ushort CurrentFrequency { get; private set; } = 10_000;
        public bool IsDriverRunning { get; private set; }

        public DriverContoller(DriverModule driver, Func<int> channel, Func<DriversPortEnum> driversPort, Settings settings, ILogger<DriverContoller> logger)
        {
            this._driver = driver;
            this._channel = channel;
            this._driversPort = driversPort;
            this._settings = settings;
            this._logger = logger;
        }

        public void SetNewTarget(ushort newTarget)
        {
            Stop();
            this._targetFrequence = newTarget;


            _logger.LogInformation($"Запускаем шаговый двигатель на порту {_driversPort}");
            this._driver.StartDriver(_driversPort());
            _rampingTimer = new Timer(RampingTick, null, _settings.DriverRampingUpdateInterval, _settings.DriverRampingUpdateInterval);
        }

        private void RampingTick(object? state)
        {
            int error = _targetFrequence - CurrentFrequency;

            var stepSize = _settings.StepSizeDriver;

            if (Math.Abs(error) < stepSize)
            {
                CurrentFrequency = _targetFrequence;

            }
            else
            {
                CurrentFrequency = (ushort)(error < 0 ? CurrentFrequency - stepSize : CurrentFrequency + stepSize);
            }


            this._driver.SetDriverFrequency(this._channel(), CurrentFrequency);
            App.Current.Dispatcher.Invoke(() => { OnPropertyChanged(nameof(CurrentFrequency)); });

            if (CurrentFrequency == _targetFrequence)
            {
                _logger.LogInformation($"Шаговый двигатель на порту {_driversPort} разогнался до нужной скорости");
                _rampingTimer?.Change(Timeout.Infinite, Timeout.Infinite);
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void Start()
        {
            _logger.LogInformation($"Запускаем шаговый двигатель на порту {_driversPort}");
            this._driver.StartDriver(_driversPort());

            _logger.LogInformation($"Останавливаем шаговый двигатель на порту {_driversPort}");
            this.CurrentFrequency = 4;
            this._driver.SetDriverFrequency(_channel(), CurrentFrequency);
            OnPropertyChanged(nameof(CurrentFrequency));
        }

        public void Stop()
        {
            _logger.LogInformation($"Останавливаем шаговый двигатель на порту {_driversPort}");
            this._driver.StopDriver(this._driversPort());
            IsDriverRunning = false;
            _rampingTimer?.Change(Timeout.Infinite, Timeout.Infinite);
        }

        public void Dispose()
        {
            _rampingTimer?.Dispose();
            _rampingTimer = null;
        }

        public DriversPortEnum GetDriversPort()
        {
            return _driversPort();
        }

        public async Task OnDirectionChanged()
        {
            _logger.LogInformation($"Меняем направление шд: {this._channel}");

            _directionCts?.Cancel();
            _directionCts = new CancellationTokenSource();
            var ct = _directionCts.Token;

            _savedTarget = _targetFrequence;

            // немедленно останавливаем драйвер
            _driver.StopDriver(_driversPort());

            try
            {
                await Task.Delay(500, ct);
            }
            catch (TaskCanceledException)
            {
                return;
            }

            SetNewTarget(_savedTarget);
        }

    }
}
