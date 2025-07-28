using System.Windows.Input;
using vFalcon.Commands;

namespace vFalcon.ViewModels
{
    public class ConfirmViewModel : ViewModelBase
    {
        // Events
        public event Action<bool>? DialogResultSet;

        // Commands
        public ICommand ConfirmCommand { get; }
        public ICommand CancelCommand { get; }

        // Properties
        public string Message { get; set; }

        // Constructor
        public ConfirmViewModel(string message)
        {
            Message = message;

            ConfirmCommand = new RelayCommand(OnConfirm);
            CancelCommand = new RelayCommand(OnCancel);
        }

        // Methods
        private void OnConfirm()
        {
            DialogResultSet?.Invoke(true);
        }

        private void OnCancel()
        {
            DialogResultSet?.Invoke(false);
        }
    }
}
