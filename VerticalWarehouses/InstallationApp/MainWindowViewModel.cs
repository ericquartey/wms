using System;
using Prism.Mvvm;
using System.Windows.Input;
using Prism.Commands;
using Ferretto.VW.Navigation;
using Ferretto.VW.InstallationApp.ViewsAndViewModels;
using Ferretto.VW.InstallationApp.ViewsAndViewModels.LowSpeedMovements;
using Ferretto.VW.InstallationApp.ViewsAndViewModels.SensorsState;
using Ferretto.VW.InstallationApp.ViewsAndViewModels.SingleViews;
using Ferretto.VW.InstallationApp.ViewsAndViewModels.GatesControl;
using Ferretto.VW.InstallationApp.ViewsAndViewModels.GatesHeightControl;
using Ferretto.VW.InstallationApp.ServiceUtilities;
using Ferretto.VW.Utils.Source;
using System.Net;
using System.IO;
using System.Configuration;
using System.Windows.Controls.Primitives;
using System.Windows.Controls;

#if CODEMAID
    // disable codemaid in this file
#endif

namespace Ferretto.VW.InstallationApp
{
    public delegate void ClickedOnMachineOnMarchEvent();

    public delegate void ClickedOnMachineModeEvent();

    public delegate void SensorsStatesChangedEvent();

    public delegate void ChangeBoolDelegate(bool b);

    public class MainWindowViewModel : BindableBase
    {
        #region Constants, Statics & Others

        private static readonly string SENSOR_INITIALIZER_URL = ConfigurationManager.AppSettings["SensorsStatesInitializer"];
        private static readonly string SERVICE_PATH = ConfigurationManager.AppSettings["SensorsStatesHubPath"];
        private static readonly string URL = ConfigurationManager.AppSettings["ServiceURL"];
        public static SensorsStates States;
        private SensorsStatesHubClient client;
        private BindableBase contentRegionCurrentViewModel;
        private BindableBase navigationRegionCurrentViewModel;
        private bool machineModeSelectionBool = false;
        private bool machineOnMarchSelectionBool = false;
        private bool isNavigationButtonRegionExpanded = true;

        #endregion Constants, Statics & Others

        #region ViewModels & Commands Fields

        private readonly BeltBurnishingViewModel BeltBurnishingVMInstance = new BeltBurnishingViewModel();
        private readonly CellsControlViewModel CellsControlVMInstance = new CellsControlViewModel();
        private readonly CellsPanelsControlViewModel CellsPanelControlVMInsance = new CellsPanelsControlViewModel();
        private readonly Gate1ControlViewModel Gate1ControlVMInstance = new Gate1ControlViewModel();
        private readonly Gate1HeightControlViewModel Gate1HeightControlVMInstance = new Gate1HeightControlViewModel();
        private readonly Gate2ControlViewModel Gate2ControlVMInstance = new Gate2ControlViewModel();
        private readonly Gate2HeightControlViewModel Gate2HeightControlVMInstance = new Gate2HeightControlViewModel();
        private readonly Gate3ControlViewModel Gate3ControlVMInstance = new Gate3ControlViewModel();
        private readonly Gate3HeightControlViewModel Gate3HeightControlVMInstance = new Gate3HeightControlViewModel();
        private readonly InstallationStateViewModel InstallationStateVMInstance = new InstallationStateViewModel();
        private readonly LSMTGateEngineViewModel LSMTGateEngineVMInstance = new LSMTGateEngineViewModel();
        private readonly LSMTHorizontalEngineViewModel LSMTHorizontalEngineVMInstance = new LSMTHorizontalEngineViewModel();
        private readonly LSMTNavigationButtonsViewModel LSMTNavigationButtonsVMInstance = new LSMTNavigationButtonsViewModel();
        private readonly LSMTVerticalEngineViewModel LSMTVerticalEngineVMInstance = new LSMTVerticalEngineViewModel();
        private readonly MainWindowNavigationButtonsViewModel MainWindowNavigationButtonsVMInstance = new MainWindowNavigationButtonsViewModel();
        private readonly ResolutionCalibrationVerticalAxisViewModel ResolutionCalibrationVerticalAxisVMInstance = new ResolutionCalibrationVerticalAxisViewModel();
        private readonly SSBaysViewModel SSBaysVMInstance = new SSBaysViewModel();
        private readonly SSCradleViewModel SSCradleVMInstance = new SSCradleViewModel();
        private readonly SSGateViewModel SSGateVMInstance = new SSGateViewModel();
        private readonly SSNavigationButtonsViewModel SSNavigationButtonsVMInstance = new SSNavigationButtonsViewModel();
        private readonly SSVariousInputsViewModel SSVariousInputsVMInstance = new SSVariousInputsViewModel();
        private readonly SSVerticalAxisViewModel SSVerticalAxisVMInstance = new SSVerticalAxisViewModel();
        private readonly VerticalAxisCalibrationViewModel VerticalAxisCalibrationVMInstance = new VerticalAxisCalibrationViewModel();
        private readonly VerticalOffsetCalibrationViewModel VerticalOffsetCalibrationVMInstance = new VerticalOffsetCalibrationViewModel();
        private readonly WeightControlViewModel WeightControlVMInstance = new WeightControlViewModel();
        private readonly IdleViewModel IdleVMInstance = new IdleViewModel();

        private ICommand backToVWAPPCommand;
        private ICommand backToMainWindowNavigationButtonsViewCommand;
        private ICommand beltBurnishingButtonCommand;
        private ICommand cellsControlButtonCommand;
        private ICommand cellsPanelControlButtonCommand;
        private ICommand gate1HeightControlNavigationButtonCommand;
        private ICommand gate2HeightControlNavigationButtonCommand;
        private ICommand gate3HeightControlNavigationButtonCommand;
        private ICommand gateHeightControlButtonCommand;
        private ICommand gates1ControlNavigationButtonCommand;
        private ICommand gates2ControlNavigationButtonCommand;
        private ICommand gates3ControlNavigationButtonCommand;
        private ICommand gatesControlButtonCommand;
        private ICommand installationStateButtonCommand;
        private ICommand lowSpeedMovementsTestButtonCommand;
        private ICommand lsmtGateEngineButtonCommand;
        private ICommand lsmtHorizontalEngineButtonCommand;
        private ICommand lsmtVerticalEngineButtonCommand;
        private ICommand resolutionCalibrationVerticalAxisButtonCommand;
        private ICommand ssBaysButtonCommand;
        private ICommand ssCradleButtonCommand;
        private ICommand ssGateButtonCommand;
        private ICommand ssNavigationButtonsButtonCommand;
        private ICommand ssVariousInputsButtonCommand;
        private ICommand ssVerticalAxisButtonCommand;
        private ICommand verticalAxisCalibrationButtonCommand;
        private ICommand verticalOffsetCalibrationButtonCommand;
        private ICommand weightControlButtonCommand;
        private ICommand machineModeCustomCommand;
        private ICommand machineOnMarchCustomCommand;
        private ICommand errorButtonCommand;

        #endregion ViewModels & Commands Fields

        #region Constructors

        public MainWindowViewModel()
        {
            this.NavigationRegionCurrentViewModel = this.MainWindowNavigationButtonsVMInstance;
            this.ContentRegionCurrentViewModel = this.IdleVMInstance;
            this.ConnectMethod();
            NavigationService.GoToViewEventHandler += this.HideNavigationButtonRegion;
            NavigationService.ExitViewEventHandler += this.ShowNavigationButtonRegion;
            MainWindow.FinishedMachineModeChangeStateEventHandler += () => { this.MachineModeSelectionBool = !this.MachineModeSelectionBool; };
            MainWindow.FinishedMachineOnMarchChangeStateEventHandler += () => { this.MachineOnMarchSelectionBool = !this.MachineOnMarchSelectionBool; };
            ClickedOnMachineModeEventHandler += () => { };
            ClickedOnMachineOnMarchEventHandler += () => { };
            SensorsStatesChangedEventHandler += () => { };
        }

        #endregion Constructors

        #region Events

        public static event SensorsStatesChangedEvent SensorsStatesChangedEventHandler;

