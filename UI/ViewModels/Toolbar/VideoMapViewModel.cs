using System.Windows.Input;
using vFalcon.Mvvm;
namespace vFalcon.UI.ViewModels.Toolbar
{
    public class VideoMapViewModel : ViewModelBase
    {
        private string name = string.Empty;
        private bool isChecked;
        private string starsId = string.Empty;

        public string Name
        {
            get => name;
            set
            {
                name = value;
                OnPropertyChanged();
            }
        }

        public bool IsChecked
        {
            get => isChecked;
            set
            {
                isChecked = value;
                OnPropertyChanged();
            }
        }

        public string StarsId
        {
            get => starsId;
            set
            {
                starsId = value;
                OnPropertyChanged();
            }
        }

        public ICommand Command { get; set; }
    }
}
