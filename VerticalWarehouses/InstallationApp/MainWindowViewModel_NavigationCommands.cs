using System.Diagnostics;
using System.Windows.Input;
using Ferretto.VW.Navigation;
using Microsoft.Practices.Unity;
using Prism.Commands;

namespace Ferretto.VW.InstallationApp
{
    public partial class MainWindowViewModel
    {
        #region Fields

        private ICommand backToMainWindowNavigationButtonsViewCommand;

        private ICommand backToVWAPPCommand;

        private ICommand beltBurnishingButtonCommand;

        private ICommand cellsControlButtonCommand;

        private ICommand cellsPanelControlButtonCommand;

        private ICommand errorButtonCommand;

        private ICommand gate1HeightControlNavigationButtonCommand;

        private ICommand gate2HeightControlNavigationButtonCommand;

        private ICommand gate3HeightControlNavigationButtonCommand;

        private ICommand gates1ControlNavigationButtonCommand;

        private ICommand gates2ControlNavigationButtonCommand;

        private ICommand gates3ControlNavigationButtonCommand;

        private ICommand installationStateButtonCommand;

        private ICommand lowSpeedMovementsTestButtonCommand;

        private ICommand lsmtGateEngineButtonCommand;

        private ICommand lsmtHorizontalEngineButtonCommand;

        private ICommand lsmtVerticalEngineButtonCommand;

        private ICommand machineModeCustomCommand;

        private ICommand machineOnMarchCustomCommand;

        private ICommand openHelpWindow;

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

        #endregion

        #region Properties

        public ICommand BackToMainWindowNavigationButtonsViewButtonCommand => this.backToMainWindowNavigationButtonsViewCommand ?? (this.backToMainWindowNavigationButtonsViewCommand = new DelegateCommand(() =>
        {
            this.NavigationRegionCurrentViewModel = (MainWindowNavigationButtonsViewModel)this.container.Resolve<IMainWindowNavigationButtonsViewModel>();
            this.ContentRegionCurrentViewModel = (IdleViewModel)this.container.Resolve<IIdleViewModel>();
            NavigationService.RaiseExitViewEvent();
        }));

        public ICommand BackToVWAPPCommand => this.backToVWAPPCommand ?? (this.backToVWAPPCommand = new DelegateCommand(() =>
        {
            this.IsPopupOpen = false;
            NavigationService.RaiseBackToVWAppEvent();
            ClickedOnMachineModeEventHandler = null;
            ClickedOnMachineOnMarchEventHandler = null;
        }));

        public ICommand BeltBurnishingButtonCommand => this.beltBurnishingButtonCommand ?? (this.beltBurnishingButtonCommand = new DelegateCommand(() =>
        {
            NavigationService.RaiseGoToViewEvent();
            this.ContentRegionCurrentViewModel = (BeltBurnishingViewModel)this.container.Resolve<IBeltBurnishingViewModel>();
        }));

        public ICommand CellsControlButtonCommand => this.cellsControlButtonCommand ?? (this.cellsControlButtonCommand = new DelegateCommand(() =>
        {
            NavigationService.RaiseGoToViewEvent();
            this.ContentRegionCurrentViewModel = (CellsControlViewModel)this.container.Resolve<ICellsControlViewModel>();
        }));

        public ICommand CellsPanelControlButtonCommand => this.cellsPanelControlButtonCommand ?? (this.cellsPanelControlButtonCommand = new DelegateCommand(() =>
        {
            NavigationService.RaiseGoToViewEvent();
            this.ContentRegionCurrentViewModel = (CellsPanelsControlViewModel)this.container.Resolve<ICellsPanelsControlViewModel>();
        }));

        public ICommand ErrorButtonCommand => this.errorButtonCommand ?? (this.errorButtonCommand = new DelegateCommand(() =>
        {
            Debug.Print("TODO: IMPLEMENT ERROR SYSTEM");
        }));

        public ICommand Gate1HeightControlNavigationButtonCommand => this.gate1HeightControlNavigationButtonCommand ?? (this.gate1HeightControlNavigationButtonCommand = new DelegateCommand(() =>
        {
            NavigationService.RaiseGoToViewEvent();
            this.ContentRegionCurrentViewModel = (Gate1HeightControlViewModel)this.container.Resolve<IGate1HeightControlViewModel>();
        }));

        public ICommand Gate2HeightControlNavigationButtonCommand => this.gate2HeightControlNavigationButtonCommand ?? (this.gate2HeightControlNavigationButtonCommand = new DelegateCommand(() =>
        {
            NavigationService.RaiseGoToViewEvent();
            this.ContentRegionCurrentViewModel = (Gate2HeightControlViewModel)this.container.Resolve<IGate2HeightControlViewModel>();
        }));

        public ICommand Gate3HeightControlNavigationButtonCommand => this.gate3HeightControlNavigationButtonCommand ?? (this.gate3HeightControlNavigationButtonCommand = new DelegateCommand(() =>
        {
            NavigationService.RaiseGoToViewEvent();
            this.ContentRegionCurrentViewModel = (Gate3HeightControlViewModel)this.container.Resolve<IGate3HeightControlViewModel>();
        }));

        public ICommand Gates1ControlNavigationButtonCommand => this.gates1ControlNavigationButtonCommand ?? (this.gates1ControlNavigationButtonCommand = new DelegateCommand(() =>
        {
            NavigationService.RaiseGoToViewEvent();
            this.ContentRegionCurrentViewModel = (Gate1ControlViewModel)this.container.Resolve<IGate1ControlViewModel>();
        }));

        public ICommand Gates2ControlNavigationButtonCommand => this.gates2ControlNavigationButtonCommand ?? (this.gates2ControlNavigationButtonCommand = new DelegateCommand(() =>
        {
            NavigationService.RaiseGoToViewEvent();
            this.ContentRegionCurrentViewModel = (Gate2ControlViewModel)this.container.Resolve<IGate2ControlViewModel>();
        }));

        public ICommand Gates3ControlNavigationButtonCommand => this.gates3ControlNavigationButtonCommand ?? (this.gates3ControlNavigationButtonCommand = new DelegateCommand(() =>
        {
            NavigationService.RaiseGoToViewEvent();
            this.ContentRegionCurrentViewModel = (Gate3ControlViewModel)this.container.Resolve<IGate3ControlViewModel>();
        }));

        public ICommand InstallationStateButtonCommand => this.installationStateButtonCommand ?? (this.installationStateButtonCommand = new DelegateCommand(() =>
        {
            NavigationService.RaiseGoToViewEvent();
            this.ContentRegionCurrentViewModel = (InstallationStateViewModel)this.container.Resolve<IInstallationStateViewModel>();
        }));

        public ICommand LowSpeedMovementsTestButtonCommand => this.lowSpeedMovementsTestButtonCommand ?? (this.lowSpeedMovementsTestButtonCommand = new DelegateCommand(() =>
        {
            NavigationService.RaiseGoToViewEvent();
            this.ContentRegionCurrentViewModel = (LSMTMainViewModel)this.container.Resolve<ILSMTMainViewModel>();
        }));

        public ICommand LSMTGateEngineButtonCommand => this.lsmtGateEngineButtonCommand ?? (this.lsmtGateEngineButtonCommand = new DelegateCommand(() =>
        {
            NavigationService.RaiseGoToViewEvent();
            this.ContentRegionCurrentViewModel = (LSMTGateEngineViewModel)this.container.Resolve<ILSMTGateEngineViewModel>();
        }));

        public ICommand LSMTHorizontalEngineButtonCommand => this.lsmtHorizontalEngineButtonCommand ?? (this.lsmtHorizontalEngineButtonCommand = new DelegateCommand(() =>
        {
            NavigationService.RaiseGoToViewEvent();
            this.ContentRegionCurrentViewModel = (LSMTHorizontalEngineViewModel)this.container.Resolve<ILSMTHorizontalEngineViewModel>();
        }));

        public ICommand LSMTVerticalEngineButtonCommand => this.lsmtVerticalEngineButtonCommand ?? (this.lsmtVerticalEngineButtonCommand = new DelegateCommand(() =>
        {
            NavigationService.RaiseGoToViewEvent();
            this.ContentRegionCurrentViewModel = (LSMTVerticalEngineViewModel)this.container.Resolve<ILSMTVerticalEngineViewModel>();
        }));

        public ICommand MachineModeCustomCommand => this.machineModeCustomCommand ?? (this.machineModeCustomCommand = new DelegateCommand(() => this.RaiseClickedOnMachineModeEvent()));

        public ICommand MachineOnMarchCustomCommand => this.machineOnMarchCustomCommand ?? (this.machineOnMarchCustomCommand = new DelegateCommand(() => this.RaiseClickedOnMachineOnMarchEvent()));

        public ICommand OpenHelpWindow => this.openHelpWindow ?? (this.openHelpWindow = new DelegateCommand(() =>
        {
            this.helpWindow.Show();
            this.helpWindow.HelpContentRegion.Content = this.contentRegionCurrentViewModel;
        }));

        public ICommand ResolutionCalibrationVerticalAxisButtonCommand => this.resolutionCalibrationVerticalAxisButtonCommand ?? (this.resolutionCalibrationVerticalAxisButtonCommand = new DelegateCommand(() =>
        {
            NavigationService.RaiseGoToViewEvent();
            this.ContentRegionCurrentViewModel = (ResolutionCalibrationVerticalAxisViewModel)this.container.Resolve<IResolutionCalibrationVerticalAxisViewModel>();
            ((ResolutionCalibrationVerticalAxisViewModel)this.ContentRegionCurrentViewModel)?.SubscribeMethodToEvent();
        }));