        public static event ClickedOnMachineOnMarchEvent ClickedOnMachineOnMarchEventHandler;

        public static event ClickedOnMachineModeEvent ClickedOnMachineModeEventHandler;

        #endregion Events

        #region Commands Properties

        public ICommand BackToMainWindowNavigationButtonsViewButtonCommand => this.backToMainWindowNavigationButtonsViewCommand ?? (this.backToMainWindowNavigationButtonsViewCommand = new DelegateCommand(() => { this.NavigationRegionCurrentViewModel = this.MainWindowNavigationButtonsVMInstance; this.ContentRegionCurrentViewModel = this.IdleVMInstance; NavigationService.RaiseExitViewEvent(); }));

        public ICommand BeltBurnishingButtonCommand => this.beltBurnishingButtonCommand ?? (this.beltBurnishingButtonCommand = new DelegateCommand(() => { this.ContentRegionCurrentViewModel = this.BeltBurnishingVMInstance; this.MainWindowNavigationButtonsVMInstance.SetAllNavigationButtonDisabled(); }));

        public ICommand CellsControlButtonCommand => this.cellsControlButtonCommand ?? (this.cellsControlButtonCommand = new DelegateCommand(() => this.ContentRegionCurrentViewModel = this.CellsControlVMInstance));

        public ICommand CellsPanelControlButtonCommand => this.cellsPanelControlButtonCommand ?? (this.cellsPanelControlButtonCommand = new DelegateCommand(() => { this.ContentRegionCurrentViewModel = this.CellsPanelControlVMInsance; this.MainWindowNavigationButtonsVMInstance.SetAllNavigationButtonDisabled(); }));

        public ICommand Gate1HeightControlNavigationButtonCommand => this.gate1HeightControlNavigationButtonCommand ?? (this.gate1HeightControlNavigationButtonCommand = new DelegateCommand(() => this.ContentRegionCurrentViewModel = this.Gate1HeightControlVMInstance));

        public ICommand Gate2HeightControlNavigationButtonCommand => this.gate2HeightControlNavigationButtonCommand ?? (this.gate2HeightControlNavigationButtonCommand = new DelegateCommand(() => this.ContentRegionCurrentViewModel = this.Gate2HeightControlVMInstance));

        public ICommand Gate3HeightControlNavigationButtonCommand => this.gate3HeightControlNavigationButtonCommand ?? (this.gate3HeightControlNavigationButtonCommand = new DelegateCommand(() => this.ContentRegionCurrentViewModel = this.Gate3HeightControlVMInstance));

        public ICommand GateHeightControlButtonCommand => this.gateHeightControlButtonCommand ?? (this.gateHeightControlButtonCommand = new DelegateCommand(() => this.ContentRegionCurrentViewModel = this.Gate2HeightControlVMInstance));

        public ICommand Gates1ControlNavigationButtonCommand => this.gates1ControlNavigationButtonCommand ?? (this.gates1ControlNavigationButtonCommand = new DelegateCommand(() => this.ContentRegionCurrentViewModel = this.Gate1ControlVMInstance));

        public ICommand Gates2ControlNavigationButtonCommand => this.gates2ControlNavigationButtonCommand ?? (this.gates2ControlNavigationButtonCommand = new DelegateCommand(() => this.ContentRegionCurrentViewModel = this.Gate2ControlVMInstance));

        public ICommand Gates3ControlNavigationButtonCommand => this.gates3ControlNavigationButtonCommand ?? (this.gates3ControlNavigationButtonCommand = new DelegateCommand(() => this.ContentRegionCurrentViewModel = this.Gate3ControlVMInstance));

        public ICommand GatesControlButtonCommand => this.gatesControlButtonCommand ?? (this.gatesControlButtonCommand = new DelegateCommand(() => this.ContentRegionCurrentViewModel = this.Gate2ControlVMInstance));

        public ICommand InstallationStateButtonCommand => this.installationStateButtonCommand ?? (this.installationStateButtonCommand = new DelegateCommand(() => this.ContentRegionCurrentViewModel = this.InstallationStateVMInstance));

