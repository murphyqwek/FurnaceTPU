using FurnaceCore.Port;
using FurnaceWPF.Commands;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO.Ports;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace FurnaceWPF.ViewModels
{
    public class PortViewModel : BaseObservable
    {
        private const string NO_AVAILABLE_PORTS = "Нет доступных портов";

        private IEnumerable<string> _availablePorts;
        private string _selectedPort;

        private IPort _portModule;

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
                        _portModule.ClosePort();
                        _portModule.Name = SelectedPort;
                        _portModule.OpenPort();
                    }
                }
            }
        }
        #endregion

        public RemoteCommand RefreshCommand { get; }

        public PortViewModel(IPort portModule)
        {
            _portModule = portModule;
            LoadAvailablePorts();
            RefreshCommand = new RemoteCommand(() => LoadAvailablePorts());
        }

        public void LoadAvailablePorts()
        {
            AvailablePorts = SerialPort.GetPortNames().OrderBy(p => p);

            SelectedPort = AvailablePorts.Count() > 0 ? AvailablePorts.First() : NO_AVAILABLE_PORTS;
        }
    }
}
