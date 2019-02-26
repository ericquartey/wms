using System.Diagnostics;
using System.Windows.Input;
using Ferretto.VW.InstallationApp.Resources;
using Ferretto.VW.InstallationApp.Resources.Enumerables;
using Microsoft.Practices.Unity;
using Prism.Commands;
using Prism.Events;

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
            this.NavigationRegionCurrentViewModel = (MainWindowNavigationButtonsViewModel)this.Container.Resolve<IMainWindowNavigationButtonsViewModel>();
            this.ContentRegionCurrentViewModel = (IdleViewModel)this.Container.Resolve<IIdleViewModel>();
            this.eventAggregator.GetEvent<InstallationApp_Event>().Publish(new InstallationApp_EventMessage(InstallationApp_EventMessageType.ExitView));
        }));

        public ICommand BackToVWAPPCommand => this.backToVWAPPCommand ?? (this.backToVWAPPCommand = new DelegateCommand(() =>
        {
            this.IsPopupOpen = false;
            this.eventAggregator.GetEvent<InstallationApp_Event>().Publish(new InstallationApp_EventMessage(InstallationApp_EventMessageType.BackToVWApp));
            ClickedOnMachineModeEventHandler = null;
            ClickedOnMachineOnMarchEventHandler = null;
        }));

        public ICommand BeltBurnishingButtonCommand => this.beltBurnishingButtonCommand ?? (this.beltBurnishingButtonCommand = new DelegateCommand(() =>
        {
            this.eventAggregator.GetEvent<InstallationApp_Event>().Publish(new InstallationApp_EventMessage(InstallationApp_EventMessageType.EnterView));
            this.ContentRegionCurrentViewModel = (BeltBurnishingViewModel)this.Container.Resolve<IBeltBurnishingViewModel>();
        }));

        public ICommand CellsControlButtonCommand => this.cellsControlButtonCommand ?? (this.cellsControlButtonCommand = new DelegateCommand(() =>
        {
            this.eventAggregator.GetEvent<InstallationApp_Event>().Publish(new InstallationApp_EventMessage(InstallationApp_EventMessageType.EnterView));
            this.ContentRegionCurrentViewModel = (CellsControlViewModel)this.Container.Resolve<ICellsControlViewModel>();
        }));

        public ICommand CellsPanelControlButtonCommand => this.cellsPanelControlButtonCommand ?? (this.cellsPanelControlButtonCommand = new DelegateCommand(() =>
        {
            this.eventAggregator.GetEvent<InstallationApp_Event>().Publish(new InstallationApp_EventMessage(InstallationApp_EventMessageType.EnterView));
            this.ContentRegionCurrentViewModel = (CellsPanelsControlViewModel)this.Container.Resolve<ICellsPanelsControlViewModel>();
        }));

        public ICommand ErrorButtonCommand => this.errorButtonCommand ?? (this.errorButtonCommand = new DelegateCommand(() =>
        {
            Debug.Print("TODO: IMPLEMENT ERROR SYSTEM");
        }));

        public ICommand Gate1HeightControlNavigationButtonCommand => this.gate1HeightControlNavigationButtonCommand ?? (this.gate1HeightControlNavigationButtonCommand = new DelegateCommand(() =>
        {
            this.eventAggregator.GetEvent<InstallationApp_Event>().Publish(new InstallationApp_EventMessage(InstallationApp_EventMessageType.EnterView));
            this.ContentRegionCurrentViewModel = (Gate1HeightControlViewModel)this.Container.Resolve<IGate1HeightControlViewModel>();
        }));

        public ICommand Gate2HeightControlNavigationButtonCommand => this.gate2HeightControlNavigationButtonCommand ?? (this.gate2HeightControlNavigationButtonCommand = new DelegateCommand(() =>
        {
            this.eventAggregator.GetEvent<InstallationApp_Event>().Publish(new InstallationApp_EventMessage(InstallationApp_EventMessageType.EnterView));
            this.ContentRegionCurrentViewModel = (Gate2HeightControlViewModel)this.Container.Resolve<IGate2HeightControlViewModel>();
        }));

        public ICommand Gate3HeightControlNavigationButtonCommand => this.gate3HeightControlNavigationButtonCommand ?? (this.gate3HeightControlNavigationButtonCommand = new DelegateCommand(() =>
        {
            this.eventAggregator.GetEvent<InstallationApp_Event>().Publish(new InstallationApp_EventMessage(InstallationApp_EventMessageType.EnterView));
            this.ContentRegionCurrentViewModel = (Gate3HeightControlViewModel)this.Container.Resolve<IGate3HeightControlViewModel>();
        }));

        public ICommand Gates1ControlNavigationButtonCommand => this.gates1ControlNavigationButtonCommand ?? (this.gates1ControlNavigationButtonCommand = new DelegateCommand(() =>
        {
            this.eventAggregator.GetEvent<InstallationApp_Event>().Publish(new InstallationApp_EventMessage(InstallationApp_EventMessageType.EnterView));
            this.ContentRegionCurrentViewModel = (Gate1ControlViewModel)this.Container.Resolve<IGate1ControlViewModel>();
        }));

        public ICommand Gates2ControlNavigationButtonCommand => this.gates2ControlNavigationButtonCommand ?? (this.gates2ControlNavigationButtonCommand = new DelegateCommand(() =>
        {
            this.eventAggregator.GetEvent<InstallationApp_Event>().Publish(new InstallationApp_EventMessage(InstallationApp_EventMessageType.EnterView));
            this.ContentRegionCurrentViewModel = (Gate2ControlViewModel)this.Container.Resolve<IGate2ControlViewModel>();
        }));

        public ICommand Gates3ControlNavigationButtonCommand => this.gates3ControlNavigationButtonCommand ?? (this.gates3ControlNavigationButtonCommand = new DelegateCommand(() =>
        {
            this.eventAggregator.GetEvent<InstallationApp_Event>().Publish(new InstallationApp_EventMessage(InstallationApp_EventMessageType.EnterView));
            this.ContentRegionCurrentViewModel = (Gate3ControlViewModel)this.Container.Resolve<IGate3ControlViewModel>();
        }));

        public ICommand InstallationStateButtonCommand => this.installationStateButtonCommand ?? (this.installationStateButtonCommand = new DelegateCommand(() =>
        {
            this.eventAggregator.GetEvent<InstallationApp_Event>().Publish(new InstallationApp_EventMessage(InstallationApp_EventMessageType.EnterView));
            this.ContentRegionCurrentViewModel = (InstallationStateViewModel)this.Container.Resolve<IInstallationStateViewModel>();
        }));

        public ICommand LowSpeedMovementsTestButtonCommand => this.lowSpeedMovementsTestButtonCommand ?? (this.lowSpeedMovementsTestButtonCommand = new DelegateCommand(() =>
        {
            this.eventAggregator.GetEvent<InstallationApp_Event>().Publish(new InstallationApp_EventMessage(InstallationApp_EventMessageType.EnterView));
            this.ContentRegionCurrentViewModel = (LSMTMainViewModel)this.Container.Resolve<ILSMTMainViewModel>();
        }));

        public ICommand LSMTGateEngineButtonCommand => this.lsmtGateEngineButtonCommand ?? (this.lsmtGateEngineButtonCommand = new DelegateCommand(() =>
        {
            this.eventAggregator.GetEvent<InstallationApp_Event>().Publish(new InstallationApp_EventMessage(InstallationApp_EventMessageType.EnterView));
            this.ContentRegionCurrentViewModel = (LSMTGateEngineViewModel)this.Container.Resolve<ILSMTGateEngineViewModel>();
        }));

        public ICommand LSMTHorizontalEngineButtonCommand => this.lsmtHorizontalEngineButtonCommand ?? (this.lsmtHorizontalEngineButtonCommand = new DelegateCommand(() =>
        {
            this.eventAggregator.GetEvent<InstallationApp_Event>().Publish(new InstallationApp_EventMessage(InstallationApp_EventMessageType.EnterView));
            this.ContentRegionCurrentViewModel = (LSMTHorizontalEngineViewModel)this.Container.Resolve<ILSMTHorizontalEngineViewModel>();
        }));

        public ICommand LSMTVerticalEngineButtonCommand => this.lsmtVerticalEngineButtonCommand ?? (this.lsmtVerticalEngineButtonCommand = new DelegateCommand(() =>
        {
            this.eventAggregator.GetEvent<InstallationApp_Event>().Publish(new InstallationApp_EventMessage(InstallationApp_EventMessageType.EnterView));
            this.ContentRegionCurrentViewModel = (LSMTVerticalEngineViewModel)this.Container.Resolve<ILSMTVerticalEngineViewModel>();
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
            this.eventAggregator.GetEvent<InstallationApp_Event>().Publish(new InstallationApp_EventMessage(InstallationApp_EventMessageType.EnterView));
            this.ContentRegionCurrentViewModel = (ResolutionCalibrationVerticalAxisViewModel)this.Container.Resolve<IResolutionCalibrationVerticalAxisViewModel>();
            ((ResolutionCalibrationVerticalAxisViewModel)this.ContentRegionCurrentViewModel)?.SubscribeMethodToEvent();
        }));

        public ICommand SsBaysButtonCommand => this.ssBaysButtonCommand ?? (this.ssBaysButtonCommand = new DelegateCommand(() =>
        {
            this.eventAggregator.GetEvent<InstallationApp_Event>().Publish(new InstallationApp_EventMessage(InstallationApp_EventMessageType.EnterView));
            this.ContentRegionCurrentViewModel = (SSBaysViewModel)this.Container.Resolve<ISSBaysViewModel>();
        }));

        public ICommand SsCradleButtonCommand => this.ssCradleButtonCommand ?? (this.ssCradleButtonCommand = new DelegateCommand(() =>
        {
            this.eventAggregator.GetEvent<InstallationApp_Event>().Publish(new InstallationApp_EventMessage(InstallationApp_EventMessageType.EnterView));
            this.ContentRegionCurrentViewModel = (SSCradleViewModel)this.Container.Resolve<ISSCradleViewModel>();
        }));

        public ICommand SsGateButtonCommand => this.ssGateButtonCommand ?? (this.ssGateButtonCommand = new DelegateCommand(() =>
        {
            this.eventAggregator.GetEvent<InstallationApp_Event>().Publish(new InstallationApp_EventMessage(InstallationApp_EventMessageType.EnterView));
            this.ContentRegionCurrentViewModel = (SSGateViewModel)this.Container.Resolve<ISSGateViewModel>();
        }));

        public ICommand SSNavigationButtonsButtonCommand => this.ssNavigationButtonsButtonCommand ?? (this.ssNavigationButtonsButtonCommand = new DelegateCommand(() =>
        {
            this.eventAggregator.GetEvent<InstallationApp_Event>().Publish(new InstallationApp_EventMessage(InstallationApp_EventMessageType.EnterView));
            this.ContentRegionCurrentViewModel = (SSMainViewModel)this.Container.Resolve<ISSMainViewModel>();
        }));

        public ICommand SsVariousInputsButtonCommand => this.ssVariousInputsButtonCommand ?? (this.ssVariousInputsButtonCommand = new DelegateCommand(() =>
        {
            this.eventAggregator.GetEvent<InstallationApp_Event>().Publish(new InstallationApp_EventMessage(InstallationApp_EventMessageType.EnterView));
            this.ContentRegionCurrentViewModel = (SSVariousInputsViewModel)this.Container.Resolve<ISSVariousInputsViewModel>();
        }));

        public ICommand SsVerticalAxisButtonCommand => this.ssVerticalAxisButtonCommand ?? (this.ssVerticalAxisButtonCommand = new DelegateCommand(() =>
        {
            this.eventAggregator.GetEvent<InstallationApp_Event>().Publish(new InstallationApp_EventMessage(InstallationApp_EventMessageType.EnterView));
            this.ContentRegionCurrentViewModel = (SSVerticalAxisViewModel)this.Container.Resolve<ISSVerticalAxisViewModel>();
        }));

        public ICommand VerticalAxisCalibrationButtonCommand => this.verticalAxisCalibrationButtonCommand ?? (this.verticalAxisCalibrationButtonCommand = new DelegateCommand(() =>
        {
            this.eventAggregator.GetEvent<InstallationApp_Event>().Subscribe(
                (message) => { ((VerticalAxisCalibrationViewModel)this.Container.Resolve<IVerticalAxisCalibrationViewModel>()).SubscribeMethodToEvent(); },
                ThreadOption.PublisherThread,
                false,
                message => message.Type == InstallationApp_EventMessageType.EnterView);
            this.eventAggregator.GetEvent<InstallationApp_Event>().Subscribe(
                (message) => { ((VerticalAxisCalibrationViewModel)this.Container.Resolve<IVerticalAxisCalibrationViewModel>()).UnSubscribeMethodFromEvent(); },
                ThreadOption.PublisherThread,
                false,
                message => message.Type == InstallationApp_EventMessageType.ExitView);
            this.eventAggregator.GetEvent<InstallationApp_Event>().Publish(new InstallationApp_EventMessage(InstallationApp_EventMessageType.EnterView));
            this.ContentRegionCurrentViewModel = (VerticalAxisCalibrationViewModel)this.Container.Resolve<IVerticalAxisCalibrationViewModel>();
        }));

        public ICommand VerticalOffsetCalibrationButtonCommand => this.verticalOffsetCalibrationButtonCommand ?? (this.verticalOffsetCalibrationButtonCommand = new DelegateCommand(() =>
        {
            this.eventAggregator.GetEvent<InstallationApp_Event>().Publish(new InstallationApp_EventMessage(InstallationApp_EventMessageType.EnterView));
            this.ContentRegionCurrentViewModel = (VerticalOffsetCalibrationViewModel)this.Container.Resolve<IVerticalOffsetCalibrationViewModel>();
            ((VerticalOffsetCalibrationViewModel)this.Container.Resolve<IVerticalOffsetCalibrationViewModel>()).SubscribeMethodToEvent();
            ((MainWindowBackToIAPPButtonViewModel)this.Container.Resolve<IMainWindowBackToIAPPButtonViewModel>()).BackButtonCommand.RegisterCommand(((VerticalOffsetCalibrationViewModel)this.Container.Resolve<IVerticalOffsetCalibrationViewModel>()).ExitFromViewCommand);
        }));

        public ICommand WeightControlButtonCommand => this.weightControlButtonCommand ?? (this.weightControlButtonCommand = new DelegateCommand(() =>
        {
            this.eventAggregator.GetEvent<InstallationApp_Event>().Publish(new InstallationApp_EventMessage(InstallationApp_EventMessageType.EnterView));
            this.ContentRegionCurrentViewModel = (WeightControlViewModel)this.Container.Resolve<IWeightControlViewModel>();
            ((WeightControlViewModel)this.Container.Resolve<IWeightControlViewModel>()).SubscribeMethodToEvent();
            ((MainWindowBackToIAPPButtonViewModel)this.Container.Resolve<IMainWindowBackToIAPPButtonViewModel>()).BackButtonCommand.RegisterCommand(((WeightControlViewModel)this.Container.Resolve<IWeightControlViewModel>()).ExitFromViewCommand);
        }));

        #endregion
    }
}
