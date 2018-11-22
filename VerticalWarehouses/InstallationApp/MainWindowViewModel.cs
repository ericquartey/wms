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

namespace Ferretto.VW.InstallationApp
{
    public class MainWindowViewModel : BindableBase
    {
        #region Fields

        private readonly BeltBurnishingViewModel BeltBurnishingVMInstance = new BeltBurnishingViewModel();
        private readonly CellsControlViewModel CellsControlVMInstance = new CellsControlViewModel();
        private readonly CellsPanelsControlViewModel CellsPanelControlVMInsance = new CellsPanelsControlViewModel();
        private readonly Gate1ControlViewModel Gate1ControlVMInstance = new Gate1ControlViewModel();
        private readonly Gate1HeightControlViewModel Gate1HeightControlVMInstance = new Gate1HeightControlViewModel();
        private readonly Gate2ControlViewModel Gate2ControlVMInstance = new Gate2ControlViewModel();
        private readonly Gate2HeightControlViewModel Gate2HeightControlVMInstance = new Gate2HeightControlViewModel();
        private readonly Gate3ControlViewModel Gate3ControlVMInstance = new Gate3ControlViewModel();
        private readonly Gate3HeightControlViewModel Gate3HeightControlVMInstance = new Gate3HeightControlViewModel();
        private readonly GatesControlNavigationButtonsViewModel GatesControlNavigationButtonsVMInstance = new GatesControlNavigationButtonsViewModel();
        private readonly GatesHeightControlNavigationButtonsViewModel GatesHeightControlNavigationButtonsVMInstance = new GatesHeightControlNavigationButtonsViewModel();
        private readonly GetLocalIPViewModel GetLocalIPVMInstance = new GetLocalIPViewModel();
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

        private ICommand backToMainWindowNavigationButtonsViewCommand;
        private ICommand beltBurnishingButtonCommand;
        private ICommand cellsControlButtonCommand;
        private ICommand cellsPanelControlButtonCommand;
        private BindableBase contentRegionCurrentViewModel;
        private ICommand gate1HeightControlNavigationButtonCommand;
        private ICommand gate2HeightControlNavigationButtonCommand;
        private ICommand gate3HeightControlNavigationButtonCommand;
        private ICommand gateHeightControlButtonCommand;
        private ICommand gates1ControlNavigationButtonCommand;
        private ICommand gates2ControlNavigationButtonCommand;
        private ICommand gates3ControlNavigationButtonCommand;
        private ICommand gatesControlButtonCommand;
        private ICommand getIPButtonCommand;
        private ICommand installationStateButtonCommand;
        private ICommand lowSpeedMovementsTestButtonCommand;
        private ICommand lsmtGateEngineButtonCommand;
        private ICommand lsmtHorizontalEngineButtonCommand;
        private ICommand lsmtVerticalEngineButtonCommand;
        private bool machineModeSelectionBool = true;
        private int machineModeSelectionInt = 0;
        private bool machineOnMarchSelectionBool = false;
        private int machineOnMarchSelectionInt = 0;
        private BindableBase navigationRegionCurrentViewModel;
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

        #endregion Fields

        #region Constructors

        public MainWindowViewModel()
        {
            this.NavigationRegionCurrentViewModel = this.MainWindowNavigationButtonsVMInstance;
        }

        #endregion Constructors

        #region Properties

