using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vFalcon.ViewModels
{
    public class MapBrightnessButtonViewModel : ViewModelBase
    {
        public int Index { get; set; }
        public string LabelLine1 { get; set; } = string.Empty;
        private string labelLine2 = string.Empty;
        public string LabelLine2
        {
            get => labelLine2;
            set
            {
                labelLine2 = value;
                OnPropertyChanged();
            }
        }
        public int Row {  get; set; }
        public int Column { get; set; }
    }
}