        public ICommand SsBaysButtonCommand => this.ssBaysButtonCommand ?? (this.ssBaysButtonCommand = new DelegateCommand(() =>
        {
            NavigationService.RaiseGoToViewEvent();
            this.ContentRegionCurrentViewModel = (SSBaysViewModel)this.container.Resolve<ISSBaysViewModel>();
        }));

        public ICommand SsCradleButtonCommand => this.ssCradleButtonCommand ?? (this.ssCradleButtonCommand = new DelegateCommand(() =>
        {
            NavigationService.RaiseGoToViewEvent();
            this.ContentRegionCurrentViewModel = (SSCradleViewModel)this.container.Resolve<ISSCradleViewModel>();
        }));

        public ICommand SsGateButtonCommand => this.ssGateButtonCommand ?? (this.ssGateButtonCommand = new DelegateCommand(() =>
        {
            NavigationService.RaiseGoToViewEvent();
            this.ContentRegionCurrentViewModel = (SSGateViewModel)this.container.Resolve<ISSGateViewModel>();
        }));

        public ICommand SSNavigationButtonsButtonCommand => this.ssNavigationButtonsButtonCommand ?? (this.ssNavigationButtonsButtonCommand = new DelegateCommand(() =>
        {
            NavigationService.RaiseGoToViewEvent();
            this.ContentRegionCurrentViewModel = (SSMainViewModel)this.container.Resolve<ISSMainViewModel>();
        }));

        public ICommand SsVariousInputsButtonCommand => this.ssVariousInputsButtonCommand ?? (this.ssVariousInputsButtonCommand = new DelegateCommand(() =>
        {
            NavigationService.RaiseGoToViewEvent();
            this.ContentRegionCurrentViewModel = (SSVariousInputsViewModel)this.container.Resolve<ISSVariousInputsViewModel>();
        }));

        public ICommand SsVerticalAxisButtonCommand => this.ssVerticalAxisButtonCommand ?? (this.ssVerticalAxisButtonCommand = new DelegateCommand(() =>
        {
            NavigationService.RaiseGoToViewEvent();
            this.ContentRegionCurrentViewModel = (SSVerticalAxisViewModel)this.container.Resolve<ISSVerticalAxisViewModel>();
        }));

        public ICommand VerticalAxisCalibrationButtonCommand => this.verticalAxisCalibrationButtonCommand ?? (this.verticalAxisCalibrationButtonCommand = new DelegateCommand(() =>
        {
            NavigationService.GoToViewEventHandler += ((VerticalAxisCalibrationViewModel)this.container.Resolve<IVerticalAxisCalibrationViewModel>()).SubscribeMethodToEvent;
            NavigationService.ExitViewEventHandler += ((VerticalAxisCalibrationViewModel)this.container.Resolve<IVerticalAxisCalibrationViewModel>()).UnSubscribeMethodFromEvent;
            NavigationService.RaiseGoToViewEvent();
            this.ContentRegionCurrentViewModel = (VerticalAxisCalibrationViewModel)this.container.Resolve<IVerticalAxisCalibrationViewModel>();
        }));

        public ICommand VerticalOffsetCalibrationButtonCommand => this.verticalOffsetCalibrationButtonCommand ?? (this.verticalOffsetCalibrationButtonCommand = new DelegateCommand(() =>
        {
            NavigationService.RaiseGoToViewEvent();
            this.ContentRegionCurrentViewModel = (VerticalOffsetCalibrationViewModel)this.container.Resolve<IVerticalOffsetCalibrationViewModel>();
            ((VerticalOffsetCalibrationViewModel)this.container.Resolve<IVerticalOffsetCalibrationViewModel>()).SubscribeMethodToEvent();
            ((MainWindowBackToIAPPButtonViewModel)this.container.Resolve<IMainWindowBackToIAPPButtonViewModel>()).BackButtonCommand.RegisterCommand(((VerticalOffsetCalibrationViewModel)this.container.Resolve<IVerticalOffsetCalibrationViewModel>()).ExitFromViewCommand);
        }));

        public ICommand WeightControlButtonCommand => this.weightControlButtonCommand ?? (this.weightControlButtonCommand = new DelegateCommand(() =>
        {
            NavigationService.RaiseGoToViewEvent();
            this.ContentRegionCurrentViewModel = (WeightControlViewModel)this.container.Resolve<IWeightControlViewModel>();
            ((WeightControlViewModel)this.container.Resolve<IWeightControlViewModel>()).SubscribeMethodToEvent();
            ((MainWindowBackToIAPPButtonViewModel)this.container.Resolve<IMainWindowBackToIAPPButtonViewModel>()).BackButtonCommand.RegisterCommand(((WeightControlViewModel)this.container.Resolve<IWeightControlViewModel>()).ExitFromViewCommand);
        }));

        #endregion
    }
}
