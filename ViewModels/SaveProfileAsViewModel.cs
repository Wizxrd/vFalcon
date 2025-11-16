using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using vFalcon.Commands;
using vFalcon.Services.Service;
using MessageBox = vFalcon.Services.Service.MessageBoxService;

namespace vFalcon.ViewModels
{
    public class SaveProfileAsViewModel : ViewModelBase
    {
        private EramViewModel eramViewModel;
        private string profileName = string.Empty;
        public bool IsProfileSavable => !string.IsNullOrEmpty(ProfileName);

        public string ProfileName
        {
            get => profileName;
            set
            {
                if (profileName == value) return;
                profileName = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsProfileSavable));
            }
        }

        public ICommand SaveAsCommand { get; set; }
        public ICommand CancelCommand { get; set; }
        public event Action? Close;

        public SaveProfileAsViewModel(EramViewModel eramViewModel)
        {
            this.eramViewModel = eramViewModel;
            SaveAsCommand = new RelayCommand(OnSaveAsCommand);
            CancelCommand = new RelayCommand(() => Close?.Invoke());
        }

        private ProfileService profileService = new();

        private async void OnSaveAsCommand()
        {
            await profileService.SaveAs(eramViewModel.profile, ProfileName);
            Close?.Invoke();
        }
    }
}
