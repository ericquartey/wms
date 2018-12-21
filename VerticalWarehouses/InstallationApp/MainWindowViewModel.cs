using System;
using Prism.Mvvm;
using System.Windows.Input;
using Prism.Commands;
using Ferretto.VW.Navigation;
using Ferretto.VW.InstallationApp.ServiceUtilities;
using Ferretto.VW.Utils.Source;
using System.Net;
using System.IO;
using System.Configuration;
using System.Diagnostics;

#if CODEMAID
    // disable codemaid region messer in this file
#endif

namespace Ferretto.VW.InstallationApp
{
    public delegate void ClickedOnMachineOnMarchEvent();

    public delegate void ClickedOnMachineModeEvent();

    public delegate void SensorsStatesChangedEvent();

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
        private BindableBase exitViewButtonRegionCurrentViewModel;
        private bool machineModeSelectionBool = false;
        private bool machineOnMarchSelectionBool = false;
        private bool isNavigationButtonRegionExpanded = true;
        private bool isExitViewButtonRegionExpanded = false;

        #endregion Constants, Statics & Others

        #region Commands Fields

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

        #endregion Commands Fields

        #region Constructors

        public MainWindowViewModel()
        {
            this.NavigationRegionCurrentViewModel = ViewModels.MainWindowNavigationButtonsVMInstance;
            this.ExitViewButtonRegionCurrentViewModel = ViewModels.MainWindowBackToIAPPButtonVMInstance;
            this.ContentRegionCurrentViewModel = ViewModels.IdleVMInstance;
            this.ConnectMethod();
            this.InitializeEvents();
        }

        #endregion Constructors

        #region Events

        public static event SensorsStatesChangedEvent SensorsStatesChangedEventHandler;

        public static event ClickedOnMachineOnMarchEvent ClickedOnMachineOnMarchEventHandler;

        public static event ClickedOnMachineModeEvent ClickedOnMachineModeEventHandler;

        #endregion Events

        #region Commands Properties

        public ICommand BackToMainWindowNavigationButtonsViewButtonCommand => this.backToMainWindowNavigationButtonsViewCommand ?? (this.backToMainWindowNavigationButtonsViewCommand = new DelegateCommand(() => { this.NavigationRegionCurrentViewModel = ViewModels.MainWindowNavigationButtonsVMInstance; this.ContentRegionCurrentViewModel = ViewModels.IdleVMInstance; NavigationService.RaiseExitViewEvent(); }));

        public ICommand BeltBurnishingButtonCommand => this.beltBurnishingButtonCommand ?? (this.beltBurnishingButtonCommand = new DelegateCommand(() => { NavigationService.RaiseGoToViewEvent(); this.ContentRegionCurrentViewModel = ViewModels.BeltBurnishingVMInstance; }));

        public ICommand CellsControlButtonCommand => this.cellsControlButtonCommand ?? (this.cellsControlButtonCommand = new DelegateCommand(() => { NavigationService.RaiseGoToViewEvent(); this.ContentRegionCurrentViewModel = ViewModels.CellsControlVMInstance; }));

        public ICommand CellsPanelControlButtonCommand => this.cellsPanelControlButtonCommand ?? (this.cellsPanelControlButtonCommand = new DelegateCommand(() => { NavigationService.RaiseGoToViewEvent(); this.ContentRegionCurrentViewModel = ViewModels.CellsPanelControlVMInsance; }));

        public ICommand Gate1HeightControlNavigationButtonCommand => this.gate1HeightControlNavigationButtonCommand ?? (this.gate1HeightControlNavigationButtonCommand = new DelegateCommand(() => { NavigationService.RaiseGoToViewEvent(); this.ContentRegionCurrentViewModel = ViewModels.Gate1HeightControlVMInstance; }));

        public ICommand Gate2HeightControlNavigationButtonCommand => this.gate2HeightControlNavigationButtonCommand ?? (this.gate2HeightControlNavigationButtonCommand = new DelegateCommand(() => { NavigationService.RaiseGoToViewEvent(); this.ContentRegionCurrentViewModel = ViewModels.Gate2HeightControlVMInstance; }));

        public ICommand Gate3HeightControlNavigationButtonCommand => this.gate3HeightControlNavigationButtonCommand ?? (this.gate3HeightControlNavigationButtonCommand = new DelegateCommand(() => { NavigationService.RaiseGoToViewEvent(); this.ContentRegionCurrentViewModel = ViewModels.Gate3HeightControlVMInstance; }));

        public ICommand GateHeightControlButtonCommand => this.gateHeightControlButtonCommand ?? (this.gateHeightControlButtonCommand = new DelegateCommand(() => { NavigationService.RaiseGoToViewEvent(); this.ContentRegionCurrentViewModel = ViewModels.Gate2HeightControlVMInstance; }));

        public ICommand Gates1ControlNavigationButtonCommand => this.gates1ControlNavigationButtonCommand ?? (this.gates1ControlNavigationButtonCommand = new DelegateCommand(() => { NavigationService.RaiseGoToViewEvent(); this.ContentRegionCurrentViewModel = ViewModels.Gate1ControlVMInstance; }));

