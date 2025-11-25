using FurnaceCore.Model;
using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FurnaceWPF.ViewModels
{
    public class DriverWithArrowViewModel : DriverViewModel
    {
        private PackIconKind _forwardArrowKind;
        private PackIconKind _backwardArrowKind;
        private PackIconKind _stopArrowKind;

        public PackIconKind ArrowKind => DirectionEnum switch
        {
            DriverDirectionEnum.Forward => _forwardArrowKind,
            DriverDirectionEnum.Backward => _backwardArrowKind,
            DriverDirectionEnum.Stop => _stopArrowKind,
            _ => _stopArrowKind
        };

        public string ArrowDirection => DirectionEnum switch
        {
            DriverDirectionEnum.Forward => "Right",
            DriverDirectionEnum.Backward => "Left",
            DriverDirectionEnum.Stop => "Stop",
            _ => "Stop"
        };


        

        public DriverWithArrowViewModel(PackIconKind forward, PackIconKind backward, PackIconKind stop, DriverModule driverModule, string name) : base(driverModule, name)
        {
            this._forwardArrowKind = forward;
            this._backwardArrowKind = backward;
            this._stopArrowKind = stop;

            this.PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName == nameof(DirectionEnum))
                {
                    OnPropertyChanged(nameof(ArrowKind));
                    OnPropertyChanged(nameof(ArrowDirection));
                }
            };          
        }
    }
}
