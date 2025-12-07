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
    public enum DriverMotionState
    {
        Stopped,
        RampingUp,
        RampingDown,
        Running
    }

    public class DriverContoller : INotifyPropertyChanged, IDisposable
    {
        private Timer _rampingTimer;
        
        private ushort _targetFrequency;
        private ushort _requestedTargetFrequency;
        private DriverModule _driver;
        private Func<int> _channel;
        private Func<DriversPortEnum> _driversPort;
        private Settings _settings;
        private ILogger<DriverContoller> _logger;
        private DriverMotionState _state = DriverMotionState.Stopped;
        public event PropertyChangedEventHandler? PropertyChanged;

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
            _logger.LogInformation($"Новая целевая частота: {newTarget}");

            _requestedTargetFrequency = newTarget;

            if (newTarget == 0)
            {
                StartRampingDown();
                return;
            }

            // если мы стоим или тормозим — просто разгоняемся
            if (CurrentFrequency == 0)
            {
                StartRampingUp(newTarget);
                return;
            }

            // если мы уже едем — аккуратно меняем цель
            _targetFrequency = newTarget;
            StartRampingTimer();
        }
        private void RampingTick(object? state)
        {
            int error = _targetFrequency - CurrentFrequency;
            int step = _settings.StepSizeDriver;

            if (Math.Abs(error) <= step)
                CurrentFrequency = _targetFrequency;
            else
                CurrentFrequency = (ushort)(CurrentFrequency + Math.Sign(error) * step);

            _driver.SetDriverFrequency(_channel(), CurrentFrequency);

            App.Current.Dispatcher.Invoke(() =>
                OnPropertyChanged(nameof(CurrentFrequency)));

            if (CurrentFrequency == _targetFrequency)
                OnRampFinished();
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
            _state = DriverMotionState.Stopped;
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

        public void OnDirectionChanged()
        {
            _logger.LogInformation("Направление изменилось — тормозим");

            if (CurrentFrequency == 0)
            {
                if (_requestedTargetFrequency > 0)
                    StartRampingUp(_requestedTargetFrequency);

                return;
            }

            StartRampingDown();
        }

        private void StartRampingDown()
        {
            _state = DriverMotionState.RampingDown;
            _targetFrequency = 0;
            StartRampingTimer();
        }


        private void StartRampingUp(ushort target)
        {
            _state = DriverMotionState.RampingUp;
            _targetFrequency = target;
            StartRampingTimer();
        }

        private void OnRampFinished()
        {
            _rampingTimer?.Change(Timeout.Infinite, Timeout.Infinite);

            if (_state == DriverMotionState.RampingDown)
            {
                _state = DriverMotionState.Stopped;

                // направление уже сменилось извне — можно разгоняться
                if (_requestedTargetFrequency > 0)
                {
                    StartRampingUp(_requestedTargetFrequency);
                }
                return;
            }

            if (_state == DriverMotionState.RampingUp)
            {
                _state = DriverMotionState.Running;
            }
        }


        private void StartRampingTimer()
        {
            if (_rampingTimer == null)
            {
                _rampingTimer = new Timer(
                    RampingTick,
                    null,
                    _settings.DriverRampingUpdateInterval,
                    _settings.DriverRampingUpdateInterval);
            }
            else
            {
                _rampingTimer.Change(
                    _settings.DriverRampingUpdateInterval,
                    _settings.DriverRampingUpdateInterval);
            }
        }
    }
}
