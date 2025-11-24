using MaterialDesignThemes.Wpf;
using pechka4._8.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace pechka4._8
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public SolidColorBrush Zone1TempBrush { get; set; }
        public SolidColorBrush Zone2TempBrush { get; set; }
        public SolidColorBrush Zone3TempBrush { get; set; }
        private Storyboard _currentStoryboardC;
        private Storyboard _currentStoryboardB;
        private Storyboard _currentStoryboardA;

        public MainWindow()
        {
            InitializeComponent();

            this.Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            var mainWindowViewModel = (MainWindowViewModel)DataContext;
            mainWindowViewModel.DriveC.PropertyChanged += DriveC_PropertyChanged;
            mainWindowViewModel.DriveB.PropertyChanged += DriveB_PropertyChanged;
            mainWindowViewModel.DriveA.PropertyChanged += DriveA_PropertyChanged;

            UpdateCArrowsAnimation();
            UpdateBArrowsAnimation();
            UpdateAArrowsAnimation();
        }

        private void DriveA_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var vm = (pechka4._8.ViewModels.MainWindowViewModel)DataContext;
            if (e.PropertyName == nameof(vm.DriveA.IsLeft)
             || e.PropertyName == nameof(vm.DriveA.IsRight)
             || e.PropertyName == nameof(vm.DriveA.Speed))
            {
                UpdateAArrowsAnimation();
            }
        }

        private void UpdateAArrowsAnimation()
        {
            _currentStoryboardA?.Stop(this);

            var vm = (pechka4._8.ViewModels.MainWindowViewModel)DataContext;
            var driveA = vm.DriveA;

            string key = driveA.IsRight ? "RotorSpinRight" :
                         driveA.IsLeft ? "RotorSpinLeft" : null;

            if (key == null || driveA.Speed <= 0)
                return;

            var originalStoryboard = (Storyboard)FindResource(key);
            var storyboard = originalStoryboard.Clone();

            foreach (var timeline in storyboard.Children)
            {
                if (timeline is DoubleAnimation anim)
                {
                    anim.Duration = new Duration(driveA.AnimationDuration);
                }
            }

            storyboard.Begin(this, true);
            _currentStoryboardA = storyboard;
        }

        private void DriveB_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var vm = (pechka4._8.ViewModels.MainWindowViewModel)DataContext;
            if (e.PropertyName == nameof(vm.DriveB.IsLeft)
             || e.PropertyName == nameof(vm.DriveB.IsRight)
             || e.PropertyName == nameof(vm.DriveB.Speed))
            {
                UpdateBArrowsAnimation();
            }
        }


        private void UpdateBArrowsAnimation()
        {
            // Останавливаем текущую анимацию
            _currentStoryboardB?.Stop(this);

            var vm = (pechka4._8.ViewModels.MainWindowViewModel)DataContext;
            var driveB = vm.DriveB;

            // Определяем нужный Storyboard
            string key = driveB.IsRight ? "ArrowFlowRight" :
                        "ArrowFlowLeft";

            if (key == null || driveB.Speed <= 0)
                return; // Не анимируем

            // Копируем сториборд (чтобы не менять глобальный ресурс!)
            var originalStoryboard = (Storyboard)FindResource(key);
            var storyboard = originalStoryboard.Clone();

            // Меняем Duration в каждой DoubleAnimation
            foreach (var timeline in storyboard.Children)
            {
                if (timeline is DoubleAnimation anim)
                {
                    anim.Duration = new Duration(driveB.AnimationDuration);
                }
            }

            // Запускаем
            storyboard.Begin(this, true); // true = контролировать в окне
            _currentStoryboardB = storyboard;
        }

        private void DriveC_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var vm = (pechka4._8.ViewModels.MainWindowViewModel)DataContext;
            if (e.PropertyName == nameof(vm.DriveC.IsLeft)
             || e.PropertyName == nameof(vm.DriveC.IsRight)
             || e.PropertyName == nameof(vm.DriveC.Speed))
            {
                UpdateCArrowsAnimation();
            }
        }

        private void UpdateCArrowsAnimation()
        {
            // Останавливаем текущую анимацию
            _currentStoryboardC?.Stop(this);

            var vm = (pechka4._8.ViewModels.MainWindowViewModel)DataContext;
            var driveC = vm.DriveC;

            // Определяем нужный Storyboard
            string key = driveC.IsRight ? "ArrowFlowUp" :
                         driveC.IsLeft ? "ArrowFlowDown" : null;

            if (key == null || driveC.Speed <= 0)
                return; // Не анимируем

            // Копируем сториборд (чтобы не менять глобальный ресурс!)
            var originalStoryboard = (Storyboard)FindResource(key);
            var storyboard = originalStoryboard.Clone();

            // Меняем Duration в каждой DoubleAnimation
            foreach (var timeline in storyboard.Children)
            {
                if (timeline is DoubleAnimation anim)
                {
                    anim.Duration = new Duration(driveC.AnimationDuration);
                }
            }

            // Запускаем
            storyboard.Begin(this, true); // true = контролировать в окне
            _currentStoryboardC = storyboard;
        }



        private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            var textBox = sender as TextBox;
            string fullText = textBox.Text.Insert(textBox.SelectionStart, e.Text);
            // Разрешаем только числа и максимум одну точку, максимум 1 знак после точки
            e.Handled = !Regex.IsMatch(fullText, @"^\d{0,3}([.,]\d?)?$");
        }


        public SolidColorBrush InterpolateTempColor(double temp)
        {
            Color cold = (Color)ColorConverter.ConvertFromString("#1976D2");
            Color hot = (Color)ColorConverter.ConvertFromString("#FFD600");

            byte r = (byte)(cold.R + (hot.R - cold.R) * temp / 100);
            byte g = (byte)(cold.G + (hot.G - cold.G) * temp / 100);
            byte b = (byte)(cold.B + (hot.B - cold.B) * temp / 100);

            return new SolidColorBrush(Color.FromRgb(r, g, b));
        }
    }
}
