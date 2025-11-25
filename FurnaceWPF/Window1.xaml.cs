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
        private Storyboard _currentStoryboardA = new Storyboard();
        private Storyboard _currentStoryboardB = new Storyboard();
        private Storyboard _currentStoryboardC = new Storyboard();

        public Window1()
        {
            this.DataContext = App.Services.GetRequiredService<WindowViewModel>();

            InitializeComponent();
            var vm = this.DataContext as WindowViewModel;

            vm.DriveA.AnimationSettingsChangeed += () => UpdateAnimation(_currentStoryboardA, vm.DriveA, "RotorSpinRight", "RotorSpinLeft");
            vm.DriveB.AnimationSettingsChangeed += () => UpdateAnimation(_currentStoryboardB, vm.DriveB, "ArrowFlowRight", "ArrowFlowLeft");
            vm.DriveC.AnimationSettingsChangeed += () => UpdateAnimation(_currentStoryboardC, vm.DriveC, "ArrowFlowUp", "ArrowFlowDown");
        }


        private void UpdateAnimation(Storyboard driveStoryboard, DriverViewModel driveVM, string forwardKeyAnimation, string backwardKeyAnimation)
        {
            driveStoryboard.Stop(this);

            string key = driveVM.DirectionEnum switch
            {
                DriverDirectionEnum.Forward => forwardKeyAnimation,
                DriverDirectionEnum.Backward => backwardKeyAnimation,
                DriverDirectionEnum.Stop => "none",
                _ => "none"
            };

            if (key == "none" || driveVM.Speed <= 0)
                return;

            var originalStoryboard = (Storyboard)FindResource(key);
            var storyboard = originalStoryboard.Clone();

            foreach (var timeline in storyboard.Children)
            {
                if (timeline is DoubleAnimation anim)
                {
                    anim.Duration = new Duration(driveVM.AnimationDuration);
                }
            }

            storyboard.Begin(this, true);
            driveStoryboard = storyboard;
        }
    }
}
