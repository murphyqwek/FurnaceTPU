using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace FurnaceWPF.Models
{
    public class Settings : INotifyPropertyChanged
    {
        private bool _isDebug = false;
        private byte _zoneOneAddress = 0x01;
        private byte _zoneTwoAddress = 0x01;
        private byte _zoneThreeAddress = 0x01;
        private bool _isRunning;

        #region Properties
        public bool IsDebug
        {
            get => _isDebug;

            set
            {
                if (_isDebug != value)
                {
                    _isDebug = value;
                    OnPropertyChanged();
                }
            }
        }

        public byte ZoneOneAddress
        {
            get => _zoneOneAddress;
            set
            {
                if (_zoneOneAddress != value)
                {
                    _zoneOneAddress = value;
                    OnPropertyChanged();
                }
            }
        }

        public byte ZoneTwoAddress
        {
            get => _zoneTwoAddress;
            set
            {
                if (_zoneTwoAddress != value)
                {
                    _zoneTwoAddress = value;
                    OnPropertyChanged();
                }
            }
        }

        public byte ZoneThreeAddress
        {
            get => _zoneThreeAddress;
            set
            {
                if (_zoneThreeAddress != value)
                {
                    _zoneThreeAddress = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool IsRunning
        {
            get => _isRunning;
            set
            {
                if (_isRunning != value)
                {
                    _isRunning = value;
                    OnPropertyChanged();
                }
            }
        }

        public ushort StepSizeDriver { get => 500; }
        public int DriverUpdateInterval { get => 100; } // Интервал указан в мс

        public int ZonePollingInterval { get => 200; } // Интервал опроса температуры в мс

        public int ZoneHeatCheckingInterval { get => 100; } //Интервал для проверки температуры нагрева

        public double ZoneTreshold { get => 10.0; } //Трешхолд для нагревателя (TargetValue - ZoneTreshold)

        public int ZonePollingTimeout { get => 5 * 1000; } //Таймаут для опроса температуры

        public int CoolingPollingTimeout { get => 5 * 1000; }

        public int CoolingPollingTemperatureIntervall { get => 200; }

        public bool IsPortOpen { get; set; }
        #endregion

        public Settings()
        {
            this._isRunning = false;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
