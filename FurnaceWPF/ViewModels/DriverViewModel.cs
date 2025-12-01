using FurnaceCore.Model;
using FurnaceWPF.Models;
using FurnaceWPF.Models.Controllers;
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

namespace FurnaceWPF.ViewModels
{
    public enum DriverDirectionEnum : int
    {
        Stop = 0,
        Forward = 1,
        Backward = -1
    }

    public class DriverViewModel : BaseObservable
    {
        private DriverContoller _driverController;
        private string _name;
        private string _inputSpeed;

        private DriverDirectionEnum _direction = DriverDirectionEnum.Stop;

        private Settings _settings;

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
            set
            {
                if (value)
                {
                    DirectionEnum = IsLeft ? DriverDirectionEnum.Stop : DriverDirectionEnum.Backward;
                }
                else if (IsLeft)
                {
                    DirectionEnum = DriverDirectionEnum.Stop;
                }
            }
        }

        public bool IsRight
        {
            get => DirectionEnum == DriverDirectionEnum.Forward;
            set
            {
                if (value)
                {
                    DirectionEnum = IsRight ? DriverDirectionEnum.Stop : DriverDirectionEnum.Forward;
                }
                else if (IsRight)
                {
                    DirectionEnum = DriverDirectionEnum.Stop;
                }
            }
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
                }
            }
        }

        // Направление вращения: 1 = по часовой, -1 = против, 0 = стоп
        public int Direction => ((int)DirectionEnum);

        // Для сториборда (чем больше скорость — тем меньше длительность оборота)
        public double AnimationSpeed => Speed > 0 ? Math.Max(0.1, 2.5 - Speed / 40) : 2.5;
        public TimeSpan AnimationDuration => TimeSpan.FromSeconds(AnimationSpeed);

        public bool CanConfirmSpeed =>
            double.TryParse(InputSpeed, out var val) && val >= 0 && val <= 100 && Math.Abs(val - Speed) > 0.01 && _settings.IsPortOpen;
        #endregion

        #region Commands
        public ICommand ConfirmSpeedCommand => new RelayCommand(_ =>
        {
            if (double.TryParse(InputSpeed, out var val) && val >= 0 && val <= 100)
                _driverController.SetNewTarget((ushort)(val * 80));

        }, _ => CanConfirmSpeed);
        #endregion

        public event Action? AnimationSettingsChangeed;
        public DriverViewModel(DriverContoller driverController, string name, Settings settings)
        {
            this._driverController = driverController;
            this._name = name;
            this._settings = settings;

            driverController.PropertyChanged += DriverController_PropertyChanged;
        }

        private void DriverController_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if(e.PropertyName == nameof(DriverContoller.CurrentFrequency))
            {
                OnPropertyChanged(nameof(Speed));
            }
        }

        public void Dispose()
        {
            this._driverController.Dispose();
        }
    }
}
