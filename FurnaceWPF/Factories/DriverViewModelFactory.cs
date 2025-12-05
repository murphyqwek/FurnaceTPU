using FurnaceCore.Model;
using FurnaceWPF.Models;
using FurnaceWPF.Models.Controllers;
using FurnaceWPF.Models.Controllers.Driver;
using FurnaceWPF.ViewModels;
using MaterialDesignThemes.Wpf;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
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
        private readonly ILogger<DriverContoller> _logger;
        public DriverViewModelFactory(DriverModule driverModule, Settings settings, RotationController rotationController, ILogger<DriverContoller> logger)
        {
            this._driverModule = driverModule;
            this._settings = settings;
            this._rotationController = rotationController;
            this._logger = logger;
        }

        public DriverViewModel GetDriverA()
        {
            var driverController = GetDriverController(() =>_settings.DriverAChannel, () => _settings.DriverAPort);

            return new DriverViewModel(driverController, "А", _settings, _rotationController);
        }

        public DriverWithArrowViewModel GetDriverB()
        {
            var driverController = GetDriverController(() => _settings.DriverBChannel, () => _settings.DriverBPort);

            return new DriverWithArrowViewModel(PackIconKind.ArrowRightBold, PackIconKind.ArrowLeftBold, PackIconKind.ArrowRightBold, driverController, "B", _settings, _rotationController);
        }

        public DriverWithArrowViewModel GetDriverC()
        {
            var driverController = GetDriverController(() => _settings.DriverCChannel, () => _settings.DriverCPort);

            return new DriverWithArrowViewModel(PackIconKind.ArrowUpBold, PackIconKind.ArrowDownBold, PackIconKind.ArrowUpBold, driverController, "C", _settings, _rotationController);
        }

        private DriverContoller GetDriverController(Func<int> channel, Func<DriversPortEnum> driversPort)
        {
            DriverContoller driverContoller = new DriverContoller(_driverModule, channel, driversPort, _settings, _logger);

            return driverContoller;
        }

    }
}
