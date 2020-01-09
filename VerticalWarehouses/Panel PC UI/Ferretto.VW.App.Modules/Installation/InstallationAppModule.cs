using Ferretto.VW.App.Controls.Interfaces;
using Ferretto.VW.App.Installation.Views;
using Ferretto.VW.App.Modules.Installation.Views;
using Ferretto.VW.MAS.AutomationService.Contracts.Hubs;
using Prism.Ioc;
using Prism.Modularity;
using Unity;

namespace Ferretto.VW.App.Modules.Installation
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Major Code Smell",
        "S1200:Classes should not be coupled to too many other classes (Single Responsibility Principle)",
        Justification = "This is a container initialization class, so it is ok to be coupled to many types.")]
    [Module(ModuleName = nameof(Utils.Modules.Installation), OnDemand = true)]
    [ModuleDependency(nameof(Utils.Modules.Errors))]
    public class InstallationAppModule : IModule
    {
        #region Fields

        private readonly IUnityContainer container;

        #endregion

        #region Constructors

        public InstallationAppModule(IUnityContainer container)
        {
            this.container = container;
        }

        #endregion

        #region Methods

        public void OnInitialized(IContainerProvider containerProvider)
        {
            containerProvider.UseMachineAutomationHubs();
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterForNavigation<BayCheckView>();
            containerRegistry.RegisterForNavigation<BayHeightCheckView>();
            containerRegistry.RegisterForNavigation<DepositAndPickUpTestView>();

            containerRegistry.RegisterForNavigation<BaysSensorsView>();
            containerRegistry.RegisterForNavigation<VerticalAxisSensorsView>();
            containerRegistry.RegisterForNavigation<OtherSensorsView>();

            containerRegistry.RegisterForNavigation<MovementsView>();

            containerRegistry.RegisterForNavigation<SemiAutoMovementsView>();
            containerRegistry.RegisterForNavigation<ManualMovementsView>();
            containerRegistry.RegisterForNavigation<CarouselManualMovementsView>();
            containerRegistry.RegisterForNavigation<HorizontalEngineManualMovementsView>();
            containerRegistry.RegisterForNavigation<ManualMovementsNavigationView>();
            containerRegistry.RegisterForNavigation<ShutterEngineManualMovementsView>();
            containerRegistry.RegisterForNavigation<ElevatorManualMovementsView>();
            containerRegistry.RegisterForNavigation<ExternalBayManualMovementsView>();

            containerRegistry.RegisterForNavigation<ElevatorWeightAnalysisView>();
            containerRegistry.RegisterForNavigation<ElevatorWeightCheckStep1View>();
            containerRegistry.RegisterForNavigation<ElevatorWeightCheckStep2View>();
            containerRegistry.RegisterForNavigation<VerticalOffsetCalibrationView>();
            containerRegistry.RegisterForNavigation<VerticalOriginCalibrationView>();
            containerRegistry.RegisterForNavigation<VerticalResolutionCalibrationView>();
            containerRegistry.RegisterForNavigation<LoadFirstDrawerView>();
            containerRegistry.RegisterForNavigation<LoadingUnitFromBayToCellView>();
            containerRegistry.RegisterForNavigation<LoadingUnitFromBayToBayView>();
            containerRegistry.RegisterForNavigation<LoadingUnitFromCellToBayView>();
            containerRegistry.RegisterForNavigation<LoadingUnitFromCellToCellView>();
            containerRegistry.RegisterForNavigation<SaveRestoreConfigView>();
            containerRegistry.RegisterForNavigation<DrawerLoadingUnloadingTestView>();
            containerRegistry.RegisterForNavigation<CellsSideControlView>();

            containerRegistry.RegisterForNavigation<ProfileHeightCheckStep1View>();
            containerRegistry.RegisterForNavigation<ProfileHeightCheckStep2View>();
            containerRegistry.RegisterForNavigation<ProfileHeightCheckStep3View>();
            containerRegistry.RegisterForNavigation<ProfileHeightCheckStep4View>();
            containerRegistry.RegisterForNavigation<ProfileHeightCheckStep5View>();
            containerRegistry.RegisterForNavigation<ProfileHeightCheckStep6View>();

            containerRegistry.RegisterForNavigation<ShutterEnduranceTestView>();

            containerRegistry.RegisterForNavigation<BeltBurnishingView>();

            containerRegistry.RegisterForNavigation<CellsHeightCheckStep1View>();
            containerRegistry.RegisterForNavigation<CellsHeightCheckStep2View>();
            containerRegistry.RegisterForNavigation<CellPanelsCheckView>();

            containerRegistry.RegisterForNavigation<ParametersView>();
            containerRegistry.RegisterForNavigation<ParametersExportView>();
            containerRegistry.RegisterForNavigation<ParametersImportStep1View>();
            containerRegistry.RegisterForNavigation<ParametersImportStep2View>();

            containerRegistry.RegisterForNavigation<CellsLoadingUnitsMenuView>();
            containerRegistry.RegisterForNavigation<CellsView>();
            containerRegistry.RegisterForNavigation<LoadingUnitsView>();
            containerRegistry.RegisterForNavigation<UpdateView>();
            containerRegistry.RegisterForNavigation<UsersView>();
            containerRegistry.RegisterForNavigation<ParameterInverterView>();
            containerRegistry.RegisterForNavigation<ComunicationWmsView>();
            containerRegistry.RegisterForNavigation<DateTimeView>();

            containerRegistry.Register<INavigableView, DevicesView>(nameof(DevicesView));
        }

        #endregion
    }
}
