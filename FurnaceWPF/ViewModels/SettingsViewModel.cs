using FurnaceCore.Model;
using FurnaceWPF.Helpers;
using FurnaceWPF.Models;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace FurnaceWPF.ViewModels
{
    public class SettingsViewModel : INotifyPropertyChanged
    {
        private readonly Settings _originalSettings; // Оригинал
        private Settings _currentSettings; // Копия для редактирования
        private SettingsLoader _loader;

        public Settings CurrentSettings
        {
            get => _currentSettings;
            set
            {
                _currentSettings = value;
                OnPropertyChanged(nameof(CurrentSettings));
            }
        }

        private bool _hasUnsavedChanges;
        public bool HasUnsavedChanges
        {
            get => _hasUnsavedChanges;
            set
            {
                _hasUnsavedChanges = value;
                OnPropertyChanged(nameof(HasUnsavedChanges));
            }
        }

        public ICommand ApplyChangesCommand { get; }

        public SettingsViewModel(Settings originalSettings, SettingsLoader settingsLoader)
        {
            _originalSettings = originalSettings;
            _loader = settingsLoader;

            CurrentSettings = CloneSettings(originalSettings); // Копируем

            // Отслеживаем изменения в копии
            CurrentSettings.PropertyChanged += OnSettingsPropertyChanged;

            ApplyChangesCommand = new RelayCommand(ApplyChanges, CanApplyChanges);
        }

        private void OnSettingsPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            HasUnsavedChanges = true; // Простой флаг; можно сделать сравнение
            ((RelayCommand)ApplyChangesCommand).RaiseCanExecuteChanged();
        }

        private bool CanApplyChanges(object param)
        {
            if (!HasUnsavedChanges) return false;
            if (param is not DependencyObject element) return true; // если не передали — считаем, что ошибок нет

            return !HasValidationErrors(element);
        }

        private void ApplyChanges(object param)
        {
            var control = param as UserControl;
            if (control == null) return;

            // 1. Принудительно обновляем все binding'и (чтобы валидация сработала)
            foreach (var bindingExpression in BindingOperations.GetSourceUpdatingBindings(control))
            {
                bindingExpression.UpdateSource();
            }

            // 2. Проверяем обычные ошибки валидации (диапазоны, пустые поля и т.д.)
            if (HasValidationErrors(control))
            {
                MessageBox.Show("Исправьте ошибки в полях (красные рамки).",
                                "Ошибка валидации", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // 3. Проверка: каналы зон должны быть уникальными
            var zoneChannels = new HashSet<byte>
            {
                CurrentSettings.ZoneOneChannel,
                CurrentSettings.ZoneTwoChannel,
                CurrentSettings.ZoneThreeChannel
            };

            if (zoneChannels.Count != 3)
            {
                MessageBox.Show(
                    "Каналы температурных зон должны быть разными!\n" +
                    $"Зона 1: {CurrentSettings.ZoneOneChannel}\n" +
                    $"Зона 2: {CurrentSettings.ZoneTwoChannel}\n" +
                    $"Зона 3: {CurrentSettings.ZoneThreeChannel}",
                    "Конфликт каналов зон", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // 4. Проверка: порты драйверов должны быть уникальными
            var driverPorts = new HashSet<DriversPortEnum>
            {
                CurrentSettings.DriverAPort,
                CurrentSettings.DriverBPort,
                CurrentSettings.DriverCPort
            };

            if (driverPorts.Count != 3)
            {
                MessageBox.Show(
                    "Порты драйверов A, B и C должны быть разными!\n" +
                    $"Драйвер A: {CurrentSettings.DriverAPort}\n" +
                    $"Драйвер B: {CurrentSettings.DriverBPort}\n" +
                    $"Драйвер C: {CurrentSettings.DriverCPort}",
                    "Конфликт портов драйверов", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // 5. Всё ок — применяем
            CopyProperties(CurrentSettings, _originalSettings);
            HasUnsavedChanges = false;

            var savingResult = _loader.Save(_originalSettings);

            if (savingResult.Success)
            {
                MessageBox.Show("Все настройки успешно применены и сохранены!",
                                "Готово", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("Все настройки успешно применены, но при сохранении произошла ошибка: " + savingResult.ErrorMessage, 
                                "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private Settings CloneSettings(Settings source)
        {
            // Простой клон; используй deep clone если нужно
            return new Settings
            {
                IsDebug = source.IsDebug,

                ZoneOneChannel = source.ZoneOneChannel,
                ZoneTwoChannel = source.ZoneTwoChannel,
                ZoneThreeChannel = source.ZoneThreeChannel,

                ZoneHeaterOneChannel = source.ZoneHeaterOneChannel,
                ZoneHeaterTwoChannel = source.ZoneHeaterTwoChannel,
                ZoneHeaterThreeChannel = source.ZoneHeaterThreeChannel,

                DriverAChannel = source.DriverAChannel,
                DriverBChannel = source.DriverBChannel,
                DriverCChannel = source.DriverCChannel,

                DriverAPort = source.DriverAPort,
                DriverBPort = source.DriverBPort,
                DriverCPort = source.DriverCPort,

                DriverAddress = source.DriverAddress,

                CoolingChannel = source.CoolingChannel,

                IsRunning = source.IsRunning,
                IsPortOpen = source.IsPortOpen,

                StepSizeDriver = source.StepSizeDriver,
                DriverRampingUpdateInterval = source.DriverRampingUpdateInterval,

                ZonePollingInterval = source.ZonePollingInterval,
                ZonePollingCoeff = source.ZonePollingCoeff,
                ZoneTreshold = source.ZoneTreshold,
                ZonePollingTimeout = source.ZonePollingTimeout,

                CoolingPollingTimeout = source.CoolingPollingTimeout,
                CoolingPollingTemperatureIntervall = source.CoolingPollingTemperatureIntervall,

                RotationTimeout = source.RotationTimeout,
                RotationPollingInterval = source.RotationPollingInterval



            };
        }


        private bool HasValidationErrors(DependencyObject node)
        {
            if (Validation.GetHasError(node)) return true;
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(node); i++)
            {
                var child = VisualTreeHelper.GetChild(node, i);
                if (HasValidationErrors(child)) return true;
            }
            return false;
        }

        private void CopyProperties(Settings source, Settings target)
        {
            target.IsDebug = source.IsDebug;

            target.ZoneOneChannel = source.ZoneOneChannel;
            target.ZoneTwoChannel = source.ZoneTwoChannel;
            target.ZoneThreeChannel = source.ZoneThreeChannel;

            target.ZoneHeaterOneChannel = source.ZoneHeaterOneChannel;
            target.ZoneHeaterTwoChannel = source.ZoneHeaterTwoChannel;
            target.ZoneHeaterThreeChannel = source.ZoneHeaterThreeChannel;

            target.DriverAChannel = source.DriverAChannel;
            target.DriverBChannel = source.DriverBChannel;
            target.DriverCChannel = source.DriverCChannel;

            target.DriverAPort = source.DriverAPort;
            target.DriverBPort = source.DriverBPort;
            target.DriverCPort = source.DriverCPort;

            target.DriverAddress = source.DriverAddress;

            target.CoolingChannel = source.CoolingChannel;

            target.IsRunning = source.IsRunning;
            target.IsPortOpen = source.IsPortOpen;

            target.StepSizeDriver = source.StepSizeDriver;
            target.DriverRampingUpdateInterval = source.DriverRampingUpdateInterval;

            target.ZonePollingInterval = source.ZonePollingInterval;
            target.ZonePollingCoeff = source.ZonePollingCoeff;
            target.ZoneTreshold = source.ZoneTreshold;
            target.ZonePollingTimeout = source.ZonePollingTimeout;

            target.CoolingPollingTimeout = source.CoolingPollingTimeout;
            target.CoolingPollingTemperatureIntervall = source.CoolingPollingTemperatureIntervall;

            target.RotationTimeout = source.RotationTimeout;
            target.RotationPollingInterval = source.RotationPollingInterval;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    // Простая реализация ICommand
    public class RelayCommand : ICommand
    {
        private readonly Action<object> _execute;
        private readonly Func<object, bool> _canExecute;

        public RelayCommand(Action<object> execute, Func<object, bool> canExecute = null)
        {
            _execute = execute;
            _canExecute = canExecute;
        }

        public event EventHandler CanExecuteChanged;
        public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);

        public bool CanExecute(object parameter) => true;
        public void Execute(object parameter) => _execute(parameter);
    }
}