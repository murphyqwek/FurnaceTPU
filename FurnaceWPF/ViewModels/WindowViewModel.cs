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
    public class WindowViewModel
    {
        public DriverViewModel DriveA { get; }
        public DriverWithArrowViewModel DriveB { get; }
        public DriverWithArrowViewModel DriveC { get; }

        public WindowViewModel()
        {
            var factory = App.Services.GetRequiredService<DriverViewModelFactory>();

            DriveA = factory.GetDriverA();
            DriveB = factory.GetDriverB();
            DriveC = factory.GetDriverC();
        }
    }
}
