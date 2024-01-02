using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;
using NanobionicsTestVirusDesktop.Commands;
using NanobionicsTestVirusDesktop.Context;
using NanobionicsTestVirusDesktop.Models;
using NanobionicsTestVirusDesktop.Views;
using NanobionicsTestVirusLibrary;
using Newtonsoft.Json.Linq;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Legends;
using OxyPlot.Series;


namespace NanobionicsTestVirusDesktop.ViewModels
{
    public class MeasureViewModel : BaseViewModel
    {
        readonly DBContext _dbcontext;
        public PlotModel CimModel { get; private set; }
        public RelayCommand AddTypeCommand { get; private set; }
        public RelayCommand DeleteTypeCommand { get; private set; }
        public RelayCommand ExecAnalyze { get; private set; }
        public RelayCommand SaveTextConsole { get; private set; }
        public RelayCommand CopyTextConsole { get; private set; }
        public RelayCommand CutTextConsole { get; private set; }
        public RelayCommand ClearTextConsole { get; private set; }
        public RelayCommand CutOffMaxDownValue { get; private set; }
        public RelayCommand CutOffMaxUpValue { get; private set; }
        public RelayCommand CutOffMinDownValue { get; private set; }
        public RelayCommand CutOffMinUpValue { get; private set; }
        public RelayCommand FreqCutMinUpValue { get; private set; }
        public RelayCommand FreqCutMinDownValue { get; private set; }
        public RelayCommand FreqCutMaxUpValue { get; private set; }
        public RelayCommand FreqCutMaxDownValue { get; private set; }
        public RelayCommand ShowPlotWindow { get; private set; }

        private ObservableCollection<DataMeasure> _dataMeasures;
        public ObservableCollection<DataMeasure> DataMeasures
        {
            get => _dataMeasures;
            set
            {
                _dataMeasures = value;
                OnPropertyChanged(nameof(DataMeasures));
            }
        }

        private ObservableCollection<FileMeasure> _fileMeasures;
        public ObservableCollection<FileMeasure> FileMeasures
        {
            get => _fileMeasures;
            set
            {
                _fileMeasures = value;
                OnPropertyChanged(nameof(FileMeasures));
                AddTypeCommand = new RelayCommand(OnAdd, CanExecute);
                DeleteTypeCommand = new RelayCommand(OnRemove, CanExecuteAnalizeDelete);
                ExecAnalyze = new RelayCommand(OnAnalyze, CanExecuteAnalizeDelete);
            }
        }

        private PlotWindow _pw = null;

        private FileMeasure _selectedItem;
        public FileMeasure SelectedItem
        {
            get => _selectedItem;
            set
            {
                if (_selectedItem != value)
                {
                    _selectedItem = value;
                    if (_selectedItem != null)
                    {
                        DM1 = _selectedItem.DataMeasures.ElementAt(0);
                        DM2 = _selectedItem.DataMeasures.ElementAt(1);
                        StrCutOffMax = _selectedItem.CutOffMax.ToString();
                        StrCutOffMin = _selectedItem.CutOffMin.ToString();
                        StrFreqCutMin = _selectedItem.FreqCutMin.ToString();
                        StrFreqCutMax = _selectedItem.FreqCutMax.ToString();
                        LoadDataPlot(SelectedItem.DataMeasures.ElementAt(0).Values, SelectedItem.DataMeasures.ElementAt(1).Values);
                    }
                    else
                    {
                        DM1 = null;
                        DM2 = null;
                        CimModel = null;
                    }
                    DeleteTypeCommand.RaiseCanExecuteChanged();
                    ExecAnalyze.RaiseCanExecuteChanged();
                    VisibilityCtrl = Visibility.Visible;
                    BgCtrolInfo = (Brush)Application.Current.Resources["DefaultBannerIdleBackground"];
                    StrBanner = "Not Analized";

                    OnPropertyChanged(nameof(SelectedItem));
                    OnPropertyChanged(nameof(CimModel));
                }
            }
        }

        private DataMeasure _selectDM;
        public DataMeasure SelectDM
        {
            get => _selectDM;
            set
            {
                _selectDM = value;
                SelectedItem = FileMeasures.SingleOrDefault(f => f.IdFileMeasure == _selectDM.IdFileMeasure);
                VisibilityCtrl = Visibility.Visible;
                BgCtrolInfo = (Brush)Application.Current.Resources["DefaultBannerIdleBackground"];
                if (SelectedItem != null)
                { 
                    StrCutOffMax = SelectedItem.CutOffMax.ToString();
                    StrCutOffMin = SelectedItem.CutOffMin.ToString();
                }
                StrBanner = "Not Analized";
                OnPropertyChanged(nameof(SelectDM));
            }
        }

        private DataMeasure _dm1;
        public DataMeasure DM1
        {
            get => _dm1;
            set
            {
                _dm1 = value;
                OnPropertyChanged(nameof(DM1));
            }
        }

        private DataMeasure _dm2;
        public DataMeasure DM2
        {
            get => _dm2;
            set
            {
                _dm2 = value;
                OnPropertyChanged(nameof(DM2));
            }
        }

        private Visibility visibilityCtrl;
        public Visibility VisibilityCtrl
        {
            get
            {
                return visibilityCtrl;
            }
            set
            {
                visibilityCtrl = value;

                OnPropertyChanged(nameof(VisibilityCtrl));
            }
        }

        private Brush bgCtrolInfo;
        public Brush BgCtrolInfo
        {
            get
            {
                return bgCtrolInfo;
            }

            set
            {
                bgCtrolInfo = value;
                OnPropertyChanged(nameof(BgCtrolInfo));
            }
        }

        private string strBanner;
        public string StrBanner
        {
            get => strBanner;
            set
            {
                strBanner = value;
                OnPropertyChanged(nameof(StrBanner));
            }
        }

        private string strConsole;
        public string StrConsole 
        {
            get => strConsole;
            set
            {
                strConsole = value;
                OnPropertyChanged(nameof(StrConsole));
            }
        }

        private string strCutOffMax;
        public string StrCutOffMax
        {
            get => strCutOffMax;
            set
            {
                strCutOffMax = value;
                if (SelectedItem != null && strCutOffMax != null && double.TryParse(StrCutOffMax, out _) &&
                    double.Parse(StrCutOffMax) != SelectedItem.CutOffMax)
                {
                    SelectedItem.CutOffMax = double.Parse(StrCutOffMax);
                }
                ReloadItem();
                OnPropertyChanged(nameof(StrCutOffMax));
            }
        }

        private string strCutOffMin;
        public string StrCutOffMin
        {
            get => strCutOffMin;
            set
            {
                strCutOffMin = value;
                if (SelectedItem != null && strCutOffMin != null && double.TryParse(StrCutOffMin, out _) &&
                    double.Parse(StrCutOffMin) != SelectedItem.CutOffMin)
                {
                    SelectedItem.CutOffMin = double.Parse(StrCutOffMin);
                }
                ReloadItem();
                OnPropertyChanged(nameof(StrCutOffMin));
            }
        }

        private string strFreqCutMin;
        public string StrFreqCutMin
        {
            get => strFreqCutMin;
            set
            {
                strFreqCutMin = value;
                if (SelectedItem != null && strFreqCutMin != null && double.TryParse(StrFreqCutMin, out _) &&
                    double.Parse(StrFreqCutMin) != SelectedItem.FreqCutMin)
                {
                    SelectedItem.FreqCutMin = double.Parse(StrFreqCutMin);
                }
                ReloadItem();
                OnPropertyChanged(nameof(StrFreqCutMin));
            }
        }

        private string strFreqCutMax;
        public string StrFreqCutMax
        {
            get => strFreqCutMax;
            set
            {
                strFreqCutMax = value;
                if (SelectedItem != null && strFreqCutMax != null && double.TryParse(StrCutOffMax, out _) && 
                    double.Parse(StrFreqCutMax) != SelectedItem.FreqCutMax)
                {
                    SelectedItem.FreqCutMax  = double.Parse(StrFreqCutMax);
                }
                ReloadItem();
                OnPropertyChanged(nameof(StrFreqCutMax));
            }
        }

        private bool _showAllLog;
        public bool ShowAllLog
        {
            get => _showAllLog;
            set
            {
                _showAllLog = value;
                OnPropertyChanged(nameof(ShowAllLog));
            }
        }

        private bool _useFilter;
        public bool UseFilter
        {
            get => _useFilter;
            set
            {
                _useFilter = value;
                OnPropertyChanged(nameof(UseFilter));
            }
        }

        public static bool CanExecute()
        {
            return true;
        }

        public bool CanExecuteAnalizeDelete()
        {
            return SelectedItem != null;
        }

        private void OnAdd()
        {

            OpenFileDialog openFileDialog = new()
            {
                Filter = "Measure Files (*.pssession)|*.pssession|All files (*.*)|*.*"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                string psessionStr = File.ReadAllText(openFileDialog.FileName, System.Text.Encoding.UTF8).Trim(new char[] { '\uFEFF', '\u200B' });
                string name = openFileDialog.SafeFileName;
                int lp = name.LastIndexOf('.');
                if (lp > 0)
                {
                    name = name[..lp];
                }

                JObject psessionObj = JObject.Parse(psessionStr);
                JToken _data_mesures = psessionObj["measurements"];

                try
                {
                    var fm = new FileMeasure
                    {
                        ContentValue = psessionStr,
                        Name = name,
                        CutOffMax = Constants.INIT_VALUE_CUTOFF,
                        CutOffMin = Constants.INIT_VALUE_CUTOFF,
                        FreqCutMin = Constants.MINFREQBOX,
                        FreqCutMax = Constants.MAXFREQBOX,
                        LoadedAt = DateTime.Now
                    };
                    _dbcontext.FileMeasures.Add(fm);
                    _dbcontext.SaveChanges();

                    foreach (JToken _data in _data_mesures)
                    {
                        var dm = new DataMeasure
                        {
                            IdFileMeasure = fm.IdFileMeasure,
                            Name = _data["title"].ToString(),
                            Values = _data["dataset"].ToString(),
                            PointsCount = _data["dataset"]["values"][5]["datavalues"].Count(),
                            Type = 5,
                            FileMeasure = fm
                        };

                        _dbcontext.DataMeasures.Add(dm);
                        _dbcontext.SaveChanges();

                        DataMeasures.Add(dm);
                    }
                    
                    SelectedItem = FileMeasures.Last();
                    StrConsole += string.Format("{0}Measeure {1} successful loaded\n", TimeStamp(), SelectedItem.Name);
                }
                catch (Exception e)
                {
                    StrConsole += string.Format("{0}{1}\n", TimeStamp(2), e.ToString());
                }

            }
        }

        private void OnRemove()
        {
            if (SelectedItem != null)
            {
                if (MessageBox.Show($"Do you want to delete measure file \"{SelectedItem.Name}\"?",
                     "Delete file", MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.No) == MessageBoxResult.Yes)
                {
                    foreach (DataMeasure d in SelectedItem.DataMeasures)
                    {
                        _ = DataMeasures.Remove(d);
                    }
                    _ = FileMeasures.Remove(SelectedItem);
                    _dbcontext.SaveChanges();
                    if(SelectedItem != null)
                    {
                        StrConsole += string.Format("{0}Measeure {1} successful removed\n", TimeStamp(), SelectedItem.Name);
                    }
                }
            }
        }