        public ICommand LowSpeedMovementsTestButtonCommand => this.lowSpeedMovementsTestButtonCommand ?? (this.lowSpeedMovementsTestButtonCommand = new DelegateCommand(() => { this.NavigationRegionCurrentViewModel = this.LSMTNavigationButtonsVMInstance; this.ContentRegionCurrentViewModel = null; }));

        public ICommand LSMTGateEngineButtonCommand => this.lsmtGateEngineButtonCommand ?? (this.lsmtGateEngineButtonCommand = new DelegateCommand(() => { this.ContentRegionCurrentViewModel = this.LSMTGateEngineVMInstance; }));

        public ICommand LSMTHorizontalEngineButtonCommand => this.lsmtHorizontalEngineButtonCommand ?? (this.lsmtHorizontalEngineButtonCommand = new DelegateCommand(() => { this.ContentRegionCurrentViewModel = this.LSMTHorizontalEngineVMInstance; }));

        public ICommand LSMTVerticalEngineButtonCommand => this.lsmtVerticalEngineButtonCommand ?? (this.lsmtVerticalEngineButtonCommand = new DelegateCommand(() => { this.ContentRegionCurrentViewModel = this.LSMTVerticalEngineVMInstance; }));

        public ICommand ResolutionCalibrationVerticalAxisButtonCommand => this.resolutionCalibrationVerticalAxisButtonCommand ?? (this.resolutionCalibrationVerticalAxisButtonCommand = new DelegateCommand(() => { NavigationService.RaiseGoToViewEvent(); this.ContentRegionCurrentViewModel = this.ResolutionCalibrationVerticalAxisVMInstance; }));

        public ICommand SsBaysButtonCommand => this.ssBaysButtonCommand ?? (this.ssBaysButtonCommand = new DelegateCommand(() => { this.ContentRegionCurrentViewModel = this.SSBaysVMInstance; }));

        public ICommand SsCradleButtonCommand => this.ssCradleButtonCommand ?? (this.ssCradleButtonCommand = new DelegateCommand(() => { this.ContentRegionCurrentViewModel = this.SSCradleVMInstance; }));

        public ICommand SsGateButtonCommand => this.ssGateButtonCommand ?? (this.ssGateButtonCommand = new DelegateCommand(() => { this.ContentRegionCurrentViewModel = this.SSGateVMInstance; }));

        public ICommand SSNavigationButtonsButtonCommand => this.ssNavigationButtonsButtonCommand ?? (this.ssNavigationButtonsButtonCommand = new DelegateCommand(() => { this.NavigationRegionCurrentViewModel = this.SSNavigationButtonsVMInstance; this.ContentRegionCurrentViewModel = null; }));

        public ICommand SsVariousInputsButtonCommand => this.ssVariousInputsButtonCommand ?? (this.ssVariousInputsButtonCommand = new DelegateCommand(() => { this.ContentRegionCurrentViewModel = this.SSVariousInputsVMInstance; }));

        public ICommand SsVerticalAxisButtonCommand => this.ssVerticalAxisButtonCommand ?? (this.ssVerticalAxisButtonCommand = new DelegateCommand(() => { this.ContentRegionCurrentViewModel = this.SSVerticalAxisVMInstance; }));

        public ICommand VerticalAxisCalibrationButtonCommand => this.verticalAxisCalibrationButtonCommand ?? (this.verticalAxisCalibrationButtonCommand = new DelegateCommand(() => { this.ContentRegionCurrentViewModel = this.VerticalAxisCalibrationVMInstance; }));

        public ICommand VerticalOffsetCalibrationButtonCommand => this.verticalOffsetCalibrationButtonCommand ?? (this.verticalOffsetCalibrationButtonCommand = new DelegateCommand(() => { this.ContentRegionCurrentViewModel = this.VerticalOffsetCalibrationVMInstance; }));

        public ICommand WeightControlButtonCommand => this.weightControlButtonCommand ?? (this.weightControlButtonCommand = new DelegateCommand(() => { this.ContentRegionCurrentViewModel = this.WeightControlVMInstance; }));

