using System;
using System.Windows.Input;

namespace SafeBox.Commands
{
    public class RelayCommand(Action commandAction, Func<bool> canExecute = null) : ICommand
    {
        private readonly Action _execute = commandAction;
        private readonly Func<bool> _canExecute = canExecute;

        public event EventHandler CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

        public bool CanExecute(object obj) => _canExecute == null || _canExecute();

        public void Execute(object obj) => _execute();
    }
}
