using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
    /// Логика взаимодействия для DriverControl.xaml
    /// </summary>
    public partial class DriverControl : UserControl
    {
        public DriverControl()
        {
            InitializeComponent();
        }

        private void Input_TextPrewview(object sender, TextCompositionEventArgs e)
        {
            var textBox = sender as TextBox;
            string fullText = textBox.Text.Insert(textBox.SelectionStart, e.Text);
            // Разрешаем только числа и максимум одну точку, максимум 1 знак после точки
            e.Handled = !Regex.IsMatch(fullText, @"^\d{0,3}([.,]\d?)?$");
        }

    }
}
