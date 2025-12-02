using FurnaceCore.Model;
using FurnaceCore.utlis;
using FurnaceWPF.Models.Controllers.Zone;
using FurnaceWPF.ViewModels;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace FurnaceWPF.Models.Controllers.Cooling
{
    public class CoolingConroller : BaseObservable
    {
        private double _currentTemperature;
        private bool _isWorking;
        private byte _channel;

        private TemperatureController _temperatureController;
        private CoolingModule _coolingModule;
        private ILogger<CoolingConroller> _logger;
        private Settings _settings;

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

        public CoolingConroller(TemperatureController temperatureController, CoolingModule coolingModule, ILogger<CoolingConroller> logger, Settings settings)
        {
            this._temperatureController = temperatureController;
            this._logger = logger;
            this._settings = settings;
            this._coolingModule = coolingModule;
            this._channel = _settings.CoolingChannel;
            this._temperatureController.GlobalErrorEvent += (e) => { StopPollingTemperature(); };
        }

        public void StartPollingTemperature()
        {
            if (IsWorking) return;

            IsWorking = true;

            _logger.LogInformation($"Начат опрос температуры с интервалом {_settings.ZonePollingInterval} мс. Таймаут чтения: {_settings.ZonePollingTimeout} мс.");

            _temperatureController.AddCaller(this._channel, new TemperatureEvent { reciveError = ErrorHandle, reciveTemperatue = TemperatureHandle });
        }

        public void StopPollingTemperature()
        {
            if (!IsWorking) return;

            IsWorking = false;
            OnPropertyChanged(nameof(IsWorking));

            _temperatureController.RemoveCaller(_channel);

            _logger.LogInformation($"Опрос температуры канала {this._channel} преркащён");
            IsWorking = false;

        }


        public void SetChannel(byte newChannel)
        {
            _temperatureController.ChangeChannel(this._channel, newChannel);
            _logger.LogInformation($"Канал холодильника изменён с {this._channel} на {newChannel}");
            this._channel = newChannel;
        }

        public void TemperatureHandle(double temperature)
        {
            CurrentTemperature = temperature;
        }

        public void ErrorHandle(string message)
        {
            _logger.LogError(message);
            StopPollingTemperature();

            MessageBox.Show(message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
