using System.Windows;
using System.Windows.Input;
using Ferretto.VW.InstallationApp.Interfaces;
using Ferretto.VW.InstallationApp.Resources;
using Ferretto.VW.InstallationApp.Resources.Enumerables;
using Unity;
using Prism.Commands;
using Prism.Mvvm;
using Ferretto.VW.Utils.Interfaces;
using System.Threading.Tasks;

namespace Ferretto.VW.InstallationApp
{
    public partial class MainWindowViewModel
    {
        #region Fields

        private ICommand backToMainWindowNavigationButtonsViewCommand;

        private ICommand backToVWAPPCommand;

        private ICommand bayControlButtonCommand;

        private ICommand beltBurnishingButtonCommand;

        private ICommand carouselButtonCommand;

        private ICommand cellsControlButtonCommand;

        private ICommand cellsPanelControlButtonCommand;

        private ICommand cellsSideControlButtonCommand;

        private ICommand drawerLoadingUnloadingTestButtonCommand;

        private ICommand errorButtonCommand;

        private ICommand gate1HeightControlNavigationButtonCommand;

        private ICommand gate2HeightControlNavigationButtonCommand;

        private ICommand gate3HeightControlNavigationButtonCommand;

        private ICommand gates1ControlNavigationButtonCommand;

        private ICommand gates2ControlNavigationButtonCommand;

        private ICommand gates3ControlNavigationButtonCommand;

        private ICommand installationStateButtonCommand;

        private ICommand loadFirstDrawerButtonCommand;

        private ICommand loadingDrawersButtonCommand;

        private ICommand lowSpeedMovementsTestButtonCommand;

        private ICommand lsmtGateEngineButtonCommand;

        private ICommand lsmtHorizontalEngineButtonCommand;

        private ICommand lsmtVerticalEngineButtonCommand;

        private ICommand machineModeCustomCommand;

        private ICommand machineOnMarchCustomCommand;

        private ICommand openHelpWindow;

        private ICommand resolutionCalibrationVerticalAxisButtonCommand;

        private ICommand saveRestoreConfigButtonCommand;

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

        public ICommand BayControlButtonCommand => this.bayControlButtonCommand ?? (this.bayControlButtonCommand = new DelegateCommand(() =>
        {
            this.NavigateToViewAsync<BayControlViewModel, IBayControlViewModel>();
        }));

        public ICommand BeltBurnishingButtonCommand => this.beltBurnishingButtonCommand ?? (this.beltBurnishingButtonCommand = new DelegateCommand(() =>
        {
            this.NavigateToViewAsync<BeltBurnishingViewModel, IBeltBurnishingViewModel>();
        }));

        public ICommand CarouselButtonCommand => this.carouselButtonCommand ?? (this.carouselButtonCommand = new DelegateCommand(() =>
        {
            this.NavigateToViewAsync<LSMTCarouselViewModel, ILSMTCarouselViewModel>();
        }));

        public ICommand CellsControlButtonCommand => this.cellsControlButtonCommand ?? (this.cellsControlButtonCommand = new DelegateCommand(() =>
                {
                    this.NavigateToViewAsync<CellsControlViewModel, ICellsControlViewModel>();
                }));

        public ICommand CellsPanelControlButtonCommand => this.cellsPanelControlButtonCommand ?? (this.cellsPanelControlButtonCommand = new DelegateCommand(() =>
        {
            this.NavigateToViewAsync<CellsPanelsControlViewModel, ICellsPanelsControlViewModel>();
        }));

        public ICommand CellsSideControlButtonCommand => this.cellsSideControlButtonCommand ?? (this.cellsSideControlButtonCommand = new DelegateCommand(() =>
        {
            this.NavigateToViewAsync<CellsSideControlViewModel, ICellsSideControlViewModel>();
        }));

        public ICommand DrawerLoadingUnloadingTestButtonCommand => this.drawerLoadingUnloadingTestButtonCommand ?? (this.drawerLoadingUnloadingTestButtonCommand = new DelegateCommand(() =>
        {
            this.NavigateToViewAsync<DrawerLoadingUnloadingTestViewModel, IDrawerLoadingUnloadingTestViewModel>();
        }));

        public ICommand ErrorButtonCommand => this.errorButtonCommand ?? (this.errorButtonCommand = new DelegateCommand(() =>
        {
            // TODO implement error system
        }));

