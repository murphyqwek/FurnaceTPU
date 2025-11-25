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
    public class PortViewModel : BaseViewModel
    {
        private const string NO_AVAILABLE_PORTS = "Нет доступных портов";

        private IEnumerable<string> _availablePorts;
        private string _selectedPort;

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
                }
            }
        }
        #endregion

        public RemoteCommand RefreshCommand { get; }

        public PortViewModel()
        {
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
