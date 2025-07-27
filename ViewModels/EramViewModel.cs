using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using vFalcon.Commands;
using vFalcon.Helpers;
using vFalcon.Models;
using vFalcon.Services.Interfaces;
using vFalcon.Views;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace vFalcon.ViewModels
{
    public class EramViewModel : ViewModelBase
    {
        private Profile profile;

        private double zuluPanelLeft;
        private double zuluPanelTop = double.NaN;
        private double zuluPanelRight = double.NaN;
        private double zuluPanelBottom = double.NaN;
        private double toolbarPanelLeft;
        private double toolbarPanelTop = double.NaN;
        private double toolbarPanelRight = double.NaN;
        private double toolbarPanelBottom = double.NaN;

        private object _masterToolbarContent;

        public ICommand ToolbarControlCommand { get; }
        public ICommand MasterToolbarCommand { get; }
        public string CursorSize
        {
            get => (string)profile.DisplayWindowSettings[0]["DisplaySettings"][0]["CursorSize"];
            set
            {
                if ((string)profile.DisplayWindowSettings[0]["DisplaySettings"][0]["CursorSize"] != value)
                {
                    profile.DisplayWindowSettings[0]["DisplaySettings"][0]["CursorSize"] = value;
                    OnPropertyChanged();
                }
            }
        }

        public object MasterToolbarContent
        {
            get => _masterToolbarContent;
            set { _masterToolbarContent = value; OnPropertyChanged(); }
        }

        public double ZuluPanelLeft
        {
            get => zuluPanelLeft;
            set { zuluPanelLeft = value; OnPropertyChanged(); }
        }

        public double ZuluPanelTop
        {
            get => zuluPanelTop;
            set { zuluPanelTop = value; OnPropertyChanged(); }
        }

        public double ZuluPanelRight
        {
            get => zuluPanelRight;
            set { zuluPanelRight = value; OnPropertyChanged(); }
        }

        public double ZuluPanelBottom
        {
            get => zuluPanelBottom;
            set { zuluPanelBottom = value; OnPropertyChanged(); }
        }

        private Thickness _toolbarMargin;
        public Thickness ToolbarMargin
        {
            get => _toolbarMargin;
            set { _toolbarMargin = value; OnPropertyChanged(); }
        }

        private HorizontalAlignment _toolbarHorizontalAlignment;
        public HorizontalAlignment ToolbarHorizontalAlignment
        {
            get => _toolbarHorizontalAlignment;
            set { _toolbarHorizontalAlignment = value; OnPropertyChanged(); }
        }

        private VerticalAlignment _toolbarVerticalAlignment;
        public VerticalAlignment ToolbarVerticalAlignment
        {
            get => _toolbarVerticalAlignment;
            set { _toolbarVerticalAlignment = value; OnPropertyChanged(); }
        }

        public bool IsMasterToolbarOpen
        {
            get => (bool)profile.DisplayWindowSettings[0]["DisplaySettings"][0]["MasterToolbarVisible"];
            set
            {
                profile.DisplayWindowSettings[0]["DisplaySettings"][0]["MasterToolbarVisible"] = value;
                OnPropertyChanged();
            }
        }

        private ToolbarControlView _toolbarContent;
        public ToolbarControlView ToolbarContent
        {
            get => _toolbarContent;
            set
            {
                _toolbarContent = value;
                OnPropertyChanged();
            }
        }

        public EramViewModel(Profile profile)
        {
            this.profile = profile;
            LoadProfileSettings();
            ToolbarContent = new ToolbarControlView(this);
            //ToolbarControlCommand = new RelayCommand(ToggleToolbarControl);
            MasterToolbarCommand = new RelayCommand(OnMasterToolbar);
            IsMasterToolbarOpen = !IsMasterToolbarOpen;
            OnMasterToolbar();
        }

        public void OnMasterToolbar()
        {
            if (!IsMasterToolbarOpen)
            {
                IsMasterToolbarOpen = !IsMasterToolbarOpen;
                MasterToolbarContent = new MasterToolbarView();
            }
            else
            {
                IsMasterToolbarOpen = !IsMasterToolbarOpen;
                MasterToolbarContent = null;
            }
        }

        private void LoadProfileSettings()
        {
            LoadTimeSettings();
            LoadTearoffs();
        }

        private void LoadTimeSettings()
        {
            JObject displaySettings = (JObject)profile.DisplayWindowSettings[0]["DisplaySettings"][0];
            JObject timeViewSettings = (JObject)displaySettings["TimeViewSettings"];

            double[] parts = ((string)timeViewSettings["Location"]["Location"])
                .Split(',')
                .Select(s => double.Parse(s, CultureInfo.InvariantCulture))
                .ToArray();

            string anchor = (string)timeViewSettings["Location"]["Anchor"];

            ZuluPanelRight = ZuluPanelBottom = double.NaN; // clear unused

            switch (anchor)
            {
                case "TopLeft":
                    ZuluPanelLeft = parts[0];
                    ZuluPanelTop = parts[1];
                    break;
                case "TopRight":
                    ZuluPanelRight = parts[0];
                    ZuluPanelTop = parts[1];
                    ZuluPanelLeft = double.NaN;
                    break;
                case "BottomLeft":
                    ZuluPanelLeft = parts[0];
                    ZuluPanelBottom = parts[1];
                    ZuluPanelTop = double.NaN;
                    break;
                case "BottomRight":
                    ZuluPanelRight = parts[0];
                    ZuluPanelBottom = parts[1];
                    ZuluPanelTop = ZuluPanelLeft = double.NaN;
                    break;
            }
        }

        private void LoadTearoffs()
        {
            JObject displaySettings = (JObject)profile.DisplayWindowSettings[0]["DisplaySettings"][0];
            JArray tearoffs = (JArray)displaySettings["Tearoffs"];
            if (tearoffs.Count == 0) return;

            foreach (JObject tearoff in tearoffs)
            {
                if ((string)tearoff["Type"] == "ToolbarControlMenu")
                {
                    double[] parts = ((string)tearoff["Location"]["Location"])
                        .Split(',')
                        .Select(s => double.Parse(s, CultureInfo.InvariantCulture))
                        .ToArray();

                    string anchor = (string)tearoff["Location"]["Anchor"];

                    switch (anchor)
                    {
                        case "TopLeft":
                            ToolbarHorizontalAlignment = HorizontalAlignment.Left;
                            ToolbarVerticalAlignment = VerticalAlignment.Top;
                            ToolbarMargin = new Thickness(parts[0], parts[1], 0, 0);
                            break;

                        case "TopRight":
                            ToolbarHorizontalAlignment = HorizontalAlignment.Right;
                            ToolbarVerticalAlignment = VerticalAlignment.Top;
                            ToolbarMargin = new Thickness(0, parts[1], parts[0], 0);
                            break;

                        case "BottomLeft":
                            ToolbarHorizontalAlignment = HorizontalAlignment.Left;
                            ToolbarVerticalAlignment = VerticalAlignment.Bottom;
                            ToolbarMargin = new Thickness(parts[0], 0, 0, parts[1]);
                            break;

                        case "BottomRight":
                            ToolbarHorizontalAlignment = HorizontalAlignment.Right;
                            ToolbarVerticalAlignment = VerticalAlignment.Bottom;
                            ToolbarMargin = new Thickness(0, 0, parts[0], parts[1]);
                            break;
                    }
                    return;
                }
            }
        }
    }
}