        public ICommand Gates2ControlNavigationButtonCommand => this.gates2ControlNavigationButtonCommand ?? (this.gates2ControlNavigationButtonCommand = new DelegateCommand(() => { NavigationService.RaiseGoToViewEvent(); this.ContentRegionCurrentViewModel = ViewModels.Gate2ControlVMInstance; }));

        public ICommand Gates3ControlNavigationButtonCommand => this.gates3ControlNavigationButtonCommand ?? (this.gates3ControlNavigationButtonCommand = new DelegateCommand(() => { NavigationService.RaiseGoToViewEvent(); this.ContentRegionCurrentViewModel = ViewModels.Gate3ControlVMInstance; }));

        public ICommand GatesControlButtonCommand => this.gatesControlButtonCommand ?? (this.gatesControlButtonCommand = new DelegateCommand(() => { NavigationService.RaiseGoToViewEvent(); this.ContentRegionCurrentViewModel = ViewModels.Gate2ControlVMInstance; }));

        public ICommand InstallationStateButtonCommand => this.installationStateButtonCommand ?? (this.installationStateButtonCommand = new DelegateCommand(() => { NavigationService.RaiseGoToViewEvent(); this.ContentRegionCurrentViewModel = ViewModels.InstallationStateVMInstance; }));

        public ICommand LowSpeedMovementsTestButtonCommand => this.lowSpeedMovementsTestButtonCommand ?? (this.lowSpeedMovementsTestButtonCommand = new DelegateCommand(() => { NavigationService.RaiseGoToViewEvent(); this.ContentRegionCurrentViewModel = ViewModels.LSMTMainVMInstance; }));

        public ICommand LSMTGateEngineButtonCommand => this.lsmtGateEngineButtonCommand ?? (this.lsmtGateEngineButtonCommand = new DelegateCommand(() => { NavigationService.RaiseGoToViewEvent(); this.ContentRegionCurrentViewModel = ViewModels.LSMTGateEngineVMInstance; }));

        public ICommand LSMTHorizontalEngineButtonCommand => this.lsmtHorizontalEngineButtonCommand ?? (this.lsmtHorizontalEngineButtonCommand = new DelegateCommand(() => { NavigationService.RaiseGoToViewEvent(); this.ContentRegionCurrentViewModel = ViewModels.LSMTHorizontalEngineVMInstance; }));

        public ICommand LSMTVerticalEngineButtonCommand => this.lsmtVerticalEngineButtonCommand ?? (this.lsmtVerticalEngineButtonCommand = new DelegateCommand(() => { NavigationService.RaiseGoToViewEvent(); this.ContentRegionCurrentViewModel = ViewModels.LSMTVerticalEngineVMInstance; }));

        public ICommand ResolutionCalibrationVerticalAxisButtonCommand => this.resolutionCalibrationVerticalAxisButtonCommand ?? (this.resolutionCalibrationVerticalAxisButtonCommand = new DelegateCommand(() => { NavigationService.RaiseGoToViewEvent(); this.ContentRegionCurrentViewModel = ViewModels.ResolutionCalibrationVerticalAxisVMInstance; ViewModels.ResolutionCalibrationVerticalAxisVMInstance.SubscribeMethodToEvent(); }));

        public ICommand SsBaysButtonCommand => this.ssBaysButtonCommand ?? (this.ssBaysButtonCommand = new DelegateCommand(() => { NavigationService.RaiseGoToViewEvent(); this.ContentRegionCurrentViewModel = ViewModels.SSBaysVMInstance; }));

        public ICommand SsCradleButtonCommand => this.ssCradleButtonCommand ?? (this.ssCradleButtonCommand = new DelegateCommand(() => { NavigationService.RaiseGoToViewEvent(); this.ContentRegionCurrentViewModel = ViewModels.SSCradleVMInstance; }));

        public ICommand SsGateButtonCommand => this.ssGateButtonCommand ?? (this.ssGateButtonCommand = new DelegateCommand(() => { NavigationService.RaiseGoToViewEvent(); this.ContentRegionCurrentViewModel = ViewModels.SSGateVMInstance; }));

        public ICommand SSNavigationButtonsButtonCommand => this.ssNavigationButtonsButtonCommand ?? (this.ssNavigationButtonsButtonCommand = new DelegateCommand(() => { NavigationService.RaiseGoToViewEvent(); this.ContentRegionCurrentViewModel = ViewModels.SSMainVMInstance; }));

        public ICommand SsVariousInputsButtonCommand => this.ssVariousInputsButtonCommand ?? (this.ssVariousInputsButtonCommand = new DelegateCommand(() => { NavigationService.RaiseGoToViewEvent(); this.ContentRegionCurrentViewModel = ViewModels.SSVariousInputsVMInstance; }));

        public ICommand SsVerticalAxisButtonCommand => this.ssVerticalAxisButtonCommand ?? (this.ssVerticalAxisButtonCommand = new DelegateCommand(() => { NavigationService.RaiseGoToViewEvent(); this.ContentRegionCurrentViewModel = ViewModels.SSVerticalAxisVMInstance; }));

