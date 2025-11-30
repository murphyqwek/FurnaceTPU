using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FurnaceWPF.ViewModels
{
    public class SettingsWindowViewModel
    {
        public SettingsViewModel SettingsControlViewModel { get; set; }

        public SettingsWindowViewModel(SettingsViewModel settingsController)
        {
            SettingsControlViewModel = settingsController;
        }
    }
}
