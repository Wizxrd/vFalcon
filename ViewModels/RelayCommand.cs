using System;
using System.Windows.Input;

namespace vFalcon.ViewModels
{
    public class RelayCommand : ICommand
    {
        private readonly Action<object> execute;
        private readonly Func<object, bool> canExecute;

        private event EventHandler? _canExecuteChanged;

        public event EventHandler? CanExecuteChanged
        {
            add => _canExecuteChanged += value;
            remove => _canExecuteChanged -= value;
        }

        public RelayCommand(Action<object> execute, Func<object, bool>? canExecute = null)
        {
            this.execute = execute;
            this.canExecute = canExecute ?? (_ => true);
        }

        public RelayCommand(Action execute, Func<bool>? canExecute = null)
        {
            this.execute = _ => execute();
            this.canExecute = canExecute != null ? _ => canExecute() : _ => true;
        }

        public bool CanExecute(object? parameter) => canExecute(parameter);
        public void Execute(object? parameter) => execute(parameter);

        public void RaiseCanExecuteChanged() => _canExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
}
