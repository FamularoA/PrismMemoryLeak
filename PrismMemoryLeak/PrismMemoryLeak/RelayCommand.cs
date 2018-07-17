using System;
using System.Windows.Input;

namespace PrismMemoryLeak
{
    public class RelayCommand : ICommand
    {
        #region Fields

        private readonly Action<object> execute;
        private readonly Func<object, bool> canExecute;

        #endregion

        #region Constructors

        public RelayCommand(Action<object> execute,
            Func<object, bool> canExecute = null)
        {
            this.execute = execute;
            this.canExecute = canExecute;
        }

        #endregion

        #region Interface Implementations

        public event EventHandler CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

        public bool CanExecute(object parameter)
        {
            return canExecute == null || canExecute(parameter);
        }

        public void Execute(object parameter)
        {
            execute(parameter);
        }

        #endregion
    }
}