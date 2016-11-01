using System;
using System.Windows.Input;

namespace GeoCoder
{
    public class DelegatingCommand : ICommand
    {
        private readonly Action _action;

        public DelegatingCommand(Action action)
        {
            _action = action;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            _action();
        }

        public event EventHandler CanExecuteChanged
        {
            add {  }
            remove { }
        }
    }
}