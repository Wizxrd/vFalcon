using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace vFalcon.Models
{
    public class MapFilter
    {
        public string Id { get; set; } = string.Empty;
        public int Index { get; set; }
        public string LabelLine1 { get; set; } = string.Empty;
        public string LabelLine2 { get; set; } = string.Empty;
        public int Row {  get; set; }
        public int Column { get; set; }
        public ICommand Command { get; set; }
        public bool IsActive { get; set; }
        public bool IsChecked { get; set; }
    }
}
