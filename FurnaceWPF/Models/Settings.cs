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
        private byte _zoneOneChannel = 0x00;
        private byte _zoneTwoChannel = 0x01;
        private byte _zoneThreeChannel = 0x02;
        private byte _zoneHeaterOneChannel = 0x00;
        private byte _zoneTHeaterwoChannel = 0x01;
        private byte _zoneHeaterThreeChannel = 0x02;
        private byte _coolingChannel = 0x03;
        private bool _isRunning;
        private bool _isPortOpen = false;

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

        public byte ZoneOneChannel
        {
            get => _zoneOneChannel;
            set
            {
                if (_zoneOneChannel != value)
                {
                    _zoneOneChannel = value;
                    OnPropertyChanged();
                }
            }
        }

        public byte ZoneTwoChannel
        {
            get => _zoneTwoChannel;
            set
            {
                if (_zoneTwoChannel != value)
                {
                    _zoneTwoChannel = value;
                    OnPropertyChanged();
                }
            }
        }

        public byte ZoneThreeChannel
        {
            get => _zoneThreeChannel;
            set
            {
                if (_zoneThreeChannel != value)
                {
                    _zoneThreeChannel = value;
                    OnPropertyChanged();
                }
            }
        }


        public byte ZoneHeaterOneChannel
        {
            get => _zoneHeaterOneChannel;
            set
            {
                if (_zoneHeaterOneChannel != value)
                {
                    _zoneHeaterOneChannel = value;
                    OnPropertyChanged();
                }
            }
        }

        public byte ZoneHeaterTwoChannel
        {
            get => _zoneHeaterTwoChannel;
            set
            {
                if (_zoneHeaterTwoChannel != value)
                {
                    _zoneHeaterTwoChannel = value;
                    OnPropertyChanged();
                }
            }
        }

        public byte ZoneHeaterThreeChannel
        {
            get => _zoneHeaterThreeChannel;
            set
            {
                if (_zoneHeaterThreeChannel != value)
                {
                    _zoneHeaterThreeChannel = value;
                    OnPropertyChanged();
                }
            }
        }

        public byte CoolingChannel
        {
            get => _coolingChannel;
            set
            {
                if (_coolingChannel != value)
                {
                    _coolingChannel = value;
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

        public int ZonePollingInterval { get => 1000; } // Интервал опроса температуры в мс

        public int ZoneHeatCheckingInterval { get => 5000; } //Интервал для проверки температуры нагрева

        public double ZoneTreshold { get => 10.0; } //Трешхолд для нагревателя (TargetValue - ZoneTreshold)

        public int ZonePollingTimeout { get => 5 * 1000; } //Таймаут для опроса температуры

        public int CoolingPollingTimeout { get => 5 * 1000; }

        public int CoolingPollingTemperatureIntervall { get => 200; }

        public bool IsPortOpen 
        { 
            get => _isPortOpen;
            set
            {
                if (_isPortOpen != value)
                {
                    _isPortOpen = value;
                    OnPropertyChanged();
                }
            }
        }
        #endregion

        public Settings()
        {
            this._isRunning = false;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
