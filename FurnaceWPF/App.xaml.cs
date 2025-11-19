using FurnaceCore.IOManager;
using Microsoft.Extensions.DependencyInjection;
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

            // показать окно
            var window = Services.GetRequiredService<MainWindow>();
            window.Show();
        }

        private void ConfigureServices(ServiceCollection services)
        {
            /* Сервисы
            services.AddSingleton<IOManager>();
            services.AddSingleton<TemperatureModule>();
            services.AddSingleton<ModbusAddressFilter>();

            // ViewModels
            services.AddSingleton<MainViewModel>();
            services.AddTransient<SettingsViewModel>();
            */
            // Views
            services.AddTransient<MainWindow>();
        }

        private void ConfigureFurnaceModules()
        {
            
        }
    }
}
