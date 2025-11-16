using System;
using System.Windows.Input;
using vFalcon.Commands;

namespace vFalcon.ViewModels
{
    public class ConfirmViewModel : ViewModelBase
    {
        // ========================================================
        //                      EVENTS
        // ========================================================
        public event Action<bool>? DialogResultSet;

        // ========================================================
        //                      PROPERTIES
        // ========================================================
        public string Message { get; set; }

        // ========================================================
        //                      COMMANDS
        // ========================================================
        public ICommand ConfirmCommand { get; }
        public ICommand CancelCommand { get; }

        // ========================================================
        //                  CONSTRUCTOR
        // ========================================================
        public ConfirmViewModel(string message)
        {
            Message = message;
            ConfirmCommand = new RelayCommand(OnConfirm);
            CancelCommand = new RelayCommand(OnCancel);
        }

        // ========================================================
        //                      METHODS
        // ========================================================
        private void OnConfirm() => DialogResultSet?.Invoke(true);

        private void OnCancel() => DialogResultSet?.Invoke(false);
    }
}
