using vFalcon.Models;

namespace vFalcon.ViewModels
{
    public class ProfileViewModel : ViewModelBase
    {
        // ========================================================
        //                      FIELDS
        // ========================================================
        private string _originalName = string.Empty;
        private bool _isSelected;
        private bool _isRenaming;

        // ========================================================
        //                      PROPERTIES
        // ========================================================
        public Profile Model { get; }
        public string OriginalName => _originalName;

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

        public bool IsSelected
        {
            get => _isSelected;
            set { _isSelected = value; OnPropertyChanged(); }
        }

        public bool IsRenaming
        {
            get => _isRenaming;
            set { _isRenaming = value; OnPropertyChanged(); }
        }

        // ========================================================
        //                  CONSTRUCTOR
        // ========================================================
        public ProfileViewModel(Profile model)
        {
            Model = model;
            IsRenaming = false;
        }

        // ========================================================
        //                      METHODS
        // ========================================================
        public void BeginRename()
        {
            _originalName = Name;
            IsRenaming = true;
        }
    }
}
