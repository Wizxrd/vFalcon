using System.Collections.ObjectModel;
using System.ComponentModel;
using vFalcon.Models;
using vFalcon.Mvvm;
namespace vFalcon.UI.ViewModels.Toolbar;

public class PositionsViewModel : ViewModelBase
{
    private ObservableCollection<string> facilities = new();
    private ObservableCollection<PositionViewModel> positions = new();
    private string selectedFacility = string.Empty;

    public ObservableCollection<string> Facilities
    {
        get => facilities;
        set
        {
            facilities = value;
            OnPropertyChanged();
        }
    }

    public ObservableCollection<PositionViewModel> Positions
    {
        get => positions;
        set
        {
            positions = value;
            OnPropertyChanged();
        }
    }

    public string SelectedFacility
    {
        get => selectedFacility;
        set
        {
            selectedFacility = value;
            OnPropertyChanged();
            if (!string.IsNullOrEmpty(value))
            {
                LoadPositions();
            }
        }
    }

    public PositionsViewModel()
    {
        LoadFacilities();
    }

    private void LoadFacilities()
    {
        foreach (string facility in Artcc.GetFacilities())
        {
            Facilities.Add(facility);
        }
    }

    private void LoadPositions()
    {
        Positions.Clear();
        string facilityId = Artcc.GetFacilityIdFromName(SelectedFacility);
        List<string> positions = Artcc.GetPositions(facilityId);
        foreach (string position in positions)
        {
            int lastIndex = position.LastIndexOf('-');
            if (lastIndex <= 0) return;
            string freq = position.Substring(lastIndex + 1).Trim();
            bool isChecked = false;
            foreach (string active in App.Profile.ActivePositions)
            {
                if (freq == active)
                {
                    isChecked = true;
                    break;
                }
            }
            PositionViewModel positionViewModel = new PositionViewModel
            {
                Name = position,
                IsChecked = isChecked
            };
            positionViewModel.PropertyChanged += PositionPropertyChanged;
            Positions.Add(positionViewModel);
        }
    } 

    private async void PositionPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(PositionViewModel.IsChecked)) return;
        if (sender is not PositionViewModel positionViewModel) return;

        var id = positionViewModel.Name;
        int lastDash = id.LastIndexOf('-');
        if (lastDash <= 0) return; // invalid format

        string freq = id.Substring(lastDash + 1).Trim();

        var positions = App.Profile.ActivePositions;
        var existing = positions.FirstOrDefault(t => string.Equals((string)t, freq, StringComparison.OrdinalIgnoreCase));

        if (existing == null)
        {
            positions.Add(freq);
        }
        else
        {
            existing.Remove();
        }
        App.MainWindowViewModel.PilotService.ForceRefresh = true;
        await App.MainWindowViewModel.PilotService.Refresh();
    }
}
