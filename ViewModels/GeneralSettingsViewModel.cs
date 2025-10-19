using Microsoft.VisualBasic.Logging;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using vFalcon.Commands;
using vFalcon.Helpers;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TrayNotify;

namespace vFalcon.ViewModels
{
    public class GeneralSettingsViewModel : ViewModelBase
    {
        // ========================================================
        //                      FIELDS
        // ========================================================
        private readonly EramViewModel eramViewModel;
        private string selectedGeoMapSet;
        private bool isTopDownMode;
        private bool isRecordingAudioEnabled = false;
        private Visibility defaultVisbility = Visibility.Visible;
        private Visibility cancelButtonVisibility = Visibility.Collapsed;
        private bool isScanningForPtt = false;
        private string pttButton = string.Empty;
        private bool clearButtonEnabled = false;
        private Visibility starsDisplay = Visibility.Visible;

        // ========================================================
        //                      PROPERTIES
        // ========================================================

        public Visibility StarsDisplay
        {
            get => starsDisplay;
            set
            {
                starsDisplay = value;
                OnPropertyChanged();
            }
        }

        public bool ClearButtonEnabled
        {
            get => clearButtonEnabled;
            set
            {
                clearButtonEnabled = value;
                OnPropertyChanged();
            }
        }

        public string PttButton
        {
            get => pttButton;
            set
            {
                pttButton = value;
                if (string.IsNullOrWhiteSpace(value)) ClearButtonEnabled = false;
                else if (!value.Contains("Scanning")) ClearButtonEnabled = true;
                OnPropertyChanged();
            }
        }

        public Visibility DefaultVisbility
        {
            get => defaultVisbility;
            set
            {
                defaultVisbility = value;
                OnPropertyChanged();
            }
        }

        public Visibility CancelButtonVisibility
        {
            get => cancelButtonVisibility;
            set
            {
                cancelButtonVisibility = value;
                OnPropertyChanged();
            }
        }

        public bool IsRecordAudioEnabled
        {
            get => isRecordingAudioEnabled;
            set
            {
                isRecordingAudioEnabled = value;
                OnPropertyChanged();
            }
        }


        public bool RecordAudio
        {
            get => (bool)eramViewModel.profile.RecordAudio;
            set
            {
                if (eramViewModel.IsRecording) return;
                eramViewModel.profile.RecordAudio = value;
                IsRecordAudioEnabled = value;
                OnPropertyChanged();
            }
        }

        public float DatablockTextSize
        {
            get => (float)eramViewModel.profile.AppearanceSettings["DatablockFontSize"];
            set
            {
                if (eramViewModel.RadarViewModel.pilotRenderer == null) return;
                eramViewModel.RadarViewModel.pilotRenderer.starsDatablockPaint.TextSize = (float)value;
                eramViewModel.RadarViewModel.pilotRenderer.starsLimitedDatablockPaint.TextSize = (float)value;
                eramViewModel.RadarViewModel.pilotRenderer.fullDatablockTextPaint.TextSize = (float)value;
                eramViewModel.RadarViewModel.pilotRenderer.limitedDatablockTextPaint.TextSize = (float)value;
                datablockTextSize = (float)value;
                eramViewModel.DatablockTextSize = (float)value;
                eramViewModel.profile.AppearanceSettings["DatablockFontSize"] = (float)value;
                eramViewModel.RadarViewModel.Redraw();
                OnPropertyChanged();
            }
        }

        private static SKColor ScaleBrightness(SKColor baseColor, double percent)
        {
            var f = Math.Max(0.0, Math.Min(1.0, percent * 0.01));
            byte r = (byte)Math.Round(baseColor.Red * f);
            byte g = (byte)Math.Round(baseColor.Green * f);
            byte b = (byte)Math.Round(baseColor.Blue * f);
            return new SKColor(r, g, b, baseColor.Alpha);
        }

        private float datablockTextSize = 12f;
        private double fullDatablockBrightness = 100.0;
        private double limitedDatablockBrightness = 75.0;
        private int mapBrightness = 100;
        private double historyBrightness = 50.0;
        private int historyLength = 5;
        private int vectorLength = 1;

        public double FullDatablockBrightness
        {
            get => (double)eramViewModel.profile.AppearanceSettings["FullDatablockBrightness"];
            set
            {
                if (eramViewModel.RadarViewModel.pilotRenderer == null) return;
                fullDatablockBrightness = value;
                var eramColor = eramViewModel.RadarViewModel.pilotRenderer.PrimaryColor;
                var starsColor = eramViewModel.RadarViewModel.pilotRenderer.StarsFullDbColor;
                var eramScaled = ScaleBrightness(eramColor, value);
                var starsScaled = ScaleBrightness(starsColor, value);
                eramViewModel.RadarViewModel.pilotRenderer.starsDatablockPaint.Color = starsScaled;
                eramViewModel.RadarViewModel.pilotRenderer.fullDatablockTextPaint.Color = eramScaled;
                eramViewModel.RadarViewModel.pilotRenderer.mainPaint.Color = eramScaled;
                eramViewModel.RadarViewModel.Redraw();
                eramViewModel.FullDatablockBrightness = value;
                eramViewModel.profile.AppearanceSettings["FullDatablockBrightness"] = value;
                OnPropertyChanged();
            }
        }