        private void OnAnalyze()
        {
            try
            {
                if (SelectedItem != null)
                {
                    StrConsole += ShowLog("##############################################");
                    StrConsole += ShowLog($"Analizing ***{SelectedItem.Name}***");

                    PSessionFiles pSessionFiles = new(SelectedItem.DataMeasures.ElementAt(0).Values, SelectedItem.DataMeasures.ElementAt(1).Values);

                    ResultList rl = pSessionFiles.GetResultList(double.Parse(StrFreqCutMin), double.Parse(StrFreqCutMax), UseFilter);

                    double? DCutOffCimMax = Math.Abs((rl.DBFreqMax - rl.DMFreqMax) / rl.DBFreqMax);
                    double? DCutOffCimMin = Math.Abs((rl.DBFreqMin - rl.DMFreqMin) / rl.DBFreqMin);
                    double? DCutOffPhaMax = Math.Abs((rl.DBPhaFreq - rl.DMPhaFreq) / rl.DBPhaFreq);

                    if (DCutOffCimMax > 2d) DCutOffCimMax = null;
                    if (DCutOffCimMin > 2d) DCutOffCimMin = null;
                    if (DCutOffPhaMax > 2d) DCutOffPhaMax = null;

                    double? DMaxValue = new[] { DCutOffCimMax, DCutOffCimMin, DCutOffPhaMax }.Max();

                    double dFreqBase = 0;
                    int iFreqBasePos = 0;
                    double dFreqMeas = 0;
                    int iFreqMeasPos = 0;

                    if (DMaxValue == DCutOffCimMax)
                    {
                        dFreqBase = rl.DBFreqMax;
                        iFreqBasePos = rl.IBFreqMax;
                        dFreqMeas = rl.DMFreqMax;
                        iFreqMeasPos = rl.IMFreqMax;
                    }
                    else if (DMaxValue == DCutOffCimMin)
                    {
                        dFreqBase = rl.DBFreqMin;
                        iFreqBasePos = rl.IBFreqMin;
                        dFreqMeas = rl.DMFreqMin;
                        iFreqMeasPos = rl.IMFreqMin;
                    }
                    else if (DMaxValue == DCutOffPhaMax)
                    {
                        dFreqBase = rl.DBPhaFreq;
                        iFreqBasePos = rl.IBPhaFreq;
                        dFreqMeas = rl.DMPhaFreq;
                        iFreqMeasPos = rl.IMPhaFreq;
                    }

                    SelectedItem.DataMeasures.ElementAt(0).FrequencyValue = dFreqBase;
                    SelectedItem.DataMeasures.ElementAt(0).FrequencyPos = iFreqBasePos;
                    SelectedItem.DataMeasures.ElementAt(1).FrequencyValue = dFreqMeas;
                    SelectedItem.DataMeasures.ElementAt(1).FrequencyPos = iFreqMeasPos;
                    _dbcontext.SaveChanges();

                    int si = SelectedItem.IdFileMeasure;
                    SelectedItem = null;
                    SelectedItem = FileMeasures.SingleOrDefault(f => f.IdFileMeasure == si);

                    StrConsole += ShowLog($"Using CutOffMax -> {SelectedItem.CutOffMax}");
                    StrConsole += ShowLog($"Using CutOffMin -> {SelectedItem.CutOffMin}");

                    if (ShowAllLog)
                    {
                        StrConsole += ShowLog($"Base Cim Max -> {rl.DBCimMax * 1e6:0.###} | Freq -> {rl.DBFreqMax} | Index -> {rl.IBCimMax:0.###}", 3);
                        StrConsole += ShowLog($"Meas Cim Max -> {rl.DMCimMax * 1e6:0.###} | Freq -> {rl.DMFreqMax} | Index -> {rl.IMCimMax:0.###}", 3);

                        StrConsole += ShowLog($"Base Cim Min -> {rl.DBCimMin * 1e6:0.###} | Freq -> {rl.DBFreqMin} | Index -> {rl.IBCimMin:0.###}", 3);
                        StrConsole += ShowLog($"Meas Cim Min -> {rl.DMCimMin * 1e6:0.###} | Freq -> {rl.DMFreqMin} | Index -> {rl.IMCimMin:0.###}", 3);

                        StrConsole += ShowLog($"Base Phas Min -> {rl.DBPhaMax:0.###} | Freq -> {rl.DBPhaFreq} | Index -> {rl.IBPhaMax:0.###}", 3);
                        StrConsole += ShowLog($"Meas Phas Min -> {rl.DMPhaMax:0.###} | Freq -> {rl.DMPhaFreq} | Index -> {rl.IMPhaMax:0.###}", 3);
                    }

                    StrConsole += ShowLog($"CutOffMax Cim -> {(DCutOffCimMax == null ? "Out limits! ****" : DCutOffCimMax):0.### %}");
                    StrConsole += ShowLog($"CutOffMin Cim -> {(DCutOffCimMin == null ? "Out limits! ****" : DCutOffCimMin):0.### %}");
                    StrConsole += ShowLog($"CutOffMin Pha -> {(DCutOffPhaMax == null ? "Out limits! ****" : DCutOffPhaMax):0.### %}");



                    if ((DCutOffCimMax != null && DCutOffCimMax >= SelectedItem.CutOffMax / 100) ||
                        (DCutOffCimMin != null && DCutOffCimMin >= SelectedItem.CutOffMin / 100))
                    {
                        BgCtrolInfo = (Brush)Application.Current.Resources["DefaultBannerErrorBackground"];
                        StrBanner = "Virus Found!";
                        StrConsole += ShowLog("Virus Found!");
                    }
                    else if (DCutOffCimMax == null && DCutOffCimMin == null &&
                        DCutOffPhaMax != null && DCutOffPhaMax >= SelectedItem.CutOffMin / 100)
                    {
                        BgCtrolInfo = (Brush)Application.Current.Resources["DefaultBannerErrorBackground"];
                        StrBanner = "Virus Found!";
                        StrConsole += ShowLog("Virus Found!");
                    }
                    else if (DCutOffCimMax == null && DCutOffCimMin == null && DCutOffPhaMax == null)
                    {
                        BgCtrolInfo = (Brush)Application.Current.Resources["DefaultBannerExceptBackground"];
                        StrBanner = "Error!";
                        StrConsole += ShowLog("Error to get SValue from data, see logs!", 2);
                    }
                    else
                    {
                        BgCtrolInfo = (Brush)Application.Current.Resources["DefaultBannerOkBackground"];
                        StrBanner = "Virus not Found!";
                        StrConsole += ShowLog("Virus not Found!");
                        if (DCutOffPhaMax != null && DCutOffPhaMax >= SelectedItem.CutOffMin / 100)
                        {
                            StrConsole += ShowLog($"SValue from phase is greater than CutOff {SelectedItem.CutOffMin} % -> {DCutOffPhaMax:0.### %}", 1);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                StrConsole += string.Format($"{TimeStamp(2)} {ex.Message} \n");
                StrConsole += ShowLog(ex.Message, 2);
            }

        }

        private void OnSaveTextConsole()
        {
            if (!string.IsNullOrEmpty(StrConsole))
            {
                SaveFileDialog sf = new()
                {
                    Filter = "Text files (*.txt)|*.txt| log files (*.log)|*.log"
                };
                if (sf.ShowDialog() == true)
                {
                    File.WriteAllText(sf.FileName, strConsole);
                }
            }
        }

        private void OnCopyTextConsole()
        {
            if (!string.IsNullOrEmpty(StrConsole))
            {
                Clipboard.SetText(strConsole);
            }
        }

        private void OnCutTextConsole()
        {
            if (!string.IsNullOrEmpty(StrConsole))
            {
                Clipboard.SetText(strConsole);
                StrConsole = null;
            }
        }

        private void OnClearTextConsole()
        {
            if (!string.IsNullOrEmpty(StrConsole))
            {
                StrConsole = null;
            }

            BgCtrolInfo = (Brush)Application.Current.Resources["DefaultBannerIdleBackground"];
            StrBanner = "Not Analized";
        }

        private void OnCutOffMaxDownValue()
        {
            if(SelectedItem != null && SelectedItem.CutOffMax > Constants.MINLIMITCUTOFF + 1)
            {                
                StrCutOffMax = (SelectedItem.CutOffMax - 1).ToString();
            }
        }

        private void OnCutOffMaxUpValue()
        {
            if (SelectedItem != null && SelectedItem.CutOffMax < Constants.MAXLIMITCUTOFF - 1)
            {                
                StrCutOffMax = (SelectedItem.CutOffMax + 1).ToString();
            }
        }

        private void OnCutOffMinDownValue()
        {
            if (SelectedItem != null && SelectedItem.CutOffMin > Constants.MINLIMITCUTOFF + 1)
            {
                StrCutOffMin = (SelectedItem.CutOffMin - 1).ToString();
            }
        }

        private void OnCutOffMinUpValue()
        {
            if (SelectedItem != null && SelectedItem.CutOffMin < Constants.MAXLIMITCUTOFF - 1)
            {
                StrCutOffMin = (SelectedItem.CutOffMin + 1).ToString();
            }
        }

        private void OnFreqCutMinUpValue()
        {
            if (SelectedItem != null && SelectedItem.FreqCutMin < 1E6)
            {
                StrFreqCutMin = (SelectedItem.FreqCutMin + 1).ToString();
            }
        }

        private void OnFreqCutMinDownValue()
        {
            if (SelectedItem != null && SelectedItem.FreqCutMin > 0)
            {
                StrFreqCutMin = (SelectedItem.FreqCutMin - 1).ToString();
            }
        }

        private void OnFreqCutMaxDownValue()
        {
            if (SelectedItem != null && SelectedItem.FreqCutMax > 0)
            {
                StrFreqCutMax = (SelectedItem.FreqCutMax - 1).ToString();
            }
        }

        private void OnFreqCutMaxUpValue()
        {
            if (SelectedItem != null && SelectedItem.FreqCutMax < 1E6)
            {    
                StrFreqCutMax = (SelectedItem.FreqCutMax + 1).ToString();
            }
        }

        private void OnShowPlotWindow()
        {
            if(_pw == null || !_pw.IsActive)
            {
                _pw = new();
                _pw.DataContext = this;
                _pw.Show();
            }
            else
            {
                StrConsole += ShowLog("Graph plot already open", 1);
                _pw.Focus();
            }
        }

        private static string TimeStamp(int level = 0)
        {
            // level
            // 0 info
            // 1 warning
            // 2 error
            string strLevel = "INFO";
            
            if (level == 1)
            {
                strLevel = "WARNING";
            }
            else if (level == 2)
            {
                strLevel = "ERROR";
            }
            else if (level == 3)
            {
                strLevel = "DEBUG";
            }

            var t_index = DateTime.Now;
            return string.Format("[{0}] [{1}] ", t_index.ToString("dd-MM-yyyy HH:mm:ss.ff"), strLevel);
        }

        private static string ShowLog(string message, int level = 0)
        {
            return string.Format($"{TimeStamp(level)}{message}\n");
        }

        public void ReloadItem()
        {
            if (_dbcontext.ChangeTracker.HasChanges())
            {
                _dbcontext.SaveChanges();
                OnPropertyChanged(nameof(SelectedItem));
            }
        }

        private void LoadDataPlot(string m1, string m2)
        {
            PSessionFiles pSessionFiles = new(m1, m2);

            CimModel = new PlotModel { Title = SelectedItem.Name };

            CimModel.Axes.Add(new LogarithmicAxis { Position = AxisPosition.Bottom, Title = "Frequency (Hz)", Key = "Freq"});
            CimModel.Axes.Add(new LinearAxis { Position = AxisPosition.Left, Title = "C''(µF)" });
            CimModel.Axes.Add(new LinearAxis { Position = AxisPosition.Right, Title = "Phase(°)" , Key="Phase"});

            CimModel.Legends.Add(new Legend()
            {
                LegendPosition = LegendPosition.RightTop,
            });

            CimModel.DefaultColors = new List<OxyColor>
            {
                OxyColor.FromRgb(65, 130, 230),
                OxyColor.FromRgb(250, 40, 40)
            };

            var seriesCimBase = new ScatterSeries { Title = SelectedItem.DataMeasures.ElementAt(0).Name, MarkerType = MarkerType.Circle, MarkerStroke = OxyColors.Black};
            var seriesCimMeas = new ScatterSeries { Title = SelectedItem.DataMeasures.ElementAt(1).Name, MarkerType = MarkerType.Circle, MarkerStroke = OxyColors.Black};

            var seriesCimBaseFilter = new LineSeries { Title = SelectedItem.DataMeasures.ElementAt(0).Name + " Filter", LineStyle = LineStyle.Solid };
            var seriesCimMeasFilter = new LineSeries { Title = SelectedItem.DataMeasures.ElementAt(1).Name + " Filter", LineStyle = LineStyle.Solid };

            var seriesPhaBase = new ScatterSeries { Title = SelectedItem.DataMeasures.ElementAt(0).Name + " Phase", MarkerType = MarkerType.Diamond, MarkerStroke = OxyColors.Gray, MarkerSize = 5, XAxisKey = "Freq", YAxisKey = "Phase"};
            var seriesPhaMeas = new ScatterSeries { Title = SelectedItem.DataMeasures.ElementAt(1).Name + " Phase", MarkerType = MarkerType.Diamond, MarkerStroke = OxyColors.Gray, MarkerSize = 5, XAxisKey = "Freq", YAxisKey = "Phase" };

            var seriesPhaBaseFilter = new LineSeries { Title = SelectedItem.DataMeasures.ElementAt(0).Name + " Phase Filter", LineStyle = LineStyle.Dash, XAxisKey = "Freq", YAxisKey = "Phase" };
            var seriesPhaMeasFilter = new LineSeries { Title = SelectedItem.DataMeasures.ElementAt(1).Name + " Phase Filter", LineStyle = LineStyle.Dash, XAxisKey = "Freq", YAxisKey = "Phase" };

            var baseCimArrayFilter = new SavitzkyGolayFilter(Constants.SIDEPOINTS, Constants.POLYNOMIALORDER).Process(pSessionFiles.BaseValues.Cim.ToArray());
            var measCimArrayFilter = new SavitzkyGolayFilter(Constants.SIDEPOINTS, Constants.POLYNOMIALORDER).Process(pSessionFiles.MeasValues.Cim.ToArray());

            var basePhaArrayFilter = new SavitzkyGolayFilter(Constants.SIDEPOINTS, Constants.POLYNOMIALORDER).Process(pSessionFiles.BaseValues.Phase.ToArray());
            var measPhaArrayFilter = new SavitzkyGolayFilter(Constants.SIDEPOINTS, Constants.POLYNOMIALORDER).Process(pSessionFiles.MeasValues.Phase.ToArray());

            for (int i = 0; i < pSessionFiles.BaseValues.Freq.Count; i++)
            {
                seriesCimBase.Points.Add(new ScatterPoint(pSessionFiles.BaseValues.Freq[i], pSessionFiles.BaseValues.Cim[i] * 1e6, double.NaN, double.NaN, i));
                seriesCimBaseFilter.Points.Add(new DataPoint(pSessionFiles.BaseValues.Freq[i], baseCimArrayFilter[i] * 1e6));

                seriesPhaBase.Points.Add(new ScatterPoint(pSessionFiles.BaseValues.Freq[i], pSessionFiles.BaseValues.Phase[i], double.NaN, double.NaN, i));
                seriesPhaBaseFilter.Points.Add(new DataPoint(pSessionFiles.BaseValues.Freq[i], basePhaArrayFilter[i]));
            }

            for (int i = 0; i < pSessionFiles.MeasValues.Freq.Count; i++)
            {
                seriesCimMeas.Points.Add(new ScatterPoint(pSessionFiles.MeasValues.Freq[i], pSessionFiles.MeasValues.Cim[i] * 1e6, double.NaN, double.NaN, i));
                seriesCimMeasFilter.Points.Add(new DataPoint(pSessionFiles.MeasValues.Freq[i], measCimArrayFilter[i] * 1e6));

                seriesPhaMeas.Points.Add(new ScatterPoint(pSessionFiles.MeasValues.Freq[i], pSessionFiles.MeasValues.Phase[i], double.NaN, double.NaN, i));
                seriesPhaMeasFilter.Points.Add(new DataPoint(pSessionFiles.MeasValues.Freq[i], measPhaArrayFilter[i]));
            }

            CimModel.Series.Add(seriesCimBase);
            CimModel.Series.Add(seriesCimMeas);
            CimModel.Series.Add(seriesCimBaseFilter);
            CimModel.Series.Add(seriesCimMeasFilter);
            CimModel.Series.Add(seriesPhaBase);
            CimModel.Series.Add(seriesPhaMeas);
            CimModel.Series.Add(seriesPhaBaseFilter);
            CimModel.Series.Add(seriesPhaMeasFilter);

            seriesCimBase.TrackerFormatString = "{0}\n{1}: {2:0.##}\n{3}: {4:0.####}\nIndex={Tag}";
            seriesCimMeas.TrackerFormatString = "{0}\n{1}: {2:0.##}\n{3}: {4:0.####}\nIndex={Tag}";
            seriesPhaBase.TrackerFormatString = "{0}\n{1}: {2:0.##}\n{3}: {4:0.####}\nIndex={Tag}";
            seriesPhaMeas.TrackerFormatString = "{0}\n{1}: {2:0.##}\n{3}: {4:0.####}\nIndex={Tag}";
        }

        public MeasureViewModel()
        {
            try
            {
                _dbcontext = new();
                _ = _dbcontext.Database.EnsureCreated();

                _dbcontext.FileMeasures.Load();
                FileMeasures = _dbcontext.FileMeasures.Local.ToObservableCollection();

                _dbcontext.DataMeasures.Load();
                DataMeasures = _dbcontext.DataMeasures.Local.ToObservableCollection();

                SaveTextConsole = new RelayCommand(OnSaveTextConsole, CanExecute);
                CopyTextConsole = new RelayCommand(OnCopyTextConsole, CanExecute);
                CutTextConsole = new RelayCommand(OnCutTextConsole, CanExecute);
                ClearTextConsole = new RelayCommand(OnClearTextConsole, CanExecute);
                CutOffMaxDownValue = new RelayCommand(OnCutOffMaxDownValue, CanExecute);
                CutOffMaxUpValue = new RelayCommand(OnCutOffMaxUpValue, CanExecute);
                CutOffMinDownValue = new RelayCommand(OnCutOffMinDownValue, CanExecute);
                CutOffMinUpValue = new RelayCommand(OnCutOffMinUpValue, CanExecute);
                FreqCutMinUpValue = new RelayCommand(OnFreqCutMinUpValue, CanExecute);
                FreqCutMinDownValue = new RelayCommand(OnFreqCutMinDownValue, CanExecute);
                FreqCutMaxUpValue = new RelayCommand(OnFreqCutMaxUpValue, CanExecute);
                FreqCutMaxDownValue = new RelayCommand(OnFreqCutMaxDownValue, CanExecute);
                ShowPlotWindow = new RelayCommand(OnShowPlotWindow, CanExecute);

                visibilityCtrl = Visibility.Hidden;
                BgCtrolInfo = (Brush)Application.Current.Resources["DefaultBannerIdleBackground"];
                StrBanner = "Not Analized";
            }
            catch (SqliteException ex)
            {
                if(MessageBox.Show($"Database error: {ex.Message}\n Do you want recreate database?", "Database error", MessageBoxButton.YesNo, MessageBoxImage.Error) == MessageBoxResult.Yes)
                {
                    string dbName = "NanobionicsDB.db";
                    string sourcePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), dbName);
                    try
                    {
                        File.Move(sourcePath, sourcePath + ".bk");       
                    }
                    catch { }   
                    MessageBox.Show("Successfully recreated database!\n Restart the app...", "Database update", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                Application.Current.Shutdown();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Generic app error", MessageBoxButton.OK, MessageBoxImage.Error);
                Application.Current.Shutdown();
            }
        }
    }
}
