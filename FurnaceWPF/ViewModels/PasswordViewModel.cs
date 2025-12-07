using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace FurnaceWPF.ViewModels
{
    public class PasswordViewModel : BaseObservable
    {
        public bool IsCorrect { get; private set; }

        private string _correctPassword;

        public PasswordViewModel(string password)
        {
            _correctPassword = password;
        }

        public void CheckPassword(string password)
        {
            IsCorrect = password == _correctPassword;
        }
    }
}
