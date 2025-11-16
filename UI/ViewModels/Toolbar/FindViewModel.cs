using System.Windows.Input;
using vFalcon.Mvvm;
namespace vFalcon.UI.ViewModels.Toolbar;

public class FindViewModel : ViewModelBase
{
    private string findText = string.Empty;

    public string FindText
    {
        get => findText;
        set
        {
            findText = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(FindButtonEnabled));
        }
    }
    public bool FindButtonEnabled => !string.IsNullOrEmpty(findText) ? true : false;

    public ICommand FindCommand { get; set; }
    public ICommand ClearFindCommand { get; set; }

    public FindViewModel()
    {
        FindCommand = new RelayCommand(OnFindCommand);
        ClearFindCommand = new RelayCommand(OnClearFindCommand);
    }

    private void OnFindCommand()
    {
        if (!App.MainWindowViewModel.FindService.Started) App.MainWindowViewModel.FindService.Start();
        App.MainWindowViewModel.FindService.ThingsToFind.Add(FindText);
        FindText = string.Empty;
    }

    private void OnClearFindCommand()
    {
        App.MainWindowViewModel.FindService.Stop();
        FindText = string.Empty;
        App.MainWindowViewModel.FindService.ThingsToFind.Clear();
    }
}
