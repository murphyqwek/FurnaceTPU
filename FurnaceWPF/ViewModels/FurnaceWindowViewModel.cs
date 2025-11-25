using FurnaceWPF.Factories;
using Microsoft.Extensions.DependencyInjection;
using pechka4._8;
using pechka4._8.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FurnaceWPF.ViewModels
{
    public class FurnaceWindowViewModel
    {
        public DriverViewModel DriveA { get; }
        public DriverWithArrowViewModel DriveB { get; }
        public DriverWithArrowViewModel DriveC { get; }

        public ZoneViewModel Zone1 { get; }
        public ZoneViewModel Zone2 { get; }
        public ZoneViewModel Zone3 { get; }

        public FurnaceWindowViewModel(DriverViewModelFactory driverFactory, ZoneViewModelFactory zoneFactory)
        {
            DriveA = driverFactory.GetDriverA();
            DriveB = driverFactory.GetDriverB();
            DriveC = driverFactory.GetDriverC();

            Zone1 = zoneFactory.GetFirstZone();
            Zone2 = zoneFactory.GetSecondZone();
            Zone3 = zoneFactory.GetThirdZone();
        }
    }
}
