using FurnaceCore.Model;
using FurnaceWPF.Models;
using FurnaceWPF.Models.Controllers;
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
        private readonly DriverModule _driverModule;
        private readonly Settings _settings;

        public DriverViewModelFactory(DriverModule driverModule, Settings settings)
        {
            this._driverModule = driverModule;
            this._settings = settings;
        }

        public DriverViewModel GetDriverA()
        {
            var driverController = GetDriverController(0, DriversPortEnum.Zero);

            return new DriverViewModel(driverController, "А");
        }

        public DriverWithArrowViewModel GetDriverB()
        {
            var driverController = GetDriverController(0, DriversPortEnum.One);

            return new DriverWithArrowViewModel(PackIconKind.ArrowRightBold, PackIconKind.ArrowLeftBold, PackIconKind.ArrowRightBold, driverController, "B");
        }

        public DriverWithArrowViewModel GetDriverC()
        {
            var driverController = GetDriverController(0, DriversPortEnum.Two);

            return new DriverWithArrowViewModel(PackIconKind.ArrowUpBold, PackIconKind.ArrowDownBold, PackIconKind.ArrowUpBold, driverController, "C");
        }

        private DriverContoller GetDriverController(int channel, DriversPortEnum driversPort)
        {
            DriverContoller driverContoller = new DriverContoller(_driverModule, channel, driversPort, _settings);

            return driverContoller;
        }
    }
}
