using FurnaceCore.Filters;
using FurnaceCore.IOManager;
using FurnaceCore.Model;
using FurnaceCore.Port;
using FurnaceWPF;
using FurnaceWPF.Converters;
using FurnaceWPF.Factories;
using FurnaceWPF.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using pechka4._8.ViewModels;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO.Ports;
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
        }

        private void ConfigureFurnaceModules(ServiceCollection services)
        {
            IOManager ioManager = new IOManager();
            HeaterModule heaterModule = new HeaterModule(0x02, 0x00, ioManager);
            DriverModule driverModule = new DriverModule(ioManager);

            PortModule port = new PortModule(new SerialPort(), ioManager);


            ioManager.RegisterModulePort(heaterModule, port);
            ioManager.RegisterModulePort(driverModule, port);

            services.AddSingleton(ioManager);
            services.AddSingleton<ZoneViewModelFactory>();
            services.AddSingleton(heaterModule);
            services.AddSingleton<IPort>(port);
            services.AddSingleton(driverModule);
            services.AddSingleton<DriverViewModelFactory>();
            services.AddSingleton<FurnaceWindowViewModel>();

            services.AddTransient<PortViewModel>();
        }
    }
}
