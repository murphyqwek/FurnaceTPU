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
            bool isZone1Valid = Validation.GetHasError(Zone1TextBox) == false;
            bool isZone2Valid = Validation.GetHasError(Zone2TextBox) == false;
            bool isZone3Valid = Validation.GetHasError(Zone3TextBox) == false;

            if (isZone1Valid && isZone2Valid && isZone3Valid)
            {
                _viewModel.HasUnsavedChanges = false;
                BindingExpression binding1 = Zone1TextBox.GetBindingExpression(TextBox.TextProperty);
                binding1?.UpdateSource();

                BindingExpression binding2 = Zone2TextBox.GetBindingExpression(TextBox.TextProperty);
                binding2?.UpdateSource();

                BindingExpression binding3 = Zone3TextBox.GetBindingExpression(TextBox.TextProperty);
                binding3?.UpdateSource();

                MessageBox.Show("Настройки применены успешно!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("Невозможно применить настройки: исправьте ошибки ввода.",
                                "Ошибка валидации",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
            }
        }

    }
}
