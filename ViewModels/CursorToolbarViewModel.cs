using System;
using System.Collections.Generic;
using System.Configuration;
using System.DirectoryServices.ActiveDirectory;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using vFalcon.Commands;

namespace vFalcon.ViewModels
{
    public class CursorToolbarViewModel : ViewModelBase
    {
        private EramViewModel eramViewModel;

        public int CursorSize
        {
            get => eramViewModel.CursorSize;
            set
            {
                eramViewModel.CursorSize = value;
                OnPropertyChanged();
            }
        }

        public ICommand CursorBackCommand { get; }

        public CursorToolbarViewModel(EramViewModel eramViewModel)
        {
            this.eramViewModel = eramViewModel;
            CursorBackCommand = new RelayCommand(() => eramViewModel.OnCursorCommand());
        }
    }
}
