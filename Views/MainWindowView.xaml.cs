using AdonisUI.Controls;
using vFalcon.ViewModels;
using vFalcon.Models;
using System.Windows.Input;
using System.Windows;
using vFalcon.Views;
using vFalcon.Helpers;

namespace vFalcon
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindowView : AdonisWindow
    {
        private MasterToolbarView _masterToolbarView;
        private ProfileToolbarView _profileToolbarView;
        private CursorToolbarView _cursorToolbarView;
        private MapsToolbarView _mapsToolbarView;

        public MainWindowView(Profile profile)
        {
            InitializeComponent();
            var viewModel = new MainWindowViewModel(profile);
            DataContext = viewModel;
            viewModel.RequestSwitchProfile += OpenLoadProfileWindow;
            viewModel.RequestConfirmation += ShowConfirmDialog;
            viewModel.RequestSaveProfileAs += OpenSaveProfileAs;
            InitializeToolbar(viewModel);
            InitializeCursor(viewModel);
        }
        private void OpenLoadProfileWindow()
        {
            LoadProfileView loadProfileView = new LoadProfileView();
            this.Close();
            loadProfileView.ShowDialog();
        }

        private Task<bool> ShowConfirmDialog(string message)
        {
            var dialog = new ConfirmView(message)
            {
                Title = "Confirm",
                Owner = this
            };
            bool? result = dialog.ShowDialog();
            return Task.FromResult(result == true);
        }

        private void OpenSaveProfileAs()
        {
            if (DataContext is MainWindowViewModel viewModel)
            {
                SaveProfileAsView saveProfileView = new SaveProfileAsView(viewModel.profile);
                saveProfileView.Owner = this;
                saveProfileView.ShowDialog();
                viewModel.SetProfileName();
            }
        }

        private void InitializeToolbar(MainWindowViewModel viewModel)
        {
            _masterToolbarView = new MasterToolbarView();
            _profileToolbarView = new ProfileToolbarView();
            _cursorToolbarView = new CursorToolbarView();
            _mapsToolbarView = new MapsToolbarView();

            _cursorToolbarView.DataContext = viewModel;
            _mapsToolbarView.DataContext = viewModel;

            ToolbarRegion.Content = _masterToolbarView;

            _masterToolbarView.ProfileButtonClicked += () => ToolbarRegion.Content = _profileToolbarView;
            _masterToolbarView.CursorButtonClicked += () => ToolbarRegion.Content = _cursorToolbarView;
            _masterToolbarView.MapsButtonClicked += () => ToolbarRegion.Content = _mapsToolbarView;

            _profileToolbarView.ProfileBackButtonClicked += () => ToolbarRegion.Content = _masterToolbarView;
            _cursorToolbarView.CursorBackButtonClicked += () => ToolbarRegion.Content = _masterToolbarView;
            _mapsToolbarView.MapsBackButtonClicked += () => ToolbarRegion.Content = _masterToolbarView;

        }
        
        private void InitializeCursor(MainWindowViewModel viewModel)
        {
            UpdateCursor(viewModel.CursorSize);
            viewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(viewModel.CursorSize))
                {
                    UpdateCursor(viewModel.CursorSize);
                }
                else if (e.PropertyName == nameof(viewModel.IsCursorVisible))
                {
                    Cursor = viewModel.IsCursorVisible
                        ? new Cursor(Application.GetResourceStream(
                            new Uri($"pack://application:,,,/Resources/Cursors/Eram{viewModel.CursorSize}.cur")).Stream)
                        : Cursors.None;
                }
            };
        }

        private void UpdateCursor(string cursorSize)
        {
            var uri = new Uri($"pack://application:,,,/Resources/Cursors/Eram{cursorSize}.cur");
            this.Cursor = new Cursor(Application.GetResourceStream(uri).Stream);
        }

        private void ToolbarButtonClick(object sender, RoutedEventArgs e)
        {
            if (ToolbarRegion.IsVisible)
            {
                ToolbarRegion.Visibility = Visibility.Collapsed;
            }
            else
            {
                ToolbarRegion.Visibility = Visibility.Visible;
            }
        }
    }
}