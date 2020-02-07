using System;
#if UNITY_WSA || !NET_LEGACY
using System.Windows.Input;
#else
namespace uguimvvm
{
    /// <summary>
    /// Defines a command.
    /// </summary>
    public interface ICommand
    {
        /// <summary>
        /// Defines the method that determines whether the command can execute in its current state.
        /// </summary>
        /// <param name="parameter">Data used by the command. If the command does not require data to be passed, this object can be set to <c>null</c>.</param>
        /// <returns><c>true</c> if this command can be executed; otherwise, <c>false</c>.</returns>
        bool CanExecute(object parameter);

        /// <summary>
        /// Defines the method to be called when the command is invoked.
        /// </summary>
        /// <param name="parameter">Data used by the command. If the command does not require data to be passed, this object can be set to <c>null</c>.</param>
        void Execute(object parameter);

        /// <summary>
        /// Occurs when changes occur that affect whether or not the command should execute.
        /// </summary>
        event EventHandler CanExecuteChanged;
    }
}
#endif

namespace uguimvvm
{
    /// <summary>
    /// A command whose purpose is to relay its functionality to other objects by invoking delegates.
    /// </summary>
    public class RelayCommand : ICommand
    {
        private readonly Action _execute;
        private readonly Func<bool> _canExecute;

        /// <summary>
        /// Creates a new command.
        /// </summary>
        /// <param name="execute">The delegate to be invoked for the <see cref="Execute(object)"/> method.</param>
        /// <param name="canExecute">The delegate to be invoked for the <see cref="CanExecute(object)"/> method.  If not provided <see cref="CanExecute(object)"/> will always return <c>true</c>.</param>
        public RelayCommand(Action execute, Func<bool> canExecute = null)
        {
            _execute = execute;
            _canExecute = canExecute;
        }

        /// <inheritdoc />
        public bool CanExecute(object parameter)
        {
            return _canExecute == null || _canExecute();
        }

        /// <inheritdoc />
        public void Execute(object parameter)
        {
            _execute();
        }

        /// <summary>
        /// Raises <see cref="CanExecuteChanged"/> so every command invoker can requery to check if the command can execute.
        /// </summary>
        public void RaiseCanExecuteChanged()
        {
            if (CanExecuteChanged != null)
                CanExecuteChanged(this, EventArgs.Empty);
        }

        /// <summary>
        /// Occurs when changes occur that affect whether or not the command should execute.
        /// </summary>
        public event EventHandler CanExecuteChanged;
    }

    /// <summary>
    /// A command whose purpose is to relay its functionality to other objects by invoking delegates.
    /// </summary>
    public class RelayCommand<T> : ICommand
    {
        private readonly Action<T> _execute;
        private readonly Func<T, bool> _canExecute;

        /// <summary>
        /// Creates a new command.
        /// </summary>
        /// <param name="execute">The delegate to be invoked for the <see cref="Execute(object)"/> method.</param>
        /// <param name="canExecute">The delegate to be invoked for the <see cref="CanExecute(object)"/> method.  If not provided <see cref="CanExecute(object)"/> will always return <c>true</c>.</param>
        public RelayCommand(Action<T> execute, Func<T, bool> canExecute = null)
        {
            _execute = execute;
            _canExecute = canExecute;
        }

        /// <inheritdoc />
        public bool CanExecute(object parameter)
        {
            if (_canExecute == null) return true;

            if (parameter is T)
                return _canExecute((T)parameter);
            return false;
        }

        /// <inheritdoc />
        public void Execute(object parameter)
        {
            if (parameter is T)
                _execute((T)parameter);
        }

        /// <summary>
        /// Raises <see cref="CanExecuteChanged"/> so every command invoker can requery to check if the command can execute.
        /// </summary>
        public void RaiseCanExecuteChanged()
        {
            if (CanExecuteChanged != null)
                CanExecuteChanged(this, EventArgs.Empty);
        }

        /// <summary>
        /// Occurs when changes occur that affect whether or not the command should execute.
        /// </summary>
        public event EventHandler CanExecuteChanged;
    }
}
