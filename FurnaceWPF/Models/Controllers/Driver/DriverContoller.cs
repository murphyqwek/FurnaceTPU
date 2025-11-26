using FurnaceCore.Model;
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
    public class DriverContoller : INotifyPropertyChanged
    {
        private Timer _rampingTimer;
        private const ushort STEP_SIZE = 500;
        private const int UPDATE_INTERVAL = 100; // Интервал 100 мс
        
        private ushort _targetFrequence;
        private DriverModule _driver;
        private int _channel;
        private DriversPortEnum _driversPort;
        private Settings _settings;

        public event PropertyChangedEventHandler? PropertyChanged;

        public ushort CurrentFrequency { get; private set; } = 0;
        public bool IsDriverRunning { get; private set; }

        public DriverContoller(DriverModule driver, int channel, DriversPortEnum driversPort, Settings settings)
        {
            this._driver = driver;
            this._channel = channel;
            this._driversPort = driversPort;
            this._settings = settings;
        }

        public void SetNewTarget(ushort newTarget)
        {
            Stop();
            this._targetFrequence = newTarget;

            this._driver.StartDriver(_driversPort);
            _rampingTimer = new Timer(RampingTick, null, UPDATE_INTERVAL, UPDATE_INTERVAL);
        }

        private void RampingTick(object? state)
        {
            ushort error = (ushort)(_targetFrequence - CurrentFrequency);

            if (Math.Abs(error) < STEP_SIZE)
            {
                CurrentFrequency = _targetFrequence;
                
            }
            else
            {
                CurrentFrequency = (ushort)(error < 0 ? CurrentFrequency - STEP_SIZE : CurrentFrequency + STEP_SIZE);
            }


            this._driver.SetDriverFrequency(this._channel, CurrentFrequency);
            App.Current.Dispatcher.Invoke(() => { OnPropertyChanged(nameof(CurrentFrequency)); });

            if(CurrentFrequency == _targetFrequence)
            {
                Stop();
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void Stop()
        {
            this._driver.StopDriver(this._driversPort);
            IsDriverRunning = false;
            _rampingTimer?.Change(Timeout.Infinite, Timeout.Infinite);
        }
    }
}
