using FurnaceCore.Model;
using Microsoft.Extensions.DependencyInjection;
using pechka4._8.Helpers;
using System;
using System.Windows.Input;

namespace pechka4._8.ViewModels
{
    public class DriveAViewModel : ObservableObject
    {
        private bool _isLeft, _isRight;
        private double _speed;
        private string _inputSpeed = "0";

        public bool IsLeft
        {
            get => _isLeft;
            set
            {
                if (_isLeft != value)
                {
                    _isLeft = value;
                    if (_isLeft) IsRight = false;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(IsRight));
                    OnPropertyChanged(nameof(Direction));
                    OnPropertyChanged(nameof(AnimationDuration));
                }
            }
        }

        public bool IsRight
        {
            get => _isRight;
            set
            {
                if (_isRight != value)
                {
                    _isRight = value;
                    if (_isRight) IsLeft = false;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(IsLeft));
                    OnPropertyChanged(nameof(Direction));
                    OnPropertyChanged(nameof(AnimationDuration));
                }
            }
        }

        public double Speed
        {
            get => _speed;
            set
            {
                if (_speed != value)
                {
                    _speed = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(AnimationDuration));
                }
            }
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
        public int Direction => IsRight ? 1 : IsLeft ? -1 : 0;

        // Для сториборда (чем больше скорость — тем меньше длительность оборота)
        public double AnimationSpeed => Speed > 0 ? Math.Max(0.1, 2.5 - Speed / 40) : 2.5;
        public TimeSpan AnimationDuration => TimeSpan.FromSeconds(AnimationSpeed);

        public bool CanConfirmSpeed =>
            double.TryParse(InputSpeed, out var val) && val >= 0 && val <= 100 && Math.Abs(val - Speed) > 0.01;

        private DriversPortEnum _driverPort = DriversPortEnum.Zero;
        private int _driverChannel = 0;
        private DriverModule _driverModule;
        public DriveAViewModel()
        {
            this._driverModule = App.Services.GetRequiredService<DriverModule>();
        }

        public ICommand ConfirmSpeedCommand => new RelayCommand(_ =>
        {
            if (double.TryParse(InputSpeed, out var val) && val >= 0 && val <= 100)
                Speed = val;

            _driverModule.StartDriver(_driverPort);
            _driverModule.SetDriverFrequency(_driverChannel, (ushort)(Speed * 100));
        }, _ => CanConfirmSpeed);
    }
}
