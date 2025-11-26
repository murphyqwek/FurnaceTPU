using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace FurnaceWPF.Models
{
    public class Settings : INotifyPropertyChanged
    {
        private bool _isDebug = false;
        private byte _zoneOneAddress = 0x01;
        private byte _zoneTwoAddress = 0x01;
        private byte _zoneThreeAddress = 0x01;

        #region Properties
        public bool IsDebug
        {
            get => _isDebug;

            set
            {
                if (_isDebug != value)
                {
                    _isDebug = value;
                    OnPropertyChanged();
                }
            }
        }

        public byte ZoneOneAddress
        {
            get => _zoneOneAddress;
            set
            {
                if (_zoneOneAddress != value)
                {
                    _zoneOneAddress = value;
                    OnPropertyChanged();
                }
            }
        }

        public byte ZoneTwoAddress
        {
            get => _zoneTwoAddress;
            set
            {
                if (_zoneTwoAddress != value)
                {
                    _zoneTwoAddress = value;
                    OnPropertyChanged();
                }
            }
        }

        public byte ZoneThreeAddress
        {
            get => _zoneThreeAddress;
            set
            {
                if (_zoneThreeAddress != value)
                {
                    _zoneThreeAddress = value;
                    OnPropertyChanged();
                }
            }
        }

        #endregion

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
