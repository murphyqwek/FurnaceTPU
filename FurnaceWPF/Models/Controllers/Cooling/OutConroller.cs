using FurnaceCore.Model;
using FurnaceCore.utlis;
using FurnaceWPF.Helpers;
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

namespace FurnaceWPF.Models.Controllers.Out
{
    public class OutConroller : BaseObservable
    {
        private double _currentTemperature;
        private bool _isWorking;
        private Func<byte> _channel;

        private TemperatureController _temperatureController;
        private OutModule _OutModule;
        private ILogger<OutConroller> _logger;
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

        public OutConroller(TemperatureController temperatureController, OutModule OutModule, ILogger<OutConroller> logger, Settings settings)
        {
            this._temperatureController = temperatureController;
            this._logger = logger;
            this._settings = settings;
            this._OutModule = OutModule;
            this._channel =  () => _settings.OutChannel;
            //this._temperatureController.GlobalErrorEvent += (e) => { StopPollingTemperature(); };
        }

        public void StartPollingTemperature()
        {
            if (IsWorking) return;

            IsWorking = true;

            _logger.LogInformation($"Начат опрос температуры с интервалом {_settings.ZonePollingInterval} мс. Таймаут чтения: {_settings.ZonePollingTimeout} мс.");

            _temperatureController.AddCaller(this._channel(), new TemperatureEvent { reciveError = ErrorHandle, reciveTemperatue = TemperatureHandle });
        }

        public void StopPollingTemperature()
        {
            if (!IsWorking) return;

            IsWorking = false;
            OnPropertyChanged(nameof(IsWorking));

            _temperatureController.RemoveCaller(_channel());

            _logger.LogInformation($"Опрос температуры канала {this._channel()} преркащён");
            IsWorking = false;

        }

        public void TemperatureHandle(double temperature)
        {
            CurrentTemperature = temperature;
        }

        public void ErrorHandle(string message)
        {
            _logger.LogError(message);
            //StopPollingTemperature();

            //NoBlockingMessageBox.ShowError(message);
        }
    }
}
