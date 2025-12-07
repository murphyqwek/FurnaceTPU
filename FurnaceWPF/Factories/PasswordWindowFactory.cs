using FurnaceWPF.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.RightsManagement;
using System.Text;
using System.Threading.Tasks;

namespace FurnaceWPF.Factories
{
    public class PasswordWindowFactory
    {
        public PasswordWindow GetPasswordWindow(string password)
        {
            PasswordViewModel passwordViewModel = new PasswordViewModel(password);

            PasswordWindow passwordWindow = new PasswordWindow(passwordViewModel);

            return passwordWindow;
        }
    }
}