        public double LimitedDatablockBrightness
        {
            get => (double)eramViewModel.profile.AppearanceSettings["LimitedDatablockBrightness"];
            set
            {
                if (eramViewModel.RadarViewModel.pilotRenderer == null) return;
                limitedDatablockBrightness = value;
                var eramColor = eramViewModel.RadarViewModel.pilotRenderer.PrimaryColor;
                var starsColor = eramViewModel.RadarViewModel.pilotRenderer.StarsLimDbColor;
                var eramScaled = ScaleBrightness(eramColor, value);
                var starsScaled = ScaleBrightness(starsColor, value);
                eramViewModel.RadarViewModel.pilotRenderer.starsLimitedDatablockPaint.Color = starsScaled;
                eramViewModel.RadarViewModel.pilotRenderer.limitedDatablockTextPaint.Color = eramScaled;
                eramViewModel.RadarViewModel.pilotRenderer.limitedPaint.Color = eramScaled;
                eramViewModel.RadarViewModel.Redraw();
                eramViewModel.LimitedDatablockBrightness = value;
                eramViewModel.profile.AppearanceSettings["LimitedDatablockBrightness"] = value;
                OnPropertyChanged();
            }
        }

        public int MapBrightness
        {
            get => (int)eramViewModel.profile.AppearanceSettings["MapBrightness"];
            set
            {
                if (eramViewModel.RadarViewModel.pilotRenderer == null) return;
                mapBrightness = value;
                eramViewModel.MapBrightness = value;
                eramViewModel.RadarViewModel.Redraw();
                eramViewModel.profile.AppearanceSettings["MapBrightness"] = value;
                OnPropertyChanged();
            }
        }

        public double HistoryBrightness
        {
            get => (double)eramViewModel.profile.AppearanceSettings["HistoryBrightness"];
            set
            {
                if (eramViewModel.RadarViewModel.pilotRenderer == null) return;
                historyBrightness = value;
                var baseColor = eramViewModel.RadarViewModel.pilotRenderer.PrimaryColor;
                var c = ScaleBrightness(baseColor, value);
                var pr = eramViewModel.RadarViewModel.pilotRenderer;
                eramViewModel.RadarViewModel.pilotRenderer.historyPaintSlant.Color = c;
                eramViewModel.RadarViewModel.pilotRenderer.historyPaintIBeam.Color = c;
                eramViewModel.RadarViewModel.pilotRenderer.historyPaintHBeam.Color = c;
                eramViewModel.RadarViewModel.Redraw();
                eramViewModel.HistoryBrightness = value;
                eramViewModel.profile.AppearanceSettings["HistoryBrightness"] = value;
                OnPropertyChanged();
            }
        }

        public int HistoryLength
        {
            get => (int)eramViewModel.profile.AppearanceSettings["HistoryLength"];
            set
            {
                if (eramViewModel.RadarViewModel.pilotRenderer == null) return;
                historyLength = value;
                eramViewModel.HistoryCount = value;
                eramViewModel.RadarViewModel.Redraw();
                eramViewModel.profile.AppearanceSettings["HistoryLength"] = value;
                OnPropertyChanged();
            }
        }

        private static int Snap(int v) => v <= 0 ? 0 : v <= 1 ? 1 : v <= 2 ? 2 : v <= 4 ? 4 : 8;
        public int VectorLength
        {
            get => (int)eramViewModel.profile.AppearanceSettings["VectorLength"];
            set
            {
                var newVal = Snap(value);
                if (vectorLength == newVal) return;
                vectorLength = newVal;
                eramViewModel.VelocityVector = newVal;
                eramViewModel.RadarViewModel.Redraw();
                eramViewModel.profile.AppearanceSettings["VectorLength"] = value;
                OnPropertyChanged();
            }
        }

        public string SelectedGeoMapSet
        {
            get => eramViewModel.profile.ActiveGeoMap;
            set
            {
                if (selectedGeoMapSet != value)
                {
                    selectedGeoMapSet = value;
                    eramViewModel.ActiveGeoMap = value;
                    eramViewModel.profile.ActiveGeoMap = value;
                    _ = eramViewModel.SwapGeoMapSet();
                    OnPropertyChanged();
                }
            }
        }

