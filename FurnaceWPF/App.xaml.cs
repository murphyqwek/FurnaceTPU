using FurnaceCore.Filters;
using FurnaceCore.IOManager;
using FurnaceCore.Model;
using FurnaceCore.Port;
using FurnaceWPF;
using FurnaceWPF.Factories;
using FurnaceWPF.Models;
using Microsoft.Extensions.Logging;
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
using FurnaceWPF.Views;

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

            var mainWindow = Services.GetRequiredService<FurnaceWindow>();

            mainWindow.Show();

            base.OnStartup(e);
        }

        private void ConfigureServices(ServiceCollection services)
        {
            services.AddLogging(builder =>
            {
                builder.ClearProviders();
                builder.AddDebug();
                builder.AddConsole();
                builder.SetMinimumLevel(LogLevel.Debug);
            });

            services.AddSingleton<Settings>();

            ConfigureFactories(services);
            ConfigureViewModels(services);
            ConfigureFurnaceModules(services);
            ConfigureWindow(services);
        }

        private void ConfigureFactories(ServiceCollection services)
        {
            services.AddSingleton<ZoneViewModelFactory>();
            services.AddSingleton<DriverViewModelFactory>();

            services.AddSingleton<Func<MockPort>>(sp =>
            {
                return () => sp.GetRequiredService<MockPort>();
            });
            services.AddSingleton<Func<PortModule>>(sp =>
            {
                return () => sp.GetRequiredService<PortModule>();
            });
        }

        private void ConfigureWindow(ServiceCollection services)
        {
            services.AddSingleton<FurnaceWindow>();
            services.AddSingleton<SettingsWindow>();
        }

        private void ConfigureViewModels(ServiceCollection services)
        {
            services.AddSingleton<FurnaceWindowViewModel>();

            services.AddTransient<PortViewModel>();
            services.AddTransient<SettingsViewModel>();
            services.AddTransient<SettingsWindowViewModel>();
        }

        private void ConfigureFurnaceModules(ServiceCollection services)
        {
            services.AddSingleton<IOManager>();
            services.AddTransient<MockPort>();
            services.AddTransient<PortModule>(sp =>
            {
                var ioManager = sp.GetRequiredService<IOManager>();
                PortModule module = new PortModule(new SerialPort(), ioManager);
                return module;
            });
            services.AddSingleton<IPort, SwitchingPort>(sp =>
            {
                var mockFactory = sp.GetRequiredService<Func<MockPort>>();
                var portModuleFactory = sp.GetRequiredService<Func<PortModule>>();
                var settings = sp.GetRequiredService<Settings>();
                SwitchingPort port = new SwitchingPort(settings.IsDebug, mockFactory, portModuleFactory, sp.GetRequiredService<ILogger<SwitchingPort>>());
                return port;
            });

            services.AddSingleton<HeaterModule>(sp =>
            {
                var ioManager = sp.GetRequiredService<IOManager>();
                var port = sp.GetRequiredService<IPort>();
                HeaterModule heaterModule = new HeaterModule(0x0, 0x0, ioManager);

                ioManager.RegisterModulePort(heaterModule, port);

                return heaterModule;
            });

            services.AddSingleton<DriverModule>(sp =>
            {
                var ioManager = sp.GetRequiredService<IOManager>();
                var port = sp.GetRequiredService<IPort>();
                DriverModule driverModule = new DriverModule(ioManager);
                ioManager.RegisterModulePort(driverModule, port);

                return driverModule;
            });
        }
    }
}