        public ICommand BackToMainWindowNavigationButtonsViewButtonCommand => this.backToMainWindowNavigationButtonsViewCommand ?? (this.backToMainWindowNavigationButtonsViewCommand = new DelegateCommand(() => { this.NavigationRegionCurrentViewModel = this.MainWindowNavigationButtonsVMInstance; this.ContentRegionCurrentViewModel = null; NavigationService.RaiseExitViewEvent(); }));
        public ICommand BeltBurnishingButtonCommand => this.beltBurnishingButtonCommand ?? (this.beltBurnishingButtonCommand = new DelegateCommand(() => { this.ContentRegionCurrentViewModel = this.BeltBurnishingVMInstance; this.MainWindowNavigationButtonsVMInstance.SetAllNavigationButtonDisabled(); }));
        public ICommand CellsControlButtonCommand => this.cellsControlButtonCommand ?? (this.cellsControlButtonCommand = new DelegateCommand(() => this.ContentRegionCurrentViewModel = this.CellsControlVMInstance));
        public ICommand CellsPanelControlButtonCommand => this.cellsPanelControlButtonCommand ?? (this.cellsPanelControlButtonCommand = new DelegateCommand(() => { this.ContentRegionCurrentViewModel = this.CellsPanelControlVMInsance; this.MainWindowNavigationButtonsVMInstance.SetAllNavigationButtonDisabled(); }));
        public BindableBase ContentRegionCurrentViewModel { get => this.contentRegionCurrentViewModel; set => this.SetProperty(ref this.contentRegionCurrentViewModel, value); }
        public ICommand Gate1HeightControlNavigationButtonCommand => this.gate1HeightControlNavigationButtonCommand ?? (this.gate1HeightControlNavigationButtonCommand = new DelegateCommand(() => this.ContentRegionCurrentViewModel = this.Gate1HeightControlVMInstance));
        public ICommand Gate2HeightControlNavigationButtonCommand => this.gate2HeightControlNavigationButtonCommand ?? (this.gate2HeightControlNavigationButtonCommand = new DelegateCommand(() => this.ContentRegionCurrentViewModel = this.Gate2HeightControlVMInstance));
        public ICommand Gate3HeightControlNavigationButtonCommand => this.gate3HeightControlNavigationButtonCommand ?? (this.gate3HeightControlNavigationButtonCommand = new DelegateCommand(() => this.ContentRegionCurrentViewModel = this.Gate3HeightControlVMInstance));
        public ICommand GateHeightControlButtonCommand => this.gateHeightControlButtonCommand ?? (this.gateHeightControlButtonCommand = new DelegateCommand(() => this.ContentRegionCurrentViewModel = this.Gate2HeightControlVMInstance));
        public ICommand Gates1ControlNavigationButtonCommand => this.gates1ControlNavigationButtonCommand ?? (this.gates1ControlNavigationButtonCommand = new DelegateCommand(() => this.ContentRegionCurrentViewModel = this.Gate1ControlVMInstance));
        public ICommand Gates2ControlNavigationButtonCommand => this.gates2ControlNavigationButtonCommand ?? (this.gates2ControlNavigationButtonCommand = new DelegateCommand(() => this.ContentRegionCurrentViewModel = this.Gate2ControlVMInstance));
        public ICommand Gates3ControlNavigationButtonCommand => this.gates3ControlNavigationButtonCommand ?? (this.gates3ControlNavigationButtonCommand = new DelegateCommand(() => this.ContentRegionCurrentViewModel = this.Gate3ControlVMInstance));
        public ICommand GatesControlButtonCommand => this.gatesControlButtonCommand ?? (this.gatesControlButtonCommand = new DelegateCommand(() => this.ContentRegionCurrentViewModel = this.Gate2ControlVMInstance));
        public ICommand GetIPButtonCommand => this.getIPButtonCommand ?? (this.getIPButtonCommand = new DelegateCommand(() => this.ContentRegionCurrentViewModel = this.GetLocalIPVMInstance));
        public ICommand InstallationStateButtonCommand => this.installationStateButtonCommand ?? (this.installationStateButtonCommand = new DelegateCommand(() => this.ContentRegionCurrentViewModel = this.InstallationStateVMInstance));
        public ICommand LowSpeedMovementsTestButtonCommand => this.lowSpeedMovementsTestButtonCommand ?? (this.lowSpeedMovementsTestButtonCommand = new DelegateCommand(() => { this.NavigationRegionCurrentViewModel = this.LSMTNavigationButtonsVMInstance; this.ContentRegionCurrentViewModel = null; }));
        public ICommand LSMTGateEngineButtonCommand => this.lsmtGateEngineButtonCommand ?? (this.lsmtGateEngineButtonCommand = new DelegateCommand(() => { this.ContentRegionCurrentViewModel = this.LSMTGateEngineVMInstance; }));
        public ICommand LSMTHorizontalEngineButtonCommand => this.lsmtHorizontalEngineButtonCommand ?? (this.lsmtHorizontalEngineButtonCommand = new DelegateCommand(() => { this.ContentRegionCurrentViewModel = this.LSMTHorizontalEngineVMInstance; }));
        public ICommand LSMTVerticalEngineButtonCommand => this.lsmtVerticalEngineButtonCommand ?? (this.lsmtVerticalEngineButtonCommand = new DelegateCommand(() => { this.ContentRegionCurrentViewModel = this.LSMTVerticalEngineVMInstance; }));
        public Boolean MachineModeSelectionBool { get => this.machineModeSelectionBool; set => this.SetProperty(ref this.machineModeSelectionBool, value); }

        public Int32 MachineModeSelectionInt
        {
            get => this.machineModeSelectionInt;
            set
            {
                this.SetProperty(ref this.machineModeSelectionInt, value);
                this.MachineModeSelectionBool = this.machineModeSelectionInt == 0 ? true : false;
            }
        }

        public Boolean MachineOnMarchSelectionBool { get => this.machineOnMarchSelectionBool; set => this.SetProperty(ref this.machineOnMarchSelectionBool, value); }

        public Int32 MachineOnMarchSelectionInt
        {
            get => this.machineOnMarchSelectionInt;
            set
            {
                this.SetProperty(ref this.machineOnMarchSelectionInt, value);
                this.MachineOnMarchSelectionBool = this.machineOnMarchSelectionInt == 0 ? false : true;
            }
        }

        public BindableBase NavigationRegionCurrentViewModel { get => this.navigationRegionCurrentViewModel; set => this.SetProperty(ref this.navigationRegionCurrentViewModel, value); }
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

        #endregion Properties
    }
}
