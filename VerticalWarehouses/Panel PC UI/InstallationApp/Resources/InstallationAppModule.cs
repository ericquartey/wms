using System.Configuration;
using Ferretto.VW.CustomControls.Controls;
using Ferretto.VW.CustomControls.Interfaces;
using Ferretto.VW.InstallationApp.Interfaces;
using Ferretto.VW.InstallationApp.ServiceUtilities;
using Ferretto.VW.InstallationApp.ServiceUtilities.Interfaces;
using Ferretto.VW.MAS_AutomationService.Contracts;
using Ferretto.VW.Utils.Interfaces;
using Prism.Events;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Mvvm;
using Unity;

namespace Ferretto.VW.InstallationApp
{
    public class InstallationAppModule : IModule
    {
        #region Fields

        private readonly string automationServiceUrl = ConfigurationManager.AppSettings.Get("AutomationServiceUrl");

        private readonly IUnityContainer container;

        private readonly string installationHubEndpoint = ConfigurationManager.AppSettings.Get("InstallationHubEndpoint");

        #endregion

        #region Constructors

        public InstallationAppModule(IUnityContainer container)
        {
            this.container = container;

            var homingService = new HomingService(this.automationServiceUrl);
            var positioningService = new PositioningService(this.automationServiceUrl);
            var beltBurnishingService = new BeltBurnishingService(this.automationServiceUrl);
            var shutterService = new ShutterService(this.automationServiceUrl);
            var resolutionCalibrationService = new ResolutionCalibrationService(this.automationServiceUrl);
            var installationStatusService = new InstallationStatusService(this.automationServiceUrl);
            var updateSensorsService = new UpdateSensorsService(this.automationServiceUrl);

            var testService = new TestService(this.automationServiceUrl);
            var mainWindowInstance = new MainWindow(container.Resolve<IEventAggregator>());
            var helpMainWindowInstance = new HelpMainWindow(container.Resolve<IEventAggregator>());
            var installationHubClientInstance = new InstallationHubClient("http://localhost:5000/", "installation-endpoint");

            var beltBurnishingVMInstance = new BeltBurnishingViewModel(container.Resolve<IEventAggregator>());
            var cellsControlVMInstance = new CellsControlViewModel(container.Resolve<IEventAggregator>());
            var cellsPanelsControlVMInstance = new CellsPanelsControlViewModel(container.Resolve<IEventAggregator>());
            var shutter1ControlVMInstance = new Shutter1ControlViewModel(container.Resolve<IEventAggregator>());
            var shutter2ControlVMInstance = new Shutter2ControlViewModel(container.Resolve<IEventAggregator>());
            var shutter3ControlVMInstance = new Shutter3ControlViewModel(container.Resolve<IEventAggregator>());
            var shutter1HeightControlVMInstance = new Shutter1HeightControlViewModel(container.Resolve<IEventAggregator>());
            var shutter2HeightControlVMInstance = new Shutter2HeightControlViewModel(container.Resolve<IEventAggregator>());
            var shutter3HeightControlVMInstance = new Shutter3HeightControlViewModel(container.Resolve<IEventAggregator>());
            var idleVMInstance = new IdleViewModel(container.Resolve<IEventAggregator>());
            var installationStateVMInstance = new InstallationStateViewModel(container.Resolve<IEventAggregator>());
            var lSMTShutterEngineVMInstance = new LSMTShutterEngineViewModel(container.Resolve<IEventAggregator>());
            var lSMTHorizontalEngineVMInstance = new LSMTHorizontalEngineViewModel(container.Resolve<IEventAggregator>());
            var lSMTNavigationButtonsVMInstance = new LSMTNavigationButtonsViewModel(container.Resolve<IEventAggregator>());
            var lSMTMainVMInstance = new LSMTMainViewModel(container.Resolve<IEventAggregator>());
            var lSMTVerticalEngineVMInstance = new LSMTVerticalEngineViewModel(container.Resolve<IEventAggregator>());
            var mainWindowBackToIAPPButtonVMInstance = new MainWindowBackToIAPPButtonViewModel(container.Resolve<IEventAggregator>());
            var mainWindowNavigationButtonsVMInstance = new MainWindowNavigationButtonsViewModel(container.Resolve<IEventAggregator>());
            var resolutionCalibrationVerticalAxisVMInstance = new ResolutionCalibrationVerticalAxisViewModel(container.Resolve<IEventAggregator>());
            var sSBaysVMInstance = new SSBaysViewModel(container.Resolve<IEventAggregator>());
            var sSCradleVMInstance = new SSCradleViewModel(container.Resolve<IEventAggregator>());
            var sSShutterVMInstance = new SSShutterViewModel(container.Resolve<IEventAggregator>());
            var sSMainVMInstance = new SSMainViewModel(container.Resolve<IEventAggregator>());
            var sSNavigationButtonsVMInstance = new SSNavigationButtonsViewModel(container.Resolve<IEventAggregator>());
            var sSVariousInputsVMInstance = new SSVariousInputsViewModel(container.Resolve<IEventAggregator>());
            var sSVerticalAxisVMInstance = new SSVerticalAxisViewModel(container.Resolve<IEventAggregator>());
            var verticalAxisCalibrationVMInstance = new VerticalAxisCalibrationViewModel(container.Resolve<IEventAggregator>());
            var verticalOffsetCalibrationVMInstance = new VerticalOffsetCalibrationViewModel(container.Resolve<IEventAggregator>());
            var weightControlVMInstance = new WeightControlViewModel(container.Resolve<IEventAggregator>());
            var bayControlVMInstance = new BayControlViewModel();
            var loadFirstDrawerVMInstance = new LoadFirstDrawerViewModel();
            var loadingDrawersVMInstance = new LoadingDrawersViewModel();
            var saveRestoreConfigVMInstance = new SaveRestoreConfigViewModel();
            var cellsSideControlVMInstance = new CellsSideControlViewModel();
            var drawerLoadingUnloadingTestVMInstance = new DrawerLoadingUnloadingTestViewModel();
            var lSMTCarouselVMInstance = new LSMTCarouselViewModel(container.Resolve<IEventAggregator>());

            this.container.RegisterInstance<IMainWindow>(mainWindowInstance);
            this.container.RegisterInstance<IInstallationHubClient>(installationHubClientInstance);
            this.container.RegisterInstance<IHelpMainWindow>(helpMainWindowInstance);

            this.container.RegisterInstance<IHomingService>(homingService);
            this.container.RegisterInstance<IPositioningService>(positioningService);
            this.container.RegisterInstance<IBeltBurnishingService>(beltBurnishingService);
            this.container.RegisterInstance<IShutterService>(shutterService);
            this.container.RegisterInstance<IResolutionCalibrationService>(resolutionCalibrationService);
            this.container.RegisterInstance<IInstallationStatusService>(installationStatusService);
            this.container.RegisterInstance<IUpdateSensorsService>(updateSensorsService);

            this.container.RegisterInstance<ITestService>(testService);

            var mainWindowVMInstance = new MainWindowViewModel(
              container.Resolve<IEventAggregator>(),
              container.Resolve<IInstallationHubClient>());
            this.container.RegisterInstance<IMainWindowViewModel>(mainWindowVMInstance);

            this.RegisterInstanceAndBindViewToViewModel<IBeltBurnishingViewModel, BeltBurnishingViewModel>(beltBurnishingVMInstance);
            this.RegisterInstanceAndBindViewToViewModel<ICellsControlViewModel, CellsControlViewModel>(cellsControlVMInstance);
            this.RegisterInstanceAndBindViewToViewModel<ICellsPanelsControlViewModel, CellsPanelsControlViewModel>(cellsPanelsControlVMInstance);
            this.RegisterInstanceAndBindViewToViewModel<IShutter1ControlViewModel, Shutter1ControlViewModel>(shutter1ControlVMInstance);
            this.RegisterInstanceAndBindViewToViewModel<IShutter2ControlViewModel, Shutter2ControlViewModel>(shutter2ControlVMInstance);
            this.RegisterInstanceAndBindViewToViewModel<IShutter3ControlViewModel, Shutter3ControlViewModel>(shutter3ControlVMInstance);
            this.RegisterInstanceAndBindViewToViewModel<IShutter1HeightControlViewModel, Shutter1HeightControlViewModel>(shutter1HeightControlVMInstance);
            this.RegisterInstanceAndBindViewToViewModel<IShutter2HeightControlViewModel, Shutter2HeightControlViewModel>(shutter2HeightControlVMInstance);
            this.RegisterInstanceAndBindViewToViewModel<IShutter3HeightControlViewModel, Shutter3HeightControlViewModel>(shutter3HeightControlVMInstance);
            this.RegisterInstanceAndBindViewToViewModel<IIdleViewModel, IdleViewModel>(idleVMInstance);
            this.RegisterInstanceAndBindViewToViewModel<IInstallationStateViewModel, InstallationStateViewModel>(installationStateVMInstance);
            this.RegisterInstanceAndBindViewToViewModel<ILSMTShutterEngineViewModel, LSMTShutterEngineViewModel>(lSMTShutterEngineVMInstance);
            this.RegisterInstanceAndBindViewToViewModel<ILSMTHorizontalEngineViewModel, LSMTHorizontalEngineViewModel>(lSMTHorizontalEngineVMInstance);
            this.RegisterInstanceAndBindViewToViewModel<ILSMTNavigationButtonsViewModel, LSMTNavigationButtonsViewModel>(lSMTNavigationButtonsVMInstance);
            this.RegisterInstanceAndBindViewToViewModel<ILSMTMainViewModel, LSMTMainViewModel>(lSMTMainVMInstance);
            this.RegisterInstanceAndBindViewToViewModel<ILSMTVerticalEngineViewModel, LSMTVerticalEngineViewModel>(lSMTVerticalEngineVMInstance);
            this.RegisterInstanceAndBindViewToViewModel<IMainWindowBackToIAPPButtonViewModel, MainWindowBackToIAPPButtonViewModel>(mainWindowBackToIAPPButtonVMInstance);
            this.RegisterInstanceAndBindViewToViewModel<IMainWindowNavigationButtonsViewModel, MainWindowNavigationButtonsViewModel>(mainWindowNavigationButtonsVMInstance);
            this.RegisterInstanceAndBindViewToViewModel<IResolutionCalibrationVerticalAxisViewModel, ResolutionCalibrationVerticalAxisViewModel>(resolutionCalibrationVerticalAxisVMInstance);
            this.RegisterInstanceAndBindViewToViewModel<ISSBaysViewModel, SSBaysViewModel>(sSBaysVMInstance);
            this.RegisterInstanceAndBindViewToViewModel<ISSCradleViewModel, SSCradleViewModel>(sSCradleVMInstance);
            this.RegisterInstanceAndBindViewToViewModel<ISSShutterViewModel, SSShutterViewModel>(sSShutterVMInstance);
            this.RegisterInstanceAndBindViewToViewModel<ISSMainViewModel, SSMainViewModel>(sSMainVMInstance);
            this.RegisterInstanceAndBindViewToViewModel<ISSNavigationButtonsViewModel, SSNavigationButtonsViewModel>(sSNavigationButtonsVMInstance);
            this.RegisterInstanceAndBindViewToViewModel<ISSVariousInputsViewModel, SSVariousInputsViewModel>(sSVariousInputsVMInstance);
            this.RegisterInstanceAndBindViewToViewModel<ISSVerticalAxisViewModel, SSVerticalAxisViewModel>(sSVerticalAxisVMInstance);
            this.RegisterInstanceAndBindViewToViewModel<IVerticalAxisCalibrationViewModel, VerticalAxisCalibrationViewModel>(verticalAxisCalibrationVMInstance);
            this.RegisterInstanceAndBindViewToViewModel<IVerticalOffsetCalibrationViewModel, VerticalOffsetCalibrationViewModel>(verticalOffsetCalibrationVMInstance);
            this.RegisterInstanceAndBindViewToViewModel<IWeightControlViewModel, WeightControlViewModel>(weightControlVMInstance);
            this.RegisterInstanceAndBindViewToViewModel<IBayControlViewModel, BayControlViewModel>(bayControlVMInstance);
            this.RegisterInstanceAndBindViewToViewModel<ILoadFirstDrawerViewModel, LoadFirstDrawerViewModel>(loadFirstDrawerVMInstance);
            this.RegisterInstanceAndBindViewToViewModel<ILoadingDrawersViewModel, LoadingDrawersViewModel>(loadingDrawersVMInstance);
            this.RegisterInstanceAndBindViewToViewModel<ISaveRestoreConfigViewModel, SaveRestoreConfigViewModel>(saveRestoreConfigVMInstance);
            this.RegisterInstanceAndBindViewToViewModel<ICellsSideControlViewModel, CellsSideControlViewModel>(cellsSideControlVMInstance);
            this.RegisterInstanceAndBindViewToViewModel<IDrawerLoadingUnloadingTestViewModel, DrawerLoadingUnloadingTestViewModel>(drawerLoadingUnloadingTestVMInstance);
            this.RegisterInstanceAndBindViewToViewModel<ILSMTCarouselViewModel, LSMTCarouselViewModel>(lSMTCarouselVMInstance);

            this.RegisterTypeAndBindViewToViewModel<ICustomShutterControlSensorsThreePositionsViewModel, CustomShutterControlSensorsThreePositionsViewModel>();
            this.RegisterTypeAndBindViewToViewModel<ICustomShutterControlSensorsTwoPositionsViewModel, CustomShutterControlSensorsTwoPositionsViewModel>();

            lSMTVerticalEngineVMInstance.InitializeViewModel(this.container);
            lSMTShutterEngineVMInstance.InitializeViewModel(this.container);
            lSMTHorizontalEngineVMInstance.InitializeViewModel(this.container);
            lSMTCarouselVMInstance.InitializeViewModel(this.container);
            lSMTNavigationButtonsVMInstance.InitializeViewModel(this.container);
            lSMTMainVMInstance.InitializeViewModel(this.container);

            mainWindowVMInstance.InitializeViewModel(this.container);
            mainWindowBackToIAPPButtonVMInstance.InitializeViewModel(this.container);
            resolutionCalibrationVerticalAxisVMInstance.InitializeViewModel(this.container);
            mainWindowNavigationButtonsVMInstance.InitializeViewModel(this.container);

            sSMainVMInstance.InitializeViewModel(this.container);
            sSNavigationButtonsVMInstance.InitializeViewModel(this.container);
            sSBaysVMInstance.InitializeViewModel(this.container);

            verticalOffsetCalibrationVMInstance.InitializeViewModel(this.container);
            installationStateVMInstance.InitializeViewModel(this.container);
            weightControlVMInstance.InitializeViewModel(this.container);
            verticalAxisCalibrationVMInstance.InitializeViewModel(this.container);
            shutter1ControlVMInstance.InitializeViewModel(this.container);
            beltBurnishingVMInstance.InitializeViewModel(this.container);

            sSVariousInputsVMInstance.InitializeViewModel(this.container);
            sSVerticalAxisVMInstance.InitializeViewModel(this.container);
            sSCradleVMInstance.InitializeViewModel(this.container);
        }

        #endregion

        #region Methods

        public void Initialize()
        {
            // HACK IModule interface requires the implementation of this method
        }

        public void OnInitialized(IContainerProvider containerProvider)
        {
            // HACK IModule interface requires the implementation of this method
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            // HACK IModule interface requires the implementation of this method
        }

        private void RegisterInstanceAndBindViewToViewModel<I, T>(T instance)
            where T : BindableBase, I
            where I : IViewModel
        {
            this.container.RegisterInstance<I>(instance);
            var view = typeof(T).ToString().Substring(0, typeof(T).ToString().Length - 9);
            //ViewModelLocationProvider.Register(view, () => this.container.Resolve<T>());
        }

        private void RegisterTypeAndBindViewToViewModel<I, T>()
            where T : BindableBase, I
            where I : IViewModel
        {
            this.container.RegisterType<I, T>();
            var view = typeof(T).ToString().Substring(0, typeof(T).ToString().Length - 9);
            //ViewModelLocationProvider.Register(view, () => this.container.Resolve<T>());
        }

        #endregion
    }
}