        public ICommand MachineModeCustomCommand => this.machineModeCustomCommand ?? (this.machineModeCustomCommand = new DelegateCommand(() => this.RaiseClickedOnMachineModeEvent()));

        public ICommand MachineOnMarchCustomCommand => this.machineOnMarchCustomCommand ?? (this.machineOnMarchCustomCommand = new DelegateCommand(() => this.RaiseClickedOnMachineOnMarchEvent()));

        public ICommand BackToVWAPPCommand => this.backToVWAPPCommand ?? (this.backToVWAPPCommand = new DelegateCommand(() => { NavigationService.RaiseBackToVWAppEvent(); MainWindowViewModel.ClickedOnMachineModeEventHandler = null; MainWindowViewModel.ClickedOnMachineOnMarchEventHandler = null; }));

        #endregion Commands Properties

        #region Other Properties

        public BindableBase ContentRegionCurrentViewModel { get => this.contentRegionCurrentViewModel; set => this.SetProperty(ref this.contentRegionCurrentViewModel, value); }

        public Boolean MachineModeSelectionBool { get => this.machineModeSelectionBool; set => this.SetProperty(ref this.machineModeSelectionBool, value); }

        public Boolean MachineOnMarchSelectionBool { get => this.machineOnMarchSelectionBool; set => this.SetProperty(ref this.machineOnMarchSelectionBool, value); }

        public BindableBase NavigationRegionCurrentViewModel { get => this.navigationRegionCurrentViewModel; set => this.SetProperty(ref this.navigationRegionCurrentViewModel, value); }

        public Boolean IsNavigationButtonRegionExpanded { get => this.isNavigationButtonRegionExpanded; set => this.SetProperty(ref this.isNavigationButtonRegionExpanded, value); }

        public ICommand ErrorButtonCommand => this.errorButtonCommand ?? (this.errorButtonCommand = new DelegateCommand(this.ErrorButtonCommandMethod));

        #endregion Other Properties

        #region Methods

        public string Get(string uri)
        {
            var request = (HttpWebRequest)WebRequest.Create(uri);
            request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

            using (var response = (HttpWebResponse)request.GetResponse())
            using (var stream = response.GetResponseStream())
            using (var reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }

        private void Client_SensorsStatesChanged(object sender, SensorsStatesEventArgs e)
        {
            States = e.SensorsStates;
            this.RaiseSensorsStatesChangedEvent();
        }

        private async void ConnectMethod()
        {
            try
            {
                this.client = new SensorsStatesHubClient(URL, SERVICE_PATH);
                await this.client.ConnectAsync();

                this.client.SensorsStatesChanged += this.Client_SensorsStatesChanged;
                this.Get(SENSOR_INITIALIZER_URL);
            }
            catch
            {
            }
        }

        private void HideNavigationButtonRegion()
        {
            this.IsNavigationButtonRegionExpanded = false;
        }

        private void ShowNavigationButtonRegion()
        {
            this.IsNavigationButtonRegionExpanded = true;
        }

        private void ErrorButtonCommandMethod()
        {
            var p = new Popup();
            var g = new Grid();
            p.Width = 300;
            p.Height = 300;
            p.Placement = PlacementMode.Absolute;
            var s = new StackPanel();
            s.Width = 200;
            s.Height = 290;
            s.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
            s.VerticalAlignment = System.Windows.VerticalAlignment.Center;
            var l = new Label();
            l.Width = 150;
            l.Height = 150;
            l.Content = "Errore";
            var b = new Button();
            b.Width = 150;
            b.Height = 60;
            b.Content = "Ok, chiudi popup";
            b.Command = new DelegateCommand(() => p.IsOpen = false);
            s.Children.Add(l);
            s.Children.Add(b);
            g.Children.Add(s);
            p.Child = g;
            p.IsOpen = true;
        }

        private void RaiseSensorsStatesChangedEvent() => SensorsStatesChangedEventHandler();

        private void RaiseClickedOnMachineModeEvent() => ClickedOnMachineModeEventHandler();

        private void RaiseClickedOnMachineOnMarchEvent() => ClickedOnMachineOnMarchEventHandler();

        #endregion Methods
    }
}
