using System.Windows.Input;
using vFalcon.Commands;

namespace vFalcon.ViewModels
{
    public class CursorToolbarViewModel : ViewModelBase
    {
        // ========================================================
        //                      FIELDS
        // ========================================================
        private readonly EramViewModel eramViewModel;

        // ========================================================
        //                      PROPERTIES
        // ========================================================
        public int CursorSize
        {
            get => eramViewModel.CursorSize;
            set
            {
                eramViewModel.CursorSize = value;
                OnPropertyChanged();
            }
        }

        // ========================================================
        //                      COMMANDS
        // ========================================================
        public ICommand CursorBackCommand { get; }

        // ========================================================
        //                  CONSTRUCTOR
        // ========================================================
        public CursorToolbarViewModel(EramViewModel eramViewModel)
        {
            this.eramViewModel = eramViewModel;
            CursorBackCommand = new RelayCommand(() => eramViewModel.OnCursorCommand());
        }
    }
}
