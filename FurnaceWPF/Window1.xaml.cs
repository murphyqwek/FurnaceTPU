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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace FurnaceWPF
{
    /// <summary>
    /// Логика взаимодействия для Window1.xaml
    /// </summary>
    public partial class Window1 : Window
    {
        private Storyboard _currentStoryboardB;

        public Window1()
        {
            InitializeComponent();

            // Останавливаем текущую анимацию
            _currentStoryboardB?.Stop(this);

            // Определяем нужный Storyboard
            string key = "ArrowFlowRight";

            // Копируем сториборд (чтобы не менять глобальный ресурс!)
            var originalStoryboard = (Storyboard)FindResource(key);
            var storyboard = originalStoryboard.Clone();

            // Меняем Duration в каждой DoubleAnimation
            foreach (var timeline in storyboard.Children)
            {
                if (timeline is DoubleAnimation anim)
                {
                    anim.Duration = new Duration(TimeSpan.FromSeconds(1));
                }
            }

            // Запускаем
            storyboard.Begin(this, true); // true = контролировать в окне
            _currentStoryboardB = storyboard;
        }
    }
}