        public ICommand Gate1HeightControlNavigationButtonCommand => this.gate1HeightControlNavigationButtonCommand ?? (this.gate1HeightControlNavigationButtonCommand = new DelegateCommand(() =>
        {
            this.NavigateToViewAsync<Shutter1HeightControlViewModel, IShutter1HeightControlViewModel>();
        }));

        public ICommand Gate2HeightControlNavigationButtonCommand => this.gate2HeightControlNavigationButtonCommand ?? (this.gate2HeightControlNavigationButtonCommand = new DelegateCommand(() =>
        {
            this.NavigateToViewAsync<Shutter2HeightControlViewModel, IShutter2HeightControlViewModel>();
        }));

        public ICommand Gate3HeightControlNavigationButtonCommand => this.gate3HeightControlNavigationButtonCommand ?? (this.gate3HeightControlNavigationButtonCommand = new DelegateCommand(() =>
        {
            this.NavigateToViewAsync<Shutter3HeightControlViewModel, IShutter3HeightControlViewModel>();
        }));

        public ICommand Gates1ControlNavigationButtonCommand => this.gates1ControlNavigationButtonCommand ?? (this.gates1ControlNavigationButtonCommand = new DelegateCommand(() =>
        {
            this.NavigateToViewAsync<Shutter1ControlViewModel, IShutter1ControlViewModel>();
        }));

        public ICommand Gates2ControlNavigationButtonCommand => this.gates2ControlNavigationButtonCommand ?? (this.gates2ControlNavigationButtonCommand = new DelegateCommand(() =>
        {
            this.NavigateToViewAsync<Shutter2ControlViewModel, IShutter2ControlViewModel>();
        }));

        public ICommand Gates3ControlNavigationButtonCommand => this.gates3ControlNavigationButtonCommand ?? (this.gates3ControlNavigationButtonCommand = new DelegateCommand(() =>
        {
            this.NavigateToViewAsync<Shutter3ControlViewModel, IShutter3ControlViewModel>();
        }));

        public ICommand InstallationStateButtonCommand => this.installationStateButtonCommand ?? (this.installationStateButtonCommand = new DelegateCommand(() =>
        {
            this.NavigateToViewAsync<InstallationStateViewModel, IInstallationStateViewModel>();
        }));

        public ICommand LoadFirstDrawerButtonCommand => this.loadFirstDrawerButtonCommand ?? (this.loadFirstDrawerButtonCommand = new DelegateCommand(() =>
        {
            this.NavigateToViewAsync<LoadFirstDrawerViewModel, ILoadFirstDrawerViewModel>();
        }));

        public ICommand LoadingDrawersButtonCommand => this.loadingDrawersButtonCommand ?? (this.loadingDrawersButtonCommand = new DelegateCommand(() =>
        {
            this.NavigateToViewAsync<LoadingDrawersViewModel, ILoadingDrawersViewModel>();
        }));

        public ICommand LowSpeedMovementsTestButtonCommand => this.lowSpeedMovementsTestButtonCommand ?? (this.lowSpeedMovementsTestButtonCommand = new DelegateCommand(() =>
        {
            this.NavigateToViewAsync<LSMTMainViewModel, ILSMTMainViewModel>();
        }));

        public ICommand LSMTGateEngineButtonCommand => this.lsmtGateEngineButtonCommand ?? (this.lsmtGateEngineButtonCommand = new DelegateCommand(() =>
        {
            this.NavigateToViewAsync<LSMTShutterEngineViewModel, ILSMTShutterEngineViewModel>();
        }));

        public ICommand LSMTHorizontalEngineButtonCommand => this.lsmtHorizontalEngineButtonCommand ?? (this.lsmtHorizontalEngineButtonCommand = new DelegateCommand(() =>
        {
            this.NavigateToViewAsync<LSMTHorizontalEngineViewModel, ILSMTHorizontalEngineViewModel>();
        }));

        public ICommand LSMTVerticalEngineButtonCommand => this.lsmtVerticalEngineButtonCommand ?? (this.lsmtVerticalEngineButtonCommand = new DelegateCommand(() =>
        {
            this.NavigateToViewAsync<LSMTVerticalEngineViewModel, ILSMTVerticalEngineViewModel>();
        }));

