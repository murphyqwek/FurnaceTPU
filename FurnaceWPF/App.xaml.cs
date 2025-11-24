using FurnaceCore.Filters;
using FurnaceCore.IOManager;
using FurnaceCore.Model;
using FurnaceWPF.Converters;
using Microsoft.Extensions.DependencyInjection;
using pechka4._8.ViewModels;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace pechka4._8
{
    /// <summary>
    /// Логика взаимодействия для App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static IServiceProvider Services { get; private set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            var services = new ServiceCollection();

            ConfigureServices(services);

            Services = services.BuildServiceProvider();

            base.OnStartup(e);
        }

        private void ConfigureServices(ServiceCollection services)
        {
            ConfigureFurnaceModules(services);

            // Views
            services.AddTransient<MainWindow>();
        }

        private void ConfigureFurnaceModules(ServiceCollection services)
        {
            IOManager ioManager = new IOManager();
            TemperatureModule temperatureModule = new TemperatureModule(0x01, 0x00, ioManager);
            HeaterModule heaterModule = new HeaterModule(0x02, 0x00, ioManager);
            DriverModule driverModule = new DriverModule(ioManager);

            ModbusAddressFilter temperatureFilter = new ModbusAddressFilter(0x01, 0x04, temperatureModule);

            MockPort mockPort = new MockPort(ioManager);

            ioManager.RegisterModulePort(temperatureModule, mockPort);
            ioManager.RegisterFilter(temperatureFilter);

            ioManager.RegisterModulePort(heaterModule, mockPort);
            ioManager.RegisterModulePort(driverModule, mockPort);

            services.AddSingleton(ioManager);
            services.AddSingleton(temperatureModule);
            services.AddSingleton(heaterModule);
            services.AddSingleton(mockPort);
            services.AddSingleton(driverModule);
        }
    }
}
