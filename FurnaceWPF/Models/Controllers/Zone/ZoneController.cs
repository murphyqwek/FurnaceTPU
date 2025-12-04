using FurnaceCore.Model;
using FurnaceCore.utlis;
using FurnaceWPF.ViewModels;
using FurnaceWPF.Views.Controls;
using Microsoft.Extensions.Logging;
using pechka4._8;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.RightsManagement;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Animation;
using System.Windows.Media.Converters;
using System.Windows.Threading;

namespace FurnaceWPF.Models.Controllers.Zone
{
    public class ZoneController : BaseObservable, IDisposable
    {
        private Timer _heaterPollingTimer;
        private bool _heatModuleStatus = false;
        private double _currentTemperature;
        private bool _isHeating;
        private bool _isPollingTemperature;
        private byte _channel;

        private HeaterModule _heaterModule;
        private ILogger<ZoneController> _logger;
        private Settings _settings;
        private double _targetTemperature;
        private byte _heatModuleChannel;
        private CancellationTokenSource _pollingCts;
        private Task _pollingTask;

        private TemperatureController _temperatureController;

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
        public bool IsHeating
        {
            get => _isHeating;
            private set
            {
                if (_isHeating != value)
                {
                    _isHeating = value;
                    OnPropertyChanged();
                }
            }
        }
        public bool IsPollingTemperature
        {
            get => _isPollingTemperature;
            private set
            {
                if (_isPollingTemperature != value)
                {
                    _isPollingTemperature = value;
                    OnPropertyChanged();
                }
            }
        }
        #endregion

        public event Action<string>? ErrorEvent;

        public ZoneController(byte channel, HeaterModule heaterModule, ILogger<ZoneController> logger, Settings settings, TemperatureController temperatureController) 
        { 
            this._channel = channel;
            this._heaterModule = heaterModule;
            this._logger = logger;
            this._settings = settings;
            this.IsHeating = false;
            this.IsPollingTemperature = false;
            this._temperatureController = temperatureController;

            _temperatureController.GlobalErrorEvent += GlobalErrorHandle;
        }

        public void StartPollingTemperature()
        {
            if (IsPollingTemperature) return;

            IsPollingTemperature = true;

            _logger.LogInformation($"Начат опрос температуры с интервалом {_settings.ZonePollingInterval} мс. Таймаут чтения: {_settings.ZonePollingTimeout} мс.");

            _temperatureController.AddCaller(this._channel, new TemperatureEvent { reciveError = ErrorHandle, reciveTemperatue = TemperatureHande });
        }

        public void StopPollingTemperature()
        {
            if(!IsPollingTemperature) return;

            IsPollingTemperature = false;
            OnPropertyChanged(nameof(IsPollingTemperature));

            _temperatureController.RemoveCaller(_channel);

            _logger.LogInformation($"Опрос температуры канала {this._channel} преркащён");
            IsPollingTemperature = false;
            
        }

        public void StartPollingHeater(double targetTemperature)
        {
            this.IsHeating = true;
            _targetTemperature = targetTemperature;
            this._heatModuleStatus = false;
            _logger.LogInformation($"Начат нагрев с интервалом {_settings.ZoneHeatCheckingInterval} мс до {_targetTemperature}");
            _heaterModule.TurnOnHeater();

            _pollingCts = new CancellationTokenSource();
            _pollingTask = Task.Run(async () => await PollHeater(_pollingCts.Token));

        }

        public void StopPollingHeater()
        {
            this.IsHeating = false;
            _pollingCts?.Cancel();

            _pollingTask?.Wait();

            _heaterModule.TurnOffHeater();
            _logger.LogInformation("Нагрев остановлен");
        }

        private async Task PollHeater(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    if (!this.IsPollingTemperature)
                    {
                        _logger.LogInformation($"Опрос температуры канала {this._channel} остановлен, прекращаем нагрев");
                        Dispatcher.CurrentDispatcher.Invoke(StopPollingHeater);
                        return;
                    }

                    if (CurrentTemperature < _targetTemperature + _settings.ZoneTreshold && CurrentTemperature > _targetTemperature - _settings.ZoneTreshold)
                    {
                        _logger.LogInformation("Текущая температура в допустимых пределах заданной");
                        _heaterModule.TurnOffHeater(_heatModuleChannel);
                        this._heatModuleStatus = false;
                    }
                    else
                    {
                        if (_targetTemperature - _settings.ZoneTreshold >= CurrentTemperature)
                        {
                            _logger.LogInformation($"Текущая температура ниже установленной. Идёт нагрев");
                            _heaterModule.TurnOnHeater(_heatModuleChannel);
                            await Task.Delay((int)(1000*_settings.ZonePollingCoeff), token);
                            _heaterModule.TurnOffHeater(_heatModuleChannel);
                            this._heatModuleStatus = true;
                        }

                        if (_targetTemperature + _settings.ZoneTreshold <= CurrentTemperature)
                        {
                            _logger.LogInformation($"Текущая температура выше установленной. Идёт охлаждение");
                            _heaterModule.TurnOffHeater(_heatModuleChannel);
                            this._heatModuleStatus = false;
                        }
                    }

                    await Task.Delay(_settings.ZoneHeatCheckingInterval, token);
                }
                catch (OperationCanceledException) when (token.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.ToString());
                    this._heatModuleStatus = false;
                    _logger.LogInformation("Нагрев остановлен");
                }
            }

            _logger.LogInformation("Алгоритм нагрева остановлен");
        }

        //private void HeatOrCold()
        //{
           
        //}

        public void SetChannelByte(byte newChannel)
        {
            _temperatureController.ChangeChannel(this._channel, newChannel);
            _logger.LogInformation($"Контроллер зоны поменял канал с {this._channel} на {newChannel}");
            this._channel = newChannel;
        }

        public void Dispose()
        {
            _heaterPollingTimer?.Dispose();
            _heaterPollingTimer = null;
        }

        private void TemperatureHande(double temperature)
        {
            _logger.LogInformation($"Контроллер зоны получил новые данные по температуре: {temperature}");
            this.CurrentTemperature = temperature;
            OnPropertyChanged(nameof(CurrentTemperature));
        }

        private void ErrorHandle(string message)
        {
            StopPollingTemperature();
            _logger.LogError(message);
            MessageBox.Show(message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void GlobalErrorHandle(string message)
        {
            StopPollingTemperature();
        }
    }

}
