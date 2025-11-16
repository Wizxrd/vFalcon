using System;
using System.Collections.Generic;
using System.DirectoryServices.ActiveDirectory;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using vFalcon.Mvvm;
namespace vFalcon.UI.ViewModels.Controls;

public class ReplayControlsViewModel : ViewModelBase
{
    private int maximumSliderValue = 0;
    private int sliderValueTick = 0;
    private string elapsedTimeTick = string.Empty;
    public int MaximumSliderValue
    {
        get => App.MainWindowViewModel.PlaybackService.GetTotalTickCount();
        set
        {
            maximumSliderValue = value;
            OnPropertyChanged();
        }
    }
    public int SliderValueTick
    {
        get => sliderValueTick;
        set
        {
            sliderValueTick = value;
            App.MainWindowViewModel.PlaybackService.Tick = value;
            OnPropertyChanged();
        }
    }
    public string ElapsedTimeTick
    {
        get => elapsedTimeTick;
        set
        {
            elapsedTimeTick = value;
            OnPropertyChanged();
        }
    }
}
