using System.Windows;
using vFalcon.Services.Interfaces;
using vFalcon.Views;

namespace vFalcon.Services
{
    public class DialogService : IDialogService
    {
        public bool ShowConfirmDialog(string title, string message)
        {
            var dialog = new ConfirmView(message)
            {
                Title = title,
                Owner = Application.Current.MainWindow
            };

            bool? result = dialog.ShowDialog();
            return result == true;
        }
    }
}
