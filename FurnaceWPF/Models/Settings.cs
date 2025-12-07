using FurnaceCore.Model;
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
        private byte _zoneHeaterTwoChannel = 0x01;
        private byte _zoneHeaterThreeChannel = 0x02;

        private byte _driverAChannel = 0x00;
        private byte _driverBChannel = 0x02;
        private byte _driverCChannel = 0x04;

        private byte _driverAddress = 0x01;
        private byte _coolingChannel = 0x04;
        private byte _outChannel = 0x03;

        private bool _isRunning = false;
        private bool _isPortOpen = false;

        private ushort _stepSizeDriver = 500;
        private int _driverRampingUpdateInterval = 200;
        private int _zonePollingInterval = 1000;

        private double _zonePollingCoeff = 0.8;
        private int _zoneHeatCheckingInterval = 3300;
        private double _zoneTreshold = 10.0;

        private int _zonePollingTimeout = 15 * 1000;
        private int _coolingPollingTimeout = 15 * 1000;
        private int _coolingPollingTemperatureIntervall = 3300;

        private int _rotationTimeout = 15 * 1000;
        private int _rotationPollingInterval = 500;


        private DriversPortEnum _driverAPort = DriversPortEnum.Zero;
        private DriversPortEnum _driverBPort = DriversPortEnum.One;
        private DriversPortEnum _driverCPort = DriversPortEnum.Three;

        public const string SETTINGS_WINDOW_PASSOWRD = "Bestpirolisis";

        #region Properties

        public bool IsDebug
        {
            get => _isDebug;
            set => SetField(ref _isDebug, value);
        }

        public byte ZoneOneChannel
        {
            get => _zoneOneChannel;
            set => SetField(ref _zoneOneChannel, value);
        }

        public byte ZoneTwoChannel
        {
            get => _zoneTwoChannel;
            set => SetField(ref _zoneTwoChannel, value);
        }

        public byte ZoneThreeChannel
        {
            get => _zoneThreeChannel;
            set => SetField(ref _zoneThreeChannel, value);
        }

        public byte ZoneHeaterOneChannel
        {
            get => _zoneHeaterOneChannel;
            set => SetField(ref _zoneHeaterOneChannel, value);
        }

        public byte ZoneHeaterTwoChannel
        {
            get => _zoneHeaterTwoChannel;
            set => SetField(ref _zoneHeaterTwoChannel, value);
        }

        public byte ZoneHeaterThreeChannel
        {
            get => _zoneHeaterThreeChannel;
            set => SetField(ref _zoneHeaterThreeChannel, value);
        }

        public byte DriverAChannel
        {
            get => _driverAChannel;
            set => SetField(ref _driverAChannel, value);
        }

        public byte DriverBChannel
        {
            get => _driverBChannel;
            set => SetField(ref _driverBChannel, value);
        }

        public byte DriverCChannel
        {
            get => _driverCChannel;
            set => SetField(ref _driverCChannel, value);
        }

        public byte DriverAddress
        {
            get => _driverAddress;
            set => SetField(ref _driverAddress, value);
        }

        public byte CoolingChannel
        {
            get => _coolingChannel;
            set => SetField(ref _coolingChannel, value);
        }

        public byte OutChannel
        {
            get => _outChannel;
            set => SetField(ref _outChannel, value);
        }

        public bool IsRunning
        {
            get => _isRunning;
            set => SetField(ref _isRunning, value);
        }

        public bool IsPortOpen
        {
            get => _isPortOpen;
            set => SetField(ref _isPortOpen, value);
        }

        public ushort StepSizeDriver
        {
            get => _stepSizeDriver;
            set => SetField(ref _stepSizeDriver, value);
        }

        public int DriverRampingUpdateInterval
        {
            get => _driverRampingUpdateInterval;
            set => SetField(ref _driverRampingUpdateInterval, value);
        }

        public int ZonePollingInterval
        {
            get => _zonePollingInterval;
            set => SetField(ref _zonePollingInterval, value);
        }

        public double ZonePollingCoeff
        {
            get => _zonePollingCoeff;
            set => SetField(ref _zonePollingCoeff, value);
        }

        public int ZoneHeatCheckingInterval
        {
            get => _zoneHeatCheckingInterval;
            set => SetField(ref _zoneHeatCheckingInterval, value);
        }

        public double ZoneTreshold
        {
            get => _zoneTreshold;
            set => SetField(ref _zoneTreshold, value);
        }

        public int ZonePollingTimeout
        {
            get => _zonePollingTimeout;
            set => SetField(ref _zonePollingTimeout, value);
        }

        public int CoolingPollingTimeout
        {
            get => _coolingPollingTimeout;
            set => SetField(ref _coolingPollingTimeout, value);
        }

        public int CoolingPollingTemperatureIntervall
        {
            get => _coolingPollingTemperatureIntervall;
            set => SetField(ref _coolingPollingTemperatureIntervall, value);
        }

        public int RotationTimeout
        {
            get => _rotationTimeout;
            set => SetField(ref _rotationTimeout, value);
        }

        public int RotationPollingInterval
        {
            get => _rotationPollingInterval;
            set => SetField(ref _rotationPollingInterval, value);
        }


        public DriversPortEnum DriverAPort
        {
            get => _driverAPort;
            set => SetField(ref _driverAPort, value);
        }

        public DriversPortEnum DriverBPort
        {
            get => _driverBPort;
            set => SetField(ref _driverBPort, value);
        }

        public DriversPortEnum DriverCPort
        {
            get => _driverCPort;
            set => SetField(ref _driverCPort, value);
        }

        #endregion

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetField<T>(ref T field, T value, [CallerMemberName] string name = "")
        {
            if (Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(name);
            return true;
        }
    }
}
