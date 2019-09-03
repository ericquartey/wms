using Ferretto.VW.App.Installation.Views;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Mvvm;
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

        public static void BindViewModelToView<TViewModel, TView>(IContainerProvider containerProvider)
        {
            ViewModelLocationProvider.Register(
                typeof(TView).ToString(),
                () => containerProvider.Resolve<TViewModel>());
        }

        public void OnInitialized(IContainerProvider containerProvider)
        {
            containerProvider.UseMachineAutomationHubs();
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterForNavigation<InstallatorMenuView>();

            containerRegistry.RegisterForNavigation<BayHeightCheckView>();

            containerRegistry.RegisterForNavigation<BaysSensorsView>();
            containerRegistry.RegisterForNavigation<ShutterSensorsView>();
            containerRegistry.RegisterForNavigation<VerticalAxisSensorsView>();
            containerRegistry.RegisterForNavigation<OtherSensorsView>();

            containerRegistry.RegisterForNavigation<CarouselManualMovementsView>();
            containerRegistry.RegisterForNavigation<HorizontalAxisManualMovementsView>();
            containerRegistry.RegisterForNavigation<ManualMovementsNavigationView>();
            containerRegistry.RegisterForNavigation<ShutterEngineManualMovementsView>();
            containerRegistry.RegisterForNavigation<VerticalEngineManualMovementsView>();
            containerRegistry.RegisterForNavigation<ExternalBayManualMovementsView>();

            #endregion

            #region Elevator Views

            containerRegistry.RegisterForNavigation<ElevatorWeightCheckStep1View>();
            containerRegistry.RegisterForNavigation<ElevatorWeightCheckStep2View>();
            containerRegistry.RegisterForNavigation<VerticalOffsetCalibrationStep1View>();
            containerRegistry.RegisterForNavigation<VerticalOffsetCalibrationStep2View>();
            containerRegistry.RegisterForNavigation<VerticalOriginCalibrationView>();
            containerRegistry.RegisterForNavigation<VerticalResolutionCalibrationStep1View>();
            containerRegistry.RegisterForNavigation<VerticalResolutionCalibrationStep2View>();
            containerRegistry.RegisterForNavigation<VerticalResolutionCalibrationStep3View>();

            containerRegistry.RegisterForNavigation<ShutterEnduranceTestView>();

            containerRegistry.RegisterForNavigation<BeltBurnishingView>();

            containerRegistry.RegisterForNavigation<CellsHeightCheckStep1View>();
            containerRegistry.RegisterForNavigation<CellsHeightCheckStep2View>();
            containerRegistry.RegisterForNavigation<CellPanelsCheckView>();
        }

        #endregion
    }
}
