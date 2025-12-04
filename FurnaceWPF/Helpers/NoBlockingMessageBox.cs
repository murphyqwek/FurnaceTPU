using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace FurnaceWPF.Helpers
{
    public static class NoBlockingMessageBox
    {
        public static void ShowError(string message)
        {
            Task.Run(() =>
            {
                MessageBox.Show(message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            });
        }
    }
}
