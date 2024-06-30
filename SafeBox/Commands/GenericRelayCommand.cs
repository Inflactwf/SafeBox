using System;
using System.Windows.Input;

namespace SafeBox.Commands
{
    public class RelayCommand<T>(Action<T> commandAction, Func<T, bool> canExecute = null) : ICommand
    {
        private readonly Action<T> _execute = commandAction;
        private readonly Func<T, bool> _canExecute = canExecute;

        public event EventHandler CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

        public bool CanExecute(object parameter) => _canExecute == null || _canExecute((T)parameter);

        public void Execute(object parameter) => _execute((T)parameter);
    }
}
