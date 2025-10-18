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
            Zone1 = new ZoneViewModel("Зона 1", 75);
            Zone2 = new ZoneViewModel("Зона 2", 45);
            Zone3 = new ZoneViewModel("Зона 3", 18);
            DriveC = new DriveCViewModel();
            DriveB = new DriveBViewModel();
            DriveA = new DriveAViewModel();
        }
    }

    
}
