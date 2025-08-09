using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vFalcon.ViewModels
{
    public class MenuViewModel : ViewModelBase
    {

        private string activateSectorText = string.Empty;

        public string ActivateSectorText
        {
            get => activateSectorText;
            set
            {
                activateSectorText = value;
                OnPropertyChanged(nameof(activateSectorText));
            }
        }
    }
}
