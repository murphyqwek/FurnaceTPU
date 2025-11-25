using FurnaceCore.Model;
using FurnaceWPF.ViewModels;
using MaterialDesignThemes.Wpf;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FurnaceWPF.Factories
{
    public class DriverViewModelFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public DriverViewModelFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public DriverViewModel GetDriverA()
        {
            DriverModule driverModule = _serviceProvider.GetRequiredService<DriverModule>();

            return new DriverViewModel(driverModule, "А");
        }

        public DriverWithArrowViewModel GetDriverB()
        {
            DriverModule driverModule = _serviceProvider.GetRequiredService<DriverModule>();

            return new DriverWithArrowViewModel(PackIconKind.ArrowLeftBold, PackIconKind.ArrowRightBold, PackIconKind.ArrowLeftBold, driverModule, "B");
        }

        public DriverWithArrowViewModel GetDriverC()
        {
            DriverModule driverModule = _serviceProvider.GetRequiredService<DriverModule>();

            return new DriverWithArrowViewModel(PackIconKind.ArrowDownBold, PackIconKind.ArrowUpBold, PackIconKind.ArrowDownBold, driverModule, "C");
        }
    }
}