        public int SelectedLogLevel
        {
            get => eramViewModel.profile.LogLevel;
            set
            {
                Logger.LogLevelThreshold = (LogLevel)value;
                eramViewModel.profile.LogLevel = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<string> GeoMapSets { get; }

        public bool IsTopDownMode
        {
            get => eramViewModel.profile.TopDown;
            set
            {
                eramViewModel.TdmActive = value;
                eramViewModel.profile.TopDown = value;
                eramViewModel.RadarViewModel.Redraw();
                OnPropertyChanged(nameof(IsTopDownMode));
            }
        }

        // ========================================================
        //                      EVENTS
        // ========================================================
        public event Action? Close;
        // ========================================================
        //                      CONSTRUCTOR
        // ========================================================

        public int BackgroundValue
        {
            get => eramViewModel.BackgroundValue;
            set
            {
                eramViewModel.BackgroundValue = value;
                eramViewModel.profile.AppearanceSettings["Background"] = value;
                eramViewModel.UpdateBackground();
                OnPropertyChanged();
            }
        }

        public int BrightnessValue
        {
            get => eramViewModel.BrightnessValue;
            set
            {
                eramViewModel.BrightnessValue = value;
                eramViewModel.profile.AppearanceSettings["Backlight"] = value;
                eramViewModel.UpdateBackground();
                OnPropertyChanged();
            }
        }

        public ICommand SetPttCommand { get; set; }
        public ICommand CancelPttScanCommand { get; set; }
        public ICommand ClearPttCommand { get; set; }

        public GeneralSettingsViewModel(EramViewModel eramViewModel)
        {
            this.eramViewModel = eramViewModel;
            if (eramViewModel.profile.DisplayType == "STARS") StarsDisplay = Visibility.Collapsed;
            BackgroundValue = (int)eramViewModel.profile.AppearanceSettings["Background"];
            BrightnessValue = (int)eramViewModel.profile.AppearanceSettings["Backlight"];
            DatablockTextSize = (float)eramViewModel.profile.AppearanceSettings["DatablockFontSize"];
            FullDatablockBrightness = (double)eramViewModel.profile.AppearanceSettings["FullDatablockBrightness"];
            LimitedDatablockBrightness = (double)eramViewModel.profile.AppearanceSettings["LimitedDatablockBrightness"];
            MapBrightness = (int)eramViewModel.profile.AppearanceSettings["MapBrightness"];
            HistoryBrightness = (double)eramViewModel.profile.AppearanceSettings["HistoryBrightness"];
            HistoryLength = (int)eramViewModel.profile.AppearanceSettings["HistoryLength"];
            VectorLength = (int)eramViewModel.profile.AppearanceSettings["VectorLength"];
            RecordAudio = (bool)eramViewModel.profile.RecordAudio;
            if(eramViewModel.profile.DisplayType == "ERAM")
            {
                GeoMapSets = new ObservableCollection<string>(
                    eramViewModel.ArtccGeoMaps.Select(g => g["name"]?.ToString()).Where(n => !string.IsNullOrEmpty(n))
                );
            }
            eramViewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(eramViewModel.TdmActive))
                {
                    OnPropertyChanged(nameof(IsTopDownMode));
                }
                if (e.PropertyName == nameof(eramViewModel.VelocityVector))
                {
                    OnPropertyChanged(nameof(VectorLength));
                }
                if (e.PropertyName == nameof(eramViewModel.isRecording))
                {
                    OnPropertyChanged(nameof(RecordAudio));
                }
            };

            selectedGeoMapSet = eramViewModel.ActiveGeoMap;


            SetPttCommand = new RelayCommand(OnSetPttCommand);
            CancelPttScanCommand = new RelayCommand(OnCancelPttScanCommand);
            ClearPttCommand = new RelayCommand(OnClearPttCommand);
            SetPttKey();
        }

        private void SetPttKey()
        {
            int? vk = eramViewModel.profile.PttKey;
            if (vk.HasValue && vk.Value > 0)
            {
                Key key = KeyInterop.KeyFromVirtualKey(vk.Value);
                PttButton = key.ToString();
            }
        }

        public void OnClearPttCommand()
        {
            ClearButtonEnabled = false;
            PttButton = string.Empty;
            eramViewModel.profile.PttKey = null;
        }

        string pttBefore = string.Empty;

        public void OnCancelPttScanCommand()
        {
            DefaultVisbility = Visibility.Visible;
            CancelButtonVisibility = Visibility.Collapsed;
            IsScanningForPtt = false;
            if (!PttFound)
            {
                if (PttButton.Contains("Scanning")) PttButton = pttBefore;
            }
            PttFound = false;
        }

        public bool IsScanningForPtt = false;
        public bool PttFound = false;
        private void OnSetPttCommand()
        {
            if (eramViewModel.IsRecording) return;
            IsScanningForPtt = true;
            pttBefore = PttButton;
            PttButton = "Scanning...";
            DefaultVisbility = Visibility.Collapsed;
            CancelButtonVisibility = Visibility.Visible;
        }
    }
}
