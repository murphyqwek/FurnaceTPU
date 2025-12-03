using FurnaceCore.Port;
using FurnaceWPF.Commands;
using FurnaceWPF.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace FurnaceWPF.ViewModels
{
    public class PortViewModel : BaseObservable
    {
        private const string NO_AVAILABLE_PORTS = "Нет доступных портов";

        private IEnumerable<string> _availablePorts;
        private string _selectedPort;

        private IPort _portModule;
        private Settings _settings;

        #region Properties
        public IEnumerable<string> AvailablePorts
        {
            get => _availablePorts;
            set
            {
                _availablePorts = value;
                OnPropertyChanged();
            }
        }

        public string SelectedPort
        {
            get => _selectedPort;
            set
            {
                if (_selectedPort != value && value != NO_AVAILABLE_PORTS)
                {
                    _selectedPort = value;
                    OnPropertyChanged();
                    if (SelectedPort != NO_AVAILABLE_PORTS)
                    {
                        _portModule.Name = SelectedPort;
                    }
                }
            }
        }

        public bool IsPortOpen
        {
            get => _portModule.IsOpen();

            set => SetPortIsOpen(value);
        }

        public bool IsPortChosingAvailable
        {
            get => !_portModule.IsOpen();
        }
        #endregion

        public event Action PortClosing;
        public RemoteCommand RefreshCommand { get; }

        public PortViewModel(IPort portModule, Settings settings)
        {
            this._portModule = portModule;
            this._settings = settings;
            LoadAvailablePorts();
            RefreshCommand = new RemoteCommand(() => LoadAvailablePorts());
        }

        public void LoadAvailablePorts()
        {
            AvailablePorts = SerialPort.GetPortNames().OrderBy(p => p);

            SelectedPort = AvailablePorts.Count() > 0 ? AvailablePorts.First() : NO_AVAILABLE_PORTS;
        }

        public void SetPortIsOpen(bool isOpen)
        {
            if (isOpen)
            {
                try
                {
                    _portModule.OpenPort();
                }
                catch (IOException ex)
                {
                    _portModule.ClosePort();
                    MessageBox.Show(ex.Message, "Ошибка открытия порта", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                PortClosing?.Invoke();
                _portModule.ClosePort();
            }

            _settings.IsPortOpen = IsPortOpen;
            OnPropertyChanged(nameof(IsPortOpen));
            OnPropertyChanged(nameof(IsPortChosingAvailable));
        }
    }
}
