using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace uguimvvm.Input
{
    public interface ICommand
    {
        bool CanExecute(object parameter);
        void Execute(object parameter);
        event EventHandler CanExecuteChanged;
    }

    public class RelayCommand : ICommand
    {
        public RelayCommand(Action execute, Func<bool> canExecute = null)
        {
            _execute = execute;
            _canExecute = canExecute;
        }
        public bool CanExecute(object parameter)
        {
            if (_canExecute != null)
                return _canExecute();
            else
                return true;
        }

        public void Execute(object parameter)
        {
            _execute();
        }

        public event EventHandler CanExecuteChanged;
        
        private readonly Action _execute;
        private readonly Func<bool> _canExecute;
    }
}