        public ICommand ResolutionCalibrationVerticalAxisButtonCommand => this.resolutionCalibrationVerticalAxisButtonCommand ?? (this.resolutionCalibrationVerticalAxisButtonCommand = new DelegateCommand(() =>
        {
            this.NavigateToViewAsync<ResolutionCalibrationVerticalAxisViewModel, IResolutionCalibrationVerticalAxisViewModel>();
        }));

        public ICommand SaveRestoreConfigButtonCommand => this.saveRestoreConfigButtonCommand ?? (this.saveRestoreConfigButtonCommand = new DelegateCommand(() =>
        {
            this.NavigateToViewAsync<SaveRestoreConfigViewModel, ISaveRestoreConfigViewModel>();
        }));

        public ICommand SsBaysButtonCommand => this.ssBaysButtonCommand ?? (this.ssBaysButtonCommand = new DelegateCommand(() =>
        {
            this.NavigateToViewAsync<SSBaysViewModel, ISSBaysViewModel>();
        }));

        public ICommand SsCradleButtonCommand => this.ssCradleButtonCommand ?? (this.ssCradleButtonCommand = new DelegateCommand(() =>
        {
            this.NavigateToViewAsync<SSCradleViewModel, ISSCradleViewModel>();
        }));

        public ICommand SsGateButtonCommand => this.ssGateButtonCommand ?? (this.ssGateButtonCommand = new DelegateCommand(() =>
        {
            this.NavigateToViewAsync<SSShutterViewModel, ISSShutterViewModel>();
        }));

        public ICommand SsNavigationButtonsButtonCommand => this.ssNavigationButtonsButtonCommand ?? (this.ssNavigationButtonsButtonCommand = new DelegateCommand(() =>
        {
            this.isNavigationButtonRegionExpanded = Visibility.Collapsed;
            this.NavigateToViewAsync<SSMainViewModel, ISSMainViewModel>();
            this.container.Resolve<ISSMainViewModel>().SSNavigationRegionCurrentViewModel = (this.container.Resolve<ISSNavigationButtonsViewModel>() as SSNavigationButtonsViewModel);
        }));

        public ICommand SsVariousInputsButtonCommand => this.ssVariousInputsButtonCommand ?? (this.ssVariousInputsButtonCommand = new DelegateCommand(() =>
        {
            this.NavigateToViewAsync<SSVariousInputsViewModel, ISSVariousInputsViewModel>();
        }));

        public ICommand SsVerticalAxisButtonCommand => this.ssVerticalAxisButtonCommand ?? (this.ssVerticalAxisButtonCommand = new DelegateCommand(() =>
        {
            this.NavigateToViewAsync<SSVerticalAxisViewModel, ISSVerticalAxisViewModel>();
        }));

        public ICommand VerticalAxisCalibrationButtonCommand => this.verticalAxisCalibrationButtonCommand ?? (this.verticalAxisCalibrationButtonCommand = new DelegateCommand(() =>
        {
            this.NavigateToViewAsync<VerticalAxisCalibrationViewModel, IVerticalAxisCalibrationViewModel>();
        }));

        public ICommand VerticalOffsetCalibrationButtonCommand => this.verticalOffsetCalibrationButtonCommand ?? (this.verticalOffsetCalibrationButtonCommand = new DelegateCommand(() =>
        {
            this.NavigateToViewAsync<VerticalOffsetCalibrationViewModel, IVerticalOffsetCalibrationViewModel>();
        }));

        public ICommand WeightControlButtonCommand => this.weightControlButtonCommand ?? (this.weightControlButtonCommand = new DelegateCommand(() =>
        {
            this.NavigateToViewAsync<WeightControlViewModel, IWeightControlViewModel>();
        }));

        #endregion

        #region Methods

        private async Task NavigateToViewAsync<T, I>()
            where T : BindableBase, I
            where I : IViewModel
        {
            this.eventAggregator.GetEvent<InstallationApp_Event>().Publish(new InstallationApp_EventMessage(InstallationApp_EventMessageType.EnterView));
            var desiredViewModel = this.container.Resolve<I>() as T;
            await desiredViewModel.OnEnterViewAsync();
            this.container.Resolve<IMainWindowBackToIAPPButtonViewModel>().BackButtonCommand.RegisterCommand(new DelegateCommand(desiredViewModel.ExitFromViewMethod));
            this.ContentRegionCurrentViewModel = desiredViewModel;
        }

        #endregion
    }
}
