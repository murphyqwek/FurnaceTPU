using pechka4._8.Helpers;
using System.Windows.Input;

namespace pechka4._8.ViewModels
{
    public class MainWindowViewModel : ObservableObject
    {
        public ZoneViewModel Zone1 { get; }
        public ZoneViewModel Zone2 { get; }
        public ZoneViewModel Zone3 { get; }
        public DriveCViewModel DriveC { get; }
        public DriveBViewModel DriveB { get; }
        public DriveAViewModel DriveA { get; }

        public MainWindowViewModel()
        {
            DriveC = new DriveCViewModel();
            DriveB = new DriveBViewModel();
            DriveA = new DriveAViewModel();
        }
    }

    
}
