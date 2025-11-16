using vFalcon.Mvvm;
namespace vFalcon.UI.ViewModels.Toolbar;

public class PositionViewModel : ViewModelBase
{
    private string name = string.Empty;
    private bool isChecked;

    public string Name
    {
        get => name;
        set
        {
            name = value;
            OnPropertyChanged();
        }
    }

    public bool IsChecked
    {
        get => isChecked;
        set
        {
            if (isChecked != value)
            {
                isChecked = value;
                OnPropertyChanged();
            }
        }
    }
}
