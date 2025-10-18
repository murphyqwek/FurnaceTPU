using pechka4._8.Helpers;
using System;
using System.Windows.Input;
using MaterialDesignThemes.Wpf;

namespace pechka4._8.ViewModels
{
    public class DriveCViewModel : ObservableObject
    {
        private bool _isLeft, _isRight;
        private double _speed;
        private string _inputSpeed = "0";

        public PackIconKind ArrowKind => IsLeft ? PackIconKind.ArrowDownBold :
                                             IsRight ? PackIconKind.ArrowUpBold :
                                             PackIconKind.ArrowUpBold;

        public string ArrowDirection => IsRight ? "Up" : IsLeft ? "Down" : "Stop";
        public double AnimationSpeed => Math.Max(0.2, 2.5 - Speed / 40);
        public TimeSpan AnimationDuration => TimeSpan.FromSeconds(AnimationSpeed);

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
                    OnPropertyChanged(nameof(IsRight)); // синхронизация
                    OnPropertyChanged(nameof(ArrowDirection)); // для анимации
                    OnPropertyChanged(nameof(AnimationDuration));
                    OnPropertyChanged(nameof(ArrowKind));
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
                    OnPropertyChanged(nameof(ArrowDirection));
                    OnPropertyChanged(nameof(AnimationDuration));
                    OnPropertyChanged(nameof(ArrowKind));
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

        public bool CanConfirmSpeed =>
            double.TryParse(InputSpeed, out var val) && val >= 0 && val <= 100 && Math.Abs(val - Speed) > 0.01;

        public ICommand ConfirmSpeedCommand => new RelayCommand(_ =>
        {
            if (double.TryParse(InputSpeed, out var val) && val >= 0 && val <= 100)
                Speed = val;
        }, _ => CanConfirmSpeed);
    }
}
