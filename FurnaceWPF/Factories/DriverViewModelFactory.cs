using FurnaceCore.Model;
using FurnaceWPF.Models;
using FurnaceWPF.Models.Controllers;
using FurnaceWPF.Models.Controllers.Driver;
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
        private readonly RotationController _rotationController;

        public DriverViewModelFactory(DriverModule driverModule, Settings settings, RotationController rotationController)
        {
            this._driverModule = driverModule;
            this._settings = settings;
            this._rotationController = rotationController;
        }

        public DriverViewModel GetDriverA()
        {
            var driverController = GetDriverController(0, DriversPortEnum.Zero);

            return new DriverViewModel(driverController, "А", _settings, _rotationController);
        }

        public DriverWithArrowViewModel GetDriverB()
        {
            var driverController = GetDriverController(0, DriversPortEnum.One);

            return new DriverWithArrowViewModel(PackIconKind.ArrowRightBold, PackIconKind.ArrowLeftBold, PackIconKind.ArrowRightBold, driverController, "B", _settings, _rotationController);
        }

        public DriverWithArrowViewModel GetDriverC()
        {
            var driverController = GetDriverController(0, DriversPortEnum.Two);

            return new DriverWithArrowViewModel(PackIconKind.ArrowUpBold, PackIconKind.ArrowDownBold, PackIconKind.ArrowUpBold, driverController, "C", _settings, _rotationController);
        }

        private DriverContoller GetDriverController(int channel, DriversPortEnum driversPort)
        {
            DriverContoller driverContoller = new DriverContoller(_driverModule, channel, driversPort, _settings);

            return driverContoller;
        }
    }
}
