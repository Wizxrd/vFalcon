using System.Windows.Input;
using vFalcon.Commands;

namespace vFalcon.ViewModels
{
    public class ConfirmViewModel : ViewModelBase
    {
        public string Message { get; set; }

        public ICommand ConfirmCommand { get; }
        public ICommand CancelCommand { get; }

        public event Action<bool>? DialogResultSet;

        public ConfirmViewModel(string message)
        {
            Message = message;

            ConfirmCommand = new RelayCommand(OnConfirm);
            CancelCommand = new RelayCommand(OnCancel);
        }

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
