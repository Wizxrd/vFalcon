using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace vFalcon.Services.Interfaces
{
    public interface IDragService
    {
        public void StartDrag(FrameworkElement element, FrameworkElement radarView);
        public void StopDrag();
    }
}
