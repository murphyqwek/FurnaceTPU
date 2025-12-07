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
using FurnaceWPF.Models.Controllers.Cooling;
using FurnaceWPF.Models.Controllers.Zone;
using FurnaceWPF.Models.Controllers.Driver;
using FurnaceWPF.Helpers;
using FurnaceWPF.Models.Controllers.Out;

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

            ConfigureSettings(services);

            ConfigureControllers(services);
            ConfigureFactories(services);
            ConfigureViewModels(services);
            ConfigureFurnaceModules(services);
            ConfigureWindow(services);
        }

        private void ConfigureSettings(ServiceCollection services)
        {
            services.AddSingleton<SettingsLoader>();

            services.AddSingleton<Settings>(provider =>
            {
                var loader = provider.GetRequiredService<SettingsLoader>();
                return loader.Load();
            });
        }

        private void ConfigureControllers(ServiceCollection services)
        {
            services.AddTransient<CoolingConroller>();

            services.AddTransient<OutConroller>();

            services.AddSingleton<TemperatureController>();

            services.AddSingleton<RotationController>();
        }

        private void ConfigureFactories(ServiceCollection services)
        {
            services.AddSingleton<ZoneViewModelFactory>();
            services.AddSingleton<DriverViewModelFactory>();
            services.AddSingleton<PasswordWindowFactory>();

            services.AddSingleton<Func<MockPort>>(sp =>
            {
                return () => sp.GetRequiredService<MockPort>();
            });
            services.AddSingleton<Func<PortModule>>(sp =>
            {
                return () => sp.GetRequiredService<PortModule>();
            });

            services.AddSingleton<Func<HeaterModule>>(sp => {
                return () => sp.GetRequiredService<HeaterModule>();
            });
        }

        private void ConfigureWindow(ServiceCollection services)
        {
            services.AddSingleton<FurnaceWindow>();
            services.AddTransient<SettingsWindow>();

            services.AddSingleton<Func<SettingsWindow>>(sp => () => sp.GetRequiredService<SettingsWindow>());
        }

        private void ConfigureViewModels(ServiceCollection services)
        {
            services.AddSingleton<FurnaceWindowViewModel>();

            services.AddTransient<PortViewModel>();
            services.AddTransient<SettingsViewModel>();
            services.AddTransient<SettingsWindowViewModel>();
            services.AddTransient<CoolingSystemViewModel>();
            services.AddTransient<OutSystemVievModel>();
        }

        private void ConfigureFurnaceModules(ServiceCollection services)
        {
            services.AddSingleton<IOManager>();
            services.AddTransient<MockPort>();
            services.AddTransient<PortModule>(sp =>
            {
                var ioManager = sp.GetRequiredService<IOManager>();

                SerialPort serialPort = new SerialPort();
                serialPort.BaudRate = 115200;

                PortModule module = new PortWrapper(serialPort, ioManager, sp.GetRequiredService<ILogger<PortWrapper>>());
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

            services.AddSingleton<TemperatureModule>(sp =>
            {
                var ioManager = sp.GetRequiredService<IOManager>();
                var port = sp.GetRequiredService<IPort>();
                TemperatureModule temperatureModule = new TemperatureModule(0x3, 0x0, ioManager);
                AddressFilter addressFilter = new AddressFilter(temperatureModule.GetAddressByte, temperatureModule);

                ioManager.RegisterModulePort(temperatureModule, port);
                ioManager.RegisterFilter(addressFilter);

                return temperatureModule;
            });

            services.AddTransient<HeaterModule>(sp =>
            {
                var ioManager = sp.GetRequiredService<IOManager>();
                var port = sp.GetRequiredService<IPort>();
                HeaterModule heaterModule = new HeaterModule(0x1, 0x0, ioManager);

                ioManager.RegisterModulePort(heaterModule, port);

                return heaterModule;
            });

            services.AddSingleton<CoolingModule>(sp =>
            {
                var ioManager = sp.GetRequiredService<IOManager>();
                var port = sp.GetRequiredService<IPort>();
                CoolingModule coolingModule = new CoolingModule(0x0, 0x0, ioManager);

                ioManager.RegisterModulePort(coolingModule, port);

                return coolingModule;
            });

            services.AddSingleton<OutModule>(sp =>
            {
                var ioManager = sp.GetRequiredService<IOManager>();
                var port = sp.GetRequiredService<IPort>();
                OutModule outModule = new OutModule(0x0, 0x0, ioManager);

                ioManager.RegisterModulePort(outModule, port);

                return outModule;
            });

            services.AddSingleton<DriverModule>(sp =>
            {
                var ioManager = sp.GetRequiredService<IOManager>();
                var port = sp.GetRequiredService<IPort>();
                var settings = sp.GetRequiredService<Settings>();
                DriverModule driverModule = new DriverModule(ioManager, settings.DriverAddress);
                AddressFilter addressFilter = new AddressFilter(driverModule.GetAddress, driverModule);

                ioManager.RegisterFilter(addressFilter);
                ioManager.RegisterModulePort(driverModule, port);

                return driverModule;
            });
        }

        protected override void OnExit(ExitEventArgs e)
        {
            // Важно: освобождаем провайдер сервисов
            (Services as IDisposable)?.Dispose();
            base.OnExit(e);
        }
    }
}
