using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FurnaceWPF.Commands
{
    using System;
    using System.Windows.Input;

    public class RemoteCommand : ICommand
    {
        private readonly Action _execute;

        public RemoteCommand(Action execute)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        }

        public event EventHandler? CanExecuteChanged;

        public bool CanExecute(object? parameter) => true;

        public void Execute(object? param) => _execute();

    }

}