        public ICommand VerticalAxisCalibrationButtonCommand => this.verticalAxisCalibrationButtonCommand ?? (this.verticalAxisCalibrationButtonCommand = new DelegateCommand(() => { NavigationService.RaiseGoToViewEvent(); this.ContentRegionCurrentViewModel = ViewModels.VerticalAxisCalibrationVMInstance; }));

        public ICommand VerticalOffsetCalibrationButtonCommand => this.verticalOffsetCalibrationButtonCommand ?? (this.verticalOffsetCalibrationButtonCommand = new DelegateCommand(() => { NavigationService.RaiseGoToViewEvent(); this.ContentRegionCurrentViewModel = ViewModels.VerticalOffsetCalibrationVMInstance; }));

        public ICommand WeightControlButtonCommand => this.weightControlButtonCommand ?? (this.weightControlButtonCommand = new DelegateCommand(() => { NavigationService.RaiseGoToViewEvent(); this.ContentRegionCurrentViewModel = ViewModels.WeightControlVMInstance; }));

        public ICommand MachineModeCustomCommand => this.machineModeCustomCommand ?? (this.machineModeCustomCommand = new DelegateCommand(() => this.RaiseClickedOnMachineModeEvent()));

        public ICommand MachineOnMarchCustomCommand => this.machineOnMarchCustomCommand ?? (this.machineOnMarchCustomCommand = new DelegateCommand(() => this.RaiseClickedOnMachineOnMarchEvent()));

        public ICommand BackToVWAPPCommand => this.backToVWAPPCommand ?? (this.backToVWAPPCommand = new DelegateCommand(() => { NavigationService.RaiseBackToVWAppEvent(); ClickedOnMachineModeEventHandler = null; ClickedOnMachineOnMarchEventHandler = null; }));

        public ICommand ErrorButtonCommand => this.errorButtonCommand ?? (this.errorButtonCommand = new DelegateCommand(() => { Debug.Print("TODO: IMPLEMENT ERROR SYSTEM"); }));

        #endregion Commands Properties

        #region Other Properties

        public BindableBase ContentRegionCurrentViewModel { get => this.contentRegionCurrentViewModel; set => this.SetProperty(ref this.contentRegionCurrentViewModel, value); }

        public Boolean MachineModeSelectionBool { get => this.machineModeSelectionBool; set => this.SetProperty(ref this.machineModeSelectionBool, value); }

        public Boolean MachineOnMarchSelectionBool { get => this.machineOnMarchSelectionBool; set => this.SetProperty(ref this.machineOnMarchSelectionBool, value); }

        public BindableBase NavigationRegionCurrentViewModel { get => this.navigationRegionCurrentViewModel; set => this.SetProperty(ref this.navigationRegionCurrentViewModel, value); }

        public Boolean IsNavigationButtonRegionExpanded { get => this.isNavigationButtonRegionExpanded; set => this.SetProperty(ref this.isNavigationButtonRegionExpanded, value); }

        public BindableBase ExitViewButtonRegionCurrentViewModel { get => this.exitViewButtonRegionCurrentViewModel; set => this.SetProperty(ref this.exitViewButtonRegionCurrentViewModel, value); }

        public Boolean IsExitViewButtonRegionExpanded { get => this.isExitViewButtonRegionExpanded; set => this.SetProperty(ref this.isExitViewButtonRegionExpanded, value); }

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

        private void InitializeEvents()
        {
            NavigationService.GoToViewEventHandler += () => this.IsNavigationButtonRegionExpanded = false;
            NavigationService.GoToViewEventHandler += () => this.IsExitViewButtonRegionExpanded = true;
            NavigationService.GoToViewEventHandler += ViewModels.MainWindowBackToIAPPButtonVMInstance.InitializeSingleViewBottomButtons;
            NavigationService.ExitViewEventHandler += () => this.IsNavigationButtonRegionExpanded = true;
            NavigationService.ExitViewEventHandler += () => this.IsExitViewButtonRegionExpanded = false;
            NavigationService.ExitViewEventHandler += ViewModels.MainWindowBackToIAPPButtonVMInstance.FinalizeBottomButtons;
            MainWindow.FinishedMachineModeChangeStateEventHandler += () => { this.MachineModeSelectionBool = !this.MachineModeSelectionBool; };
            MainWindow.FinishedMachineOnMarchChangeStateEventHandler += () => { this.MachineOnMarchSelectionBool = !this.MachineOnMarchSelectionBool; };
            ClickedOnMachineModeEventHandler += () => { };
            ClickedOnMachineOnMarchEventHandler += () => { };
            SensorsStatesChangedEventHandler += () => { };
        }

        private void RaiseSensorsStatesChangedEvent() => SensorsStatesChangedEventHandler();

        private void RaiseClickedOnMachineModeEvent() => ClickedOnMachineModeEventHandler();

        private void RaiseClickedOnMachineOnMarchEvent() => ClickedOnMachineOnMarchEventHandler();

        #endregion Methods
    }
}
