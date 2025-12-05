using FurnaceWPF.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using pechka4._8;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace FurnaceWPF.Views.Controls
{
    /// <summary>
    /// Логика взаимодействия для SettingsControll.xaml
    /// </summary>
    public partial class SettingsControl : UserControl
    {
        private SettingsViewModel _viewModel;
        public SettingsControl()
        {
            _viewModel = App.Services.GetRequiredService<SettingsViewModel>();
            this.DataContext = _viewModel;
            InitializeComponent();
        }

        private void Apply_Click(object sender, RoutedEventArgs e)
        {
            
        }

    }
}
