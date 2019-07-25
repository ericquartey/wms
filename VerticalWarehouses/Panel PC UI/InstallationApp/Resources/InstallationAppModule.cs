using System.Configuration;
using System.Windows;
using Ferretto.VW.App.Controls.Controls;
using Ferretto.VW.App.Controls.Interfaces;
using Ferretto.VW.App.Installation.HelpWindows;
using Ferretto.VW.App.Installation.Interfaces;
using Ferretto.VW.App.Installation.ViewsAndViewModels;
using Ferretto.VW.App.Installation.ViewsAndViewModels.LowSpeedMovements;
using Ferretto.VW.App.Installation.ViewsAndViewModels.SensorsState;
using Ferretto.VW.App.Installation.ViewsAndViewModels.ShuttersControl;
using Ferretto.VW.App.Installation.ViewsAndViewModels.ShuttersHeightControl;
using Ferretto.VW.App.Installation.ViewsAndViewModels.SingleViews;
using Ferretto.VW.App.Services;
using Ferretto.VW.App.Services.Interfaces;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.MAS.AutomationService.Contracts.Hubs;
using Ferretto.VW.Utils.Interfaces;
using Prism.Events;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Mvvm;
using Unity;

namespace Ferretto.VW.App.Installation.Resources
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Major Code Smell",
        "S1200:Classes should not be coupled to too many other classes (Single Responsibility Principle)",
        Justification = "This is a container initialization class, so it is ok to be coupled to many types.")]
    public class InstallationAppModule : IModule
    {
        #region Fields

        private readonly string automationServiceUrl = ConfigurationManager.AppSettings.Get("AutomationServiceUrl");

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
            BindViewModelToView<IMainWindowViewModel, MainWindow>(containerProvider);

            var mainWindow = (MainWindow)containerProvider.Resolve<IMainWindow>();
            var mainWindowViewModel = containerProvider.Resolve<IMainWindowViewModel>();
            mainWindowViewModel.LoggedUser = "Installer";
            mainWindow.DataContext = mainWindowViewModel;

            var mainWindowProperty = Application.Current.GetType().GetProperty("InstallationAppMainWindowInstance");
            mainWindowProperty.SetValue(Application.Current, mainWindow);
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            var homingService = new HomingService(this.automationServiceUrl);
            var positioningService = new PositioningService(this.automationServiceUrl);
            var beltBurnishingService = new BeltBurnishingService(this.automationServiceUrl);
            var shutterService = new ShutterService(this.automationServiceUrl);
            var resolutionCalibrationService = new ResolutionCalibrationService(this.automationServiceUrl);
            var offsetCalibrationService = new OffsetCalibrationService(this.automationServiceUrl);
            var installationStatusService = new InstallationStatusService(this.automationServiceUrl);
            var updateSensorsService = new UpdateSensorsService(this.automationServiceUrl);

            var testService = new TestService(this.automationServiceUrl);
            var helpMainWindowInstance = new HelpMainWindow(this.container.Resolve<IEventAggregator>());

            var beltBurnishingVMInstance = new BeltBurnishingViewModel(this.container.Resolve<IEventAggregator>());
            var cellsControlVMInstance = new CellsControlViewModel(this.container.Resolve<IEventAggregator>());
            var cellsPanelsControlVMInstance = new CellsPanelsControlViewModel(this.container.Resolve<IEventAggregator>());
            var shutter1ControlVMInstance = new Shutter1ControlViewModel(this.container.Resolve<IEventAggregator>());
            var shutter2ControlVMInstance = new Shutter2ControlViewModel(this.container.Resolve<IEventAggregator>());
            var shutter3ControlVMInstance = new Shutter3ControlViewModel(this.container.Resolve<IEventAggregator>());
            var shutter1HeightControlVMInstance = new Shutter1HeightControlViewModel(this.container.Resolve<IEventAggregator>());
            var shutter2HeightControlVMInstance = new Shutter2HeightControlViewModel(this.container.Resolve<IEventAggregator>());
            var shutter3HeightControlVMInstance = new Shutter3HeightControlViewModel(this.container.Resolve<IEventAggregator>());
            var installationStateVMInstance = new InstallationStateViewModel(this.container.Resolve<IEventAggregator>());
            var lSMTShutterEngineVMInstance = new LSMTShutterEngineViewModel(this.container.Resolve<IEventAggregator>());
            var lSMTHorizontalEngineVMInstance = new LSMTHorizontalEngineViewModel(this.container.Resolve<IEventAggregator>());
            var lSMTNavigationButtonsVMInstance = new LSMTNavigationButtonsViewModel(this.container.Resolve<IEventAggregator>());
            var lSMTMainVMInstance = new LSMTMainViewModel(this.container.Resolve<IEventAggregator>());
            var lSMTVerticalEngineVMInstance = new LSMTVerticalEngineViewModel(this.container.Resolve<IEventAggregator>());
            var mainWindowNavigationButtonsVMInstance = new MainWindowNavigationButtonsViewModel(this.container.Resolve<IEventAggregator>());
            var resolutionCalibrationVerticalAxisVMInstance = new ResolutionCalibrationVerticalAxisViewModel(this.container.Resolve<IEventAggregator>());
            var sSBaysVMInstance = new SSBaysViewModel(this.container.Resolve<IEventAggregator>());
            var sSCradleVMInstance = new SSCradleViewModel(this.container.Resolve<IEventAggregator>());
            var sSShutterVMInstance = new SSShutterViewModel(this.container.Resolve<IEventAggregator>());
            var sSMainVMInstance = new SSMainViewModel(this.container.Resolve<IEventAggregator>());
            var sSNavigationButtonsVMInstance = new SSNavigationButtonsViewModel(this.container.Resolve<IEventAggregator>());
            var sSVariousInputsVMInstance = new SSVariousInputsViewModel(this.container.Resolve<IEventAggregator>());
            var sSVerticalAxisVMInstance = new SSVerticalAxisViewModel(this.container.Resolve<IEventAggregator>());
            var verticalAxisCalibrationVMInstance = new VerticalAxisCalibrationViewModel(this.container.Resolve<IEventAggregator>());
            var verticalOffsetCalibrationVMInstance = new VerticalOffsetCalibrationViewModel(this.container.Resolve<IEventAggregator>());
            var weightControlVMInstance = new WeightControlViewModel(this.container.Resolve<IEventAggregator>());
            var bayControlVMInstance = new BayControlViewModel();
            var loadFirstDrawerVMInstance = new LoadFirstDrawerViewModel();
            var loadingDrawersVMInstance = new LoadingDrawersViewModel();
            var saveRestoreConfigVMInstance = new SaveRestoreConfigViewModel();
            var cellsSideControlVMInstance = new CellsSideControlViewModel();
            var drawerLoadingUnloadingTestVMInstance = new DrawerLoadingUnloadingTestViewModel();
            var lSMTCarouselVMInstance = new LSMTCarouselViewModel(this.container.Resolve<IEventAggregator>());

            this.container.RegisterSingleton<IMainWindow, MainWindow>();

            this.container.RegisterInstance<IHelpMainWindow>(helpMainWindowInstance);

            this.container.RegisterInstance<IHomingService>(homingService);
            this.container.RegisterInstance<IPositioningService>(positioningService);
            this.container.RegisterInstance<IBeltBurnishingService>(beltBurnishingService);
            this.container.RegisterInstance<IShutterService>(shutterService);
            this.container.RegisterInstance<IResolutionCalibrationService>(resolutionCalibrationService);
            this.container.RegisterInstance<IOffsetCalibrationService>(offsetCalibrationService);
            this.container.RegisterInstance<IInstallationStatusService>(installationStatusService);
            this.container.RegisterInstance<IUpdateSensorsService>(updateSensorsService);
            this.container.RegisterInstance<IOffsetCalibrationService>(offsetCalibrationService);

            this.container.RegisterInstance<ITestService>(testService);

            this.container.RegisterInstance<IStatusMessageService>(new StatusMessageService());

            this.container.RegisterSingleton<IMainWindowViewModel, MainWindowViewModel>();
            this.container.RegisterInstance<INotificationService>(
                new NotificationService(
                    this.container.Resolve<IEventAggregator>(),
                    this.container.Resolve<IOperatorHubClient>(),
                    this.container.Resolve<IInstallationHubClient>()));

            this.RegisterInstanceAndBindViewToViewModel<IBeltBurnishingViewModel, BeltBurnishingViewModel>(beltBurnishingVMInstance);
            this.RegisterInstanceAndBindViewToViewModel<ICellsControlViewModel, CellsControlViewModel>(cellsControlVMInstance);
            this.RegisterInstanceAndBindViewToViewModel<ICellsPanelsControlViewModel, CellsPanelsControlViewModel>(cellsPanelsControlVMInstance);
            this.RegisterInstanceAndBindViewToViewModel<IShutter1ControlViewModel, Shutter1ControlViewModel>(shutter1ControlVMInstance);
            this.RegisterInstanceAndBindViewToViewModel<IShutter2ControlViewModel, Shutter2ControlViewModel>(shutter2ControlVMInstance);
            this.RegisterInstanceAndBindViewToViewModel<IShutter3ControlViewModel, Shutter3ControlViewModel>(shutter3ControlVMInstance);
            this.RegisterInstanceAndBindViewToViewModel<IShutter1HeightControlViewModel, Shutter1HeightControlViewModel>(shutter1HeightControlVMInstance);
            this.RegisterInstanceAndBindViewToViewModel<IShutter2HeightControlViewModel, Shutter2HeightControlViewModel>(shutter2HeightControlVMInstance);
            this.RegisterInstanceAndBindViewToViewModel<IShutter3HeightControlViewModel, Shutter3HeightControlViewModel>(shutter3HeightControlVMInstance);
            this.container.RegisterSingleton<IIdleViewModel, IdleViewModel>();
            this.RegisterInstanceAndBindViewToViewModel<IInstallationStateViewModel, InstallationStateViewModel>(installationStateVMInstance);
            this.RegisterInstanceAndBindViewToViewModel<ILSMTShutterEngineViewModel, LSMTShutterEngineViewModel>(lSMTShutterEngineVMInstance);
            this.RegisterInstanceAndBindViewToViewModel<ILSMTHorizontalEngineViewModel, LSMTHorizontalEngineViewModel>(lSMTHorizontalEngineVMInstance);
            this.RegisterInstanceAndBindViewToViewModel<ILSMTNavigationButtonsViewModel, LSMTNavigationButtonsViewModel>(lSMTNavigationButtonsVMInstance);
            this.RegisterInstanceAndBindViewToViewModel<ILSMTMainViewModel, LSMTMainViewModel>(lSMTMainVMInstance);
            this.RegisterInstanceAndBindViewToViewModel<ILSMTVerticalEngineViewModel, LSMTVerticalEngineViewModel>(lSMTVerticalEngineVMInstance);
            this.container.RegisterSingleton<IFooterViewModel, FooterViewModel>();
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

            resolutionCalibrationVerticalAxisVMInstance.InitializeViewModel(this.container);
            mainWindowNavigationButtonsVMInstance.InitializeViewModelAsync(this.container);

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

        private void RegisterInstanceAndBindViewToViewModel<I, T>(T instance)
            where T : BindableBase, I
            where I : IViewModel
        {
            this.container.RegisterInstance<I>(instance);
        }

        private void RegisterTypeAndBindViewToViewModel<I, T>()
            where T : BindableBase, I
            where I : IViewModel
        {
            this.container.RegisterType<I, T>();
        }

        #endregion
    }
}
