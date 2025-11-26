using FurnaceWPF.Commands;
using FurnaceWPF.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FurnaceWPF.ViewModels
{
    public class SettingsViewModel : BaseViewModel
    {
        private readonly Settings _settings;
        private bool _hasUnsavedChanges = false;

        #region Properties
        public bool IsAvailable => !_settings.IsRunning;

        public bool IsDebug
        {
            get => _settings.IsDebug;

            set
            {
                if (_settings.IsDebug != value)
                {
                    _settings.IsDebug = value;
                    OnPropertyChanged();
                }
            }
        }

        public byte ZoneOneAddress
        {
            get => _settings.ZoneOneAddress;
            set
            {
                if (_settings.ZoneOneAddress != value)
                {
                    _settings.ZoneOneAddress = value;
                    OnPropertyChanged();
                }
            }
        }

        public byte ZoneTwoAddress
        {
            get => _settings.ZoneTwoAddress;
            set
            {
                if (_settings.ZoneTwoAddress != value)
                {
                    _settings.ZoneTwoAddress = value;
                    OnPropertyChanged();
                }
            }
        }

        public byte ZoneThreeAddress
        {
            get => _settings.ZoneThreeAddress;
            set
            {
                if (_settings.ZoneThreeAddress != value)
                {
                    _settings.ZoneThreeAddress = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool HasUnsavedChanges
        {
            get => _hasUnsavedChanges;
            set
            {
                if (_hasUnsavedChanges != value)
                {
                    _hasUnsavedChanges = value;
                    OnPropertyChanged(nameof(HasUnsavedChanges));
                }
            }
        }
        #endregion

        public SettingsViewModel(Settings settings)
        {
            this._settings = settings;

            _settings.PropertyChanged += (s, e) =>
            {
                OnPropertyChanged(e.PropertyName);
            };

            _settings.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(Settings.IsRunning))
                {
                    OnPropertyChanged(nameof(IsAvailable));
                }
            };
        }
    }
}
