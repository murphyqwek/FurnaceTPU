using FurnaceCore.Model;
using FurnaceWPF.Models;
using FurnaceWPF.Models.Controllers;
using FurnaceWPF.Models.Controllers.Driver;
using Microsoft.Extensions.DependencyInjection;
using pechka4._8;
using pechka4._8.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;

namespace FurnaceWPF.ViewModels
{
    public enum DriverDirectionEnum : int
    {
        Stop = 0,
        Forward = -1,
        Backward = 1
    }

    public class DriverViewModel : BaseObservable
    {
        private DriverContoller _driverController;
        private string _name;
        private string _inputSpeed;

        private DriverDirectionEnum _direction = DriverDirectionEnum.Stop;

        private Settings _settings;

        private RotationController _rotationController;

        private bool _isWorking;

        private bool _isAnimationReversed;

        #region Properties
        public string DriverName => _name;
        public DriverDirectionEnum DirectionEnum
        {
            get => _direction;
            set
            {
                if (_direction != value)
                {
                    _direction = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(Direction));
                    OnPropertyChanged(nameof(IsLeft));
                    OnPropertyChanged(nameof(IsRight));
                    AnimationSettingsChangeed?.Invoke();
                }
            }
        }

        public bool IsLeft
        {
            get => DirectionEnum == DriverDirectionEnum.Backward;
        }

        public bool IsRight
        {
            get => DirectionEnum == DriverDirectionEnum.Forward;
        }

        public double Speed
        {
            get => _driverController.CurrentFrequency / 80;
        }

        public string InputSpeed
        {
            get => _inputSpeed;
            set
            {
                if (_inputSpeed != value)
                {
                    _inputSpeed = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(CanConfirmSpeed));
                    UpdateCanConfirmSpeed();
                }
            }
        }

        public bool IsWorking
        {
            get => _isWorking;
            set
            {
                if (_isWorking != value) { 
                    _isWorking = value;

                    OnIsWorkingUpdate();
                    OnPropertyChanged(nameof(IsWorking));
                    UpdateCanConfirmSpeed();
                }
            }
        }

        public bool IsEnabled
        {
            get => _settings.IsPortOpen;
        }

        // Направление вращения: 1 = по часовой, -1 = против, 0 = стоп
        public int Direction => ((int)DirectionEnum);

        // Для сториборда (чем больше скорость — тем меньше длительность оборота)
        public double AnimationSpeed => Speed > 0 ? Math.Max(0.1, 2.5 - Speed / 40) : 2.5;
        public TimeSpan AnimationDuration => TimeSpan.FromSeconds(AnimationSpeed);

        public bool CanConfirmSpeed 
        { 
            get
            {
                double val;
                var isParsed = double.TryParse(InputSpeed, out val);

                if (!isParsed)
                {
                    return false;
                }

                return val >= 0 && val <= 100 && Math.Abs(val - Speed) > 0.01 && _settings.IsPortOpen && IsWorking;
            }
        }
        #endregion

        #region Commands
        public ICommand ConfirmSpeedCommand => new RelayCommand(_ => ConfirmSpeedHandler(), _ => CanConfirmSpeed);
        #endregion

        public event Action? AnimationSettingsChangeed;
        public DriverViewModel(DriverContoller driverController, string name, Settings settings, RotationController rotationController, bool isAnimationReversed)
        {
            this._driverController = driverController;
            this._name = name;
            this._settings = settings;
            this._rotationController = rotationController;
            this._isAnimationReversed = isAnimationReversed;

            _driverController.PropertyChanged += DriverController_PropertyChanged;
            _settings.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(Settings.IsPortOpen))
                {
                    OnPropertyChanged(nameof(IsEnabled));
                    OnPropertyChanged(nameof(CanConfirmSpeed));
                }
            };

            PropertyChanged += (s, e) =>
            {
                if (e.PropertyName is nameof(InputSpeed) or
                                         nameof(Speed) or
                                         nameof(IsWorking) or
                                         nameof(IsEnabled))
                {
                    (ConfirmSpeedCommand as RelayCommand)?.RaiseCanExecuteChanged();
                }
            };

            rotationController.RotationErrorEvent += (m) => App.Current.Dispatcher.BeginInvoke(() => IsWorking = false);
            _isAnimationReversed = isAnimationReversed;
        }

        private void UpdateCanConfirmSpeed()
        {
            (ConfirmSpeedCommand as RelayCommand)?.RaiseCanExecuteChanged();
        }

        private void ConfirmSpeedHandler()
        {
            if (double.TryParse(InputSpeed, out var val) && val >= 0 && val <= 100)
                _driverController.SetNewTarget((ushort)(val * 80));
        }

        private void RotationUpdateHandler(RotationData rotationData)
        {
            if(!IsWorking) return;

            var port = _driverController.GetDriversPort();
            var rotationDirection = rotationData.rotations.GetValueOrDefault(port);

            App.Current.Dispatcher.Invoke(() => SetDrivetionEnum(rotationDirection));
        }

        private void SetDrivetionEnum(RotationEnum rotation)
        {
            if(rotation == RotationEnum.Right)
            {
                DirectionEnum = _isAnimationReversed ? DriverDirectionEnum.Backward : DriverDirectionEnum.Forward;
            }
            else
            {
                DirectionEnum = _isAnimationReversed ? DriverDirectionEnum.Forward : DriverDirectionEnum.Backward;
            }
        }

        private void DriverController_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if(e.PropertyName == nameof(DriverContoller.CurrentFrequency))
            {
                OnPropertyChanged(nameof(Speed));
                OnPropertyChanged(nameof(CanConfirmSpeed));
                AnimationSettingsChangeed?.Invoke();
            }
        }

        private void OnIsWorkingUpdate()
        {
            if(IsWorking)
            {
                _driverController.Start();
                _rotationController.AddSubscriberToRotationUpdate(RotationUpdateHandler);
            }
            else
            {
                DirectionEnum = DriverDirectionEnum.Stop;
                _rotationController.RemoveSubscriberToRotationUpdate(RotationUpdateHandler);
                _driverController.Stop();
            }
        }

        public void Dispose()
        {
            this._driverController.Dispose();
        }
    }
}
