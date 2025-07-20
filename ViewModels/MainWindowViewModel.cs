using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using vFalcon.Models;

namespace vFalcon.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private string _zuluTime;
        private DispatcherTimer zuluTimer = new DispatcherTimer();
        public string ZuluTime
        {
            get => _zuluTime;
            set
            {
                if (_zuluTime != value)
                {
                    _zuluTime = value;
                    OnPropertyChanged();
                }
            }
        }

        public MainWindowViewModel()
        {
            StartZuluTimer();
        }

        private void StartZuluTimer()
        {
            ZuluTimerTick(null, null);
            zuluTimer.Interval = TimeSpan.FromMilliseconds(500);
            zuluTimer.Tick += ZuluTimerTick;
            zuluTimer.Start();
        }

        private void ZuluTimerTick(object? sender, EventArgs? e)
        {
            ZuluTime = DateTime.UtcNow.ToString("HHmm ss"); ;
        }
    }
}
