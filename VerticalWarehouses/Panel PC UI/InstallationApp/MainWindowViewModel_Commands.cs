using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Ferretto.VW.App.Installation.Interfaces;
using Ferretto.VW.App.Installation.Resources;
using Ferretto.VW.App.Installation.Resources.Enumerables;
using Ferretto.VW.App.Installation.ViewsAndViewModels.LowSpeedMovements;
using Ferretto.VW.App.Installation.ViewsAndViewModels.SensorsState;
using Ferretto.VW.App.Installation.ViewsAndViewModels.ShuttersControl;
using Ferretto.VW.App.Installation.ViewsAndViewModels.ShuttersHeightControl;
using Ferretto.VW.App.Installation.ViewsAndViewModels.SingleViews;
using Ferretto.VW.Utils.Interfaces;
using Prism.Commands;
using Prism.Mvvm;
using Unity;

namespace Ferretto.VW.App.Installation
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Major Code Smell",
        "S1200:Classes should not be coupled to too many other classes (Single Responsibility Principle)",
        Justification = "This class needs refactoring to decouple it from the navigation.")]
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

        private ICommand manualDrawerStoreRecallButtonCommand;

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
        private DelegateCommand dddddddButtonCommand;

        #endregion

        #region Properties

        public ICommand BayControlButtonCommand =>
           this.bayControlButtonCommand
           ??
           (this.bayControlButtonCommand = new DelegateCommand(
               async () => await this.NavigateToViewAsync<BayControlViewModel, IBayControlViewModel>()));

        public ICommand BeltBurnishingButtonCommand =>
            this.beltBurnishingButtonCommand
            ??
            (this.beltBurnishingButtonCommand = new DelegateCommand(
                async () => await this.NavigateToViewAsync<BeltBurnishingViewModel, IBeltBurnishingViewModel>()));

        public ICommand CarouselButtonCommand =>
            this.carouselButtonCommand
            ??
            (this.carouselButtonCommand = new DelegateCommand(
                async () => await this.NavigateToViewAsync<LSMTCarouselViewModel, ILSMTCarouselViewModel>()));

        public ICommand CellsControlButtonCommand =>
            this.cellsControlButtonCommand
            ??
            (this.cellsControlButtonCommand = new DelegateCommand(
                async () => await this.NavigateToViewAsync<CellsControlViewModel, ICellsControlViewModel>()));

        public ICommand CellsPanelControlButtonCommand =>
            this.cellsPanelControlButtonCommand
            ??
            (this.cellsPanelControlButtonCommand = new DelegateCommand(
                async () => await this.NavigateToViewAsync<CellsPanelsControlViewModel, ICellsPanelsControlViewModel>()));

        public ICommand CellsSideControlButtonCommand =>
            this.cellsSideControlButtonCommand
            ??
            (this.cellsSideControlButtonCommand = new DelegateCommand(
                async () => await this.NavigateToViewAsync<CellsSideControlViewModel, ICellsSideControlViewModel>()));

        public ICommand DrawerLoadingUnloadingTestButtonCommand =>
            this.drawerLoadingUnloadingTestButtonCommand
            ??
            (this.drawerLoadingUnloadingTestButtonCommand = new DelegateCommand(
                async () => await this.NavigateToViewAsync<DrawerLoadingUnloadingTestViewModel, IDrawerLoadingUnloadingTestViewModel>()));

        public ICommand ErrorButtonCommand =>
            this.errorButtonCommand
            ??
            (this.errorButtonCommand = new DelegateCommand(() =>
        {
            // TODO implement error system
        }));

        public ICommand Gate1HeightControlNavigationButtonCommand =>
            this.gate1HeightControlNavigationButtonCommand
            ??
            (this.gate1HeightControlNavigationButtonCommand = new DelegateCommand(
                async () => await this.NavigateToViewAsync<Shutter1HeightControlViewModel, IShutter1HeightControlViewModel>()));

        public ICommand Gate2HeightControlNavigationButtonCommand =>
            this.gate2HeightControlNavigationButtonCommand
            ??
            (this.gate2HeightControlNavigationButtonCommand = new DelegateCommand(
                async () => await this.NavigateToViewAsync<Shutter2HeightControlViewModel, IShutter2HeightControlViewModel>()));

        public ICommand Gate3HeightControlNavigationButtonCommand =>
            this.gate3HeightControlNavigationButtonCommand
            ??
            (this.gate3HeightControlNavigationButtonCommand = new DelegateCommand(
                async () => await this.NavigateToViewAsync<Shutter3HeightControlViewModel, IShutter3HeightControlViewModel>()));

        public ICommand Gates1ControlNavigationButtonCommand =>
            this.gates1ControlNavigationButtonCommand
            ??
            (this.gates1ControlNavigationButtonCommand = new DelegateCommand(
                async () => await this.NavigateToViewAsync<Shutter1ControlViewModel, IShutter1ControlViewModel>()));

        public ICommand Gates2ControlNavigationButtonCommand =>
            this.gates2ControlNavigationButtonCommand
            ??
            (this.gates2ControlNavigationButtonCommand = new DelegateCommand(
                async () => await this.NavigateToViewAsync<Shutter2ControlViewModel, IShutter2ControlViewModel>()));

        public ICommand Gates3ControlNavigationButtonCommand =>
            this.gates3ControlNavigationButtonCommand
            ??
            (this.gates3ControlNavigationButtonCommand = new DelegateCommand(
                async () => await this.NavigateToViewAsync<Shutter3ControlViewModel, IShutter3ControlViewModel>()));

        public ICommand InstallationStateButtonCommand =>
            this.installationStateButtonCommand
            ??
            (this.installationStateButtonCommand = new DelegateCommand(
                async () => await this.NavigateToViewAsync<InstallationStateViewModel, IInstallationStateViewModel>()));

        public ICommand LoadFirstDrawerButtonCommand =>
            this.loadFirstDrawerButtonCommand
            ??
            (this.loadFirstDrawerButtonCommand = new DelegateCommand(
                async () => await this.NavigateToViewAsync<LoadFirstDrawerViewModel, ILoadFirstDrawerViewModel>()));

        public ICommand LoadingDrawersButtonCommand =>
            this.loadingDrawersButtonCommand
            ??
            (this.loadingDrawersButtonCommand = new DelegateCommand(
                async () => await this.NavigateToViewAsync<LoadingDrawersViewModel, ILoadingDrawersViewModel>()));

        public ICommand LowSpeedMovementsTestButtonCommand =>
            this.lowSpeedMovementsTestButtonCommand
            ??
            (this.lowSpeedMovementsTestButtonCommand = new DelegateCommand(
                async () => await this.NavigateToViewAsync<LSMTMainViewModel, ILSMTMainViewModel>()));

        public ICommand LSMTGateEngineButtonCommand =>
            this.lsmtGateEngineButtonCommand
            ??
            (this.lsmtGateEngineButtonCommand = new DelegateCommand(
                async () => await this.NavigateToViewAsync<LSMTShutterEngineViewModel, ILSMTShutterEngineViewModel>()));

        public ICommand LSMTHorizontalEngineButtonCommand =>
            this.lsmtHorizontalEngineButtonCommand
            ??
            (this.lsmtHorizontalEngineButtonCommand = new DelegateCommand(
                async () => await this.NavigateToViewAsync<LSMTHorizontalEngineViewModel, ILSMTHorizontalEngineViewModel>()));

        public ICommand LSMTVerticalEngineButtonCommand =>
            this.lsmtVerticalEngineButtonCommand
            ??
            (this.lsmtVerticalEngineButtonCommand = new DelegateCommand(
                async () => await this.NavigateToViewAsync<LSMTVerticalEngineViewModel, ILSMTVerticalEngineViewModel>()));

        public ICommand ManualDrawerStoreRecallButtonCommand =>
            this.manualDrawerStoreRecallButtonCommand
            ??
            (this.manualDrawerStoreRecallButtonCommand = new DelegateCommand(
                async () => await this.NavigateToViewAsync<DrawerStoreRecallViewModel, IDrawerStoreRecallViewModel>()));

        public ICommand ResolutionCalibrationVerticalAxisButtonCommand =>
            this.resolutionCalibrationVerticalAxisButtonCommand
            ??
            (this.resolutionCalibrationVerticalAxisButtonCommand = new DelegateCommand(
                async () => await this.NavigateToViewAsync<ResolutionCalibrationVerticalAxisViewModel, IResolutionCalibrationVerticalAxisViewModel>()));

        public ICommand SaveRestoreConfigButtonCommand =>
            this.saveRestoreConfigButtonCommand
            ??
            (this.saveRestoreConfigButtonCommand = new DelegateCommand(
                async () => await this.NavigateToViewAsync<SaveRestoreConfigViewModel, ISaveRestoreConfigViewModel>()));

        public ICommand SsBaysButtonCommand =>
            this.ssBaysButtonCommand
            ??
            (this.ssBaysButtonCommand = new DelegateCommand(
                async () => await this.NavigateToViewAsync<SSBaysViewModel, ISSBaysViewModel>()));

        public ICommand SsCradleButtonCommand =>
            this.ssCradleButtonCommand
            ??
            (this.ssCradleButtonCommand = new DelegateCommand(
                async () => await this.NavigateToViewAsync<SSCradleViewModel, ISSCradleViewModel>()));

        public ICommand SsGateButtonCommand =>
            this.ssGateButtonCommand
            ??
            (this.ssGateButtonCommand = new DelegateCommand(async () => await this.NavigateToViewAsync<SSShutterViewModel, ISSShutterViewModel>()));

        public ICommand SsNavigationButtonsButtonCommand =>
            this.ssNavigationButtonsButtonCommand
            ??
            (this.ssNavigationButtonsButtonCommand = new DelegateCommand(
                async () =>
                {
                    this.isNavigationButtonRegionExpanded = Visibility.Collapsed;
                    await this.NavigateToViewAsync<SSMainViewModel, ISSMainViewModel>();
                    this.container.Resolve<ISSMainViewModel>().SSNavigationRegionCurrentViewModel =
                        this.container.Resolve<ISSNavigationButtonsViewModel>() as SSNavigationButtonsViewModel;
                }));

        public ICommand SsVariousInputsButtonCommand =>
            this.ssVariousInputsButtonCommand
            ??
            (this.ssVariousInputsButtonCommand = new DelegateCommand(
                async () => await this.NavigateToViewAsync<SSVariousInputsViewModel, ISSVariousInputsViewModel>()));

        public ICommand SsVerticalAxisButtonCommand =>
            this.ssVerticalAxisButtonCommand
            ??
            (this.ssVerticalAxisButtonCommand = new DelegateCommand(
                async () => await this.NavigateToViewAsync<SSVerticalAxisViewModel, ISSVerticalAxisViewModel>()));

        public ICommand VerticalAxisCalibrationButtonCommand =>
            this.verticalAxisCalibrationButtonCommand
            ??
            (this.verticalAxisCalibrationButtonCommand = new DelegateCommand(
                async () => await this.NavigateToViewAsync<VerticalAxisCalibrationViewModel, IVerticalAxisCalibrationViewModel>()));

        public ICommand VerticalOffsetCalibrationButtonCommand =>
            this.verticalOffsetCalibrationButtonCommand
            ??
            (this.verticalOffsetCalibrationButtonCommand = new DelegateCommand(
                async () => await this.NavigateToViewAsync<VerticalOffsetCalibrationViewModel, IVerticalOffsetCalibrationViewModel>()));

        public ICommand WeightControlButtonCommand =>
            this.weightControlButtonCommand
            ??
            (this.weightControlButtonCommand = new DelegateCommand(
                async () => await this.NavigateToViewAsync<WeightControlViewModel, IWeightControlViewModel>()));

        public ICommand DddddddButtonCommand =>
            this.dddddddButtonCommand
            ??
            (this.dddddddButtonCommand = new DelegateCommand(
                async () => await this.NavigateToViewAsync<DiagnosticDetailsViewModel, IDiagnosticDetailsViewModel>()));

        #endregion

        #region Methods

        private async Task NavigateToViewAsync<T, I>()
            where T : BindableBase, I
            where I : IViewModel
        {
            this.eventAggregator.GetEvent<InstallationApp_Event>().Publish(
                new InstallationApp_EventMessage(InstallationApp_EventMessageType.EnterView));

            var desiredViewModel = this.container.Resolve<I>() as T;
            await desiredViewModel.OnEnterViewAsync();

            this.ContentRegionCurrentViewModel = desiredViewModel;
        }

        #endregion
    }
}
