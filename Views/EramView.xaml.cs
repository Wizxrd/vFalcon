using AdonisUI.Controls;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using vFalcon.Helpers;
using vFalcon.Models;
using vFalcon.ViewModels;
using vFalcon.Views;

namespace vFalcon
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class EramView : AdonisWindow
    {
        private MasterToolbarView _masterToolbarView;
        private ProfileToolbarView _profileToolbarView;
        private CursorToolbarView _cursorToolbarView;
        private MapsToolbarView _mapsToolbarView;
        private Profile profile;

        public EramView(Profile profile)
        {
            InitializeComponent();
            this.profile = profile;
            var viewModel = new EramViewModel(profile);
            DataContext = viewModel;
            InitializeProfileSettings();
            InitializeToolbar(viewModel);
            InitializeCursor(viewModel);
            viewModel.RequestSwitchProfile += OpenLoadProfileWindow;
            viewModel.RequestConfirmation += ShowConfirmDialog;
            viewModel.RequestSaveProfileAs += OpenSaveProfileAs;
        }

        private void InitializeProfileSettings()
        {
            this.Width = profile.WindowWidth;
            this.Height = profile.WindowHeight;
            this.Top = profile.WindowTop;
            this.Left = profile.WindowLeft;

            if (profile.IsMaximized)
            {
                this.WindowState = WindowState.Maximized;
            }
        }

        private void InitializeToolbar(EramViewModel viewModel)
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

        private void InitializeCursor(EramViewModel viewModel)
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
            if (DataContext is EramViewModel viewModel)
            {
                SaveProfileAsView saveProfileView = new SaveProfileAsView(viewModel.profile);
                saveProfileView.Owner = this;
                saveProfileView.ShowDialog();
                viewModel.SetProfileName();
            }
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

        private void EramWindowSizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (this.WindowState == WindowState.Normal)
            {
                profile.WindowWidth = (int)this.ActualWidth;
                profile.WindowHeight = (int)this.ActualHeight;
            }
        }

        private void EramWindowLocationChanged(object sender, EventArgs e)
        {
            if (this.WindowState == WindowState.Normal)
            {
                profile.WindowLeft = (int)this.Left;
                profile.WindowTop = (int)this.Top;
            }
        }

        private void EramWindowStateChanged(object sender, EventArgs e)
        {
            profile.IsMaximized = this.WindowState == WindowState.Maximized;
        }
    }
}