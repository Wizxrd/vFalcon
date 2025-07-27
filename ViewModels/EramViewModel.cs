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

        private double toolbarControlLeft;
        private double toolbarControlTop;
        private double toolbarControlRight;
        private double toolbarControlBottom;

        private object _masterToolbarContent;

        public double ToolbarControlLeft
        {
            get => toolbarControlLeft;
            set
            {
                toolbarControlLeft = value;
                OnPropertyChanged();
            }
        }

        public double ToolbarControlRight
        {
            get => toolbarControlRight;
            set
            {
                toolbarControlRight = value;
                OnPropertyChanged();
            }
        }

        public double ToolbarControlTop
        {
            get => toolbarControlTop;
            set
            {
                toolbarControlTop = value;
                OnPropertyChanged();
            }
        }

        public double ToolbarControlBottom
        {
            get => toolbarControlBottom;
            set
            {
                toolbarControlBottom = value;
                OnPropertyChanged();
            }
        }

        public void OnToolbarButtonMeasured(double buttonWidth)
        {
            if (!double.IsNaN(ToolbarControlRight))
            {
                ToolbarControlRight = buttonWidth;
                Logger.Debug("OnToolbarButtonMeasured", $"buttonWidth={buttonWidth}");
            }
        }

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
                            ToolbarControlLeft = parts[0];
                            ToolbarControlTop = parts[1];
                            break;
                        case "TopRight":
                            ToolbarControlRight += parts[0];
                            ToolbarControlTop = parts[1];
                            ToolbarControlLeft = double.NaN;
                            break;
                        case "BottomLeft":
                            ToolbarControlLeft = parts[0];
                            ToolbarControlBottom = parts[1];
                            ToolbarControlTop = double.NaN;
                            break;
                        case "BottomRight":
                            ToolbarControlRight += parts[0];
                            ToolbarControlBottom = parts[1];
                            ToolbarControlTop = ToolbarControlLeft = double.NaN;
                            break;
                    }
                    return;
                }
            }
        }
    }
}
