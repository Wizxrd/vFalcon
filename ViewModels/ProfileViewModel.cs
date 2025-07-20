using vFalcon.Models;

namespace vFalcon.ViewModels
{
    public class ProfileViewModel : ViewModelBase
    {
        public Profile Model { get; }

        private string _originalName;
        private bool _isSelected;

        public ProfileViewModel(Profile model)
        {
            Model = model;
            IsRenaming = false;
        }

        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                _isSelected = value;
                OnPropertyChanged();
            }
        }

        public string Name
        {
            get => Model.Name;
            set
            {
                if (Model.Name != value)
                {
                    Model.Name = value;
                    OnPropertyChanged();
                }
            }
        }

        public void BeginRename()
        {
            _originalName = Name;
            IsRenaming = true;
        }

        public string OriginalName => _originalName;

        private bool _isRenaming;
        public bool IsRenaming
        {
            get => _isRenaming;
            set
            {
                _isRenaming = value;
                OnPropertyChanged();
            }
        }
    }
}
