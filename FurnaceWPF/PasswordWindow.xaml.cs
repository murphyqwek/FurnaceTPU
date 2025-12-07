using FurnaceWPF.ViewModels;
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
using System.Windows.Shapes;

namespace FurnaceWPF
{
    /// <summary>
    /// Логика взаимодействия для PasswordWindow.xaml
    /// </summary>
    public partial class PasswordWindow : Window
    {
        public PasswordViewModel _passwordViewModel;
        public PasswordWindow(PasswordViewModel passwordViewModel)
        {
            this._passwordViewModel = passwordViewModel;
            this.DataContext = _passwordViewModel;
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string enteredPassword = passwordBox.Password;

            _passwordViewModel.CheckPassword(enteredPassword);

            if (_passwordViewModel.IsCorrect)
            {
                this.Close();
            }
            else
            {
                MessageBox.Show("Неверный пароль!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
