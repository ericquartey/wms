using System.Net.Http;
using Ferretto.VW.InstallationApp.Interfaces;
using Ferretto.VW.InstallationApp.Resources;
using Ferretto.VW.InstallationApp.ServiceUtilities;
using Ferretto.VW.InstallationApp.ServiceUtilities.Interfaces;
using Microsoft.Practices.Unity;
using Prism.Events;
using Prism.Modularity;

namespace Ferretto.VW.InstallationApp
{
    public class InstallationAppModule : IModule
    {
        #region Fields

        private readonly IUnityContainer container;

        #endregion

        #region Constructors

        public InstallationAppModule(IUnityContainer container)
        {
            this.container = container;
            var mainWindowInstance = new MainWindow(container.Resolve<IEventAggregator>());
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
            var mainWindowVMInstance = new MainWindowViewModel(container.Resolve<IEventAggregator>());
            var helpMainWindowInstance = new HelpMainWindow(container.Resolve<IEventAggregator>());
            var installationHubClientInstance = new InstallationHubClient("http://localhost:5000", "/installation-endpoint");
            var bayControlVMInstance = new BayControlViewModel();
            var loadFirstDrawerVMInstance = new LoadFirstDrawerViewModel();
            var loadingDrawersVMInstance = new LoadingDrawersViewModel();
            var cellsSideControlVMInstance = new CellsSideControlViewModel();
            var drawerLoadingUnloadingTestVMInstance = new DrawerLoadingUnloadingTestViewModel();
            var lSMTCarouselVMInstance = new LSMTCarouselViewModel(container.Resolve<IEventAggregator>());

            this.container.RegisterInstance<IContainerInstallationHubClient>(installationHubClientInstance);
            this.container.RegisterInstance<IMainWindow>(mainWindowInstance);
            this.container.RegisterInstance<IBeltBurnishingViewModel>(beltBurnishingVMInstance);
            this.container.RegisterInstance<ICellsControlViewModel>(cellsControlVMInstance);
            this.container.RegisterInstance<ICellsPanelsControlViewModel>(cellsPanelsControlVMInstance);
            this.container.RegisterInstance<IShutter1ControlViewModel>(shutter1ControlVMInstance);
            this.container.RegisterInstance<IShutter2ControlViewModel>(shutter2ControlVMInstance);
            this.container.RegisterInstance<IShutter3ControlViewModel>(shutter3ControlVMInstance);
            this.container.RegisterInstance<IShutter1HeightControlViewModel>(shutter1HeightControlVMInstance);
            this.container.RegisterInstance<IShutter2HeightControlViewModel>(shutter2HeightControlVMInstance);
            this.container.RegisterInstance<IShutter3HeightControlViewModel>(shutter3HeightControlVMInstance);
            this.container.RegisterInstance<IIdleViewModel>(idleVMInstance);
            this.container.RegisterInstance<IInstallationStateViewModel>(installationStateVMInstance);
            this.container.RegisterInstance<ILSMTShutterEngineViewModel>(lSMTShutterEngineVMInstance);
            this.container.RegisterInstance<ILSMTHorizontalEngineViewModel>(lSMTHorizontalEngineVMInstance);
            this.container.RegisterInstance<ILSMTMainViewModel>(lSMTMainVMInstance);
            this.container.RegisterInstance<ILSMTNavigationButtonsViewModel>(lSMTNavigationButtonsVMInstance);
            this.container.RegisterInstance<ILSMTVerticalEngineViewModel>(lSMTVerticalEngineVMInstance);
            this.container.RegisterInstance<IMainWindowBackToIAPPButtonViewModel>(mainWindowBackToIAPPButtonVMInstance);
            this.container.RegisterInstance<IMainWindowNavigationButtonsViewModel>(mainWindowNavigationButtonsVMInstance);
            this.container.RegisterInstance<IResolutionCalibrationVerticalAxisViewModel>(resolutionCalibrationVerticalAxisVMInstance);
            this.container.RegisterInstance<ISSBaysViewModel>(sSBaysVMInstance);
            this.container.RegisterInstance<ISSCradleViewModel>(sSCradleVMInstance);
            this.container.RegisterInstance<ISSShutterViewModel>(sSShutterVMInstance);
            this.container.RegisterInstance<ISSMainViewModel>(sSMainVMInstance);
            this.container.RegisterInstance<ISSNavigationButtonsViewModel>(sSNavigationButtonsVMInstance);
            this.container.RegisterInstance<ISSVariousInputsViewModel>(sSVariousInputsVMInstance);
            this.container.RegisterInstance<ISSVerticalAxisViewModel>(sSVerticalAxisVMInstance);
            this.container.RegisterInstance<IVerticalAxisCalibrationViewModel>(verticalAxisCalibrationVMInstance);
            this.container.RegisterInstance<IVerticalOffsetCalibrationViewModel>(verticalOffsetCalibrationVMInstance);
            this.container.RegisterInstance<IWeightControlViewModel>(weightControlVMInstance);
            this.container.RegisterInstance<IMainWindowViewModel>(mainWindowVMInstance);
            this.container.RegisterInstance<IHelpMainWindow>(helpMainWindowInstance);
            this.container.RegisterInstance<IBayControlViewModel>(bayControlVMInstance);
            this.container.RegisterInstance<ILoadFirstDrawerViewModel>(loadFirstDrawerVMInstance);
            this.container.RegisterInstance<ILoadingDrawersViewModel>(loadingDrawersVMInstance);
            this.container.RegisterInstance<ICellsSideControlViewModel>(cellsSideControlVMInstance);
            this.container.RegisterInstance<IDrawerLoadingUnloadingTestViewModel>(drawerLoadingUnloadingTestVMInstance);
            this.container.RegisterInstance<ILSMTCarouselViewModel>(lSMTCarouselVMInstance);

            lSMTNavigationButtonsVMInstance.InitializeViewModel(this.container);
            lSMTMainVMInstance.InitializeViewModel(this.container);
            mainWindowBackToIAPPButtonVMInstance.InitializeViewModel(this.container);
            resolutionCalibrationVerticalAxisVMInstance.InitializeViewModel(this.container);
            sSMainVMInstance.InitializeViewModel(this.container);
            sSNavigationButtonsVMInstance.InitializeViewModel(this.container);
            mainWindowVMInstance.InitializeViewModel(this.container);
            verticalOffsetCalibrationVMInstance.InitializeViewModel(this.container);
            installationStateVMInstance.InitializeViewModel(this.container);
            mainWindowNavigationButtonsVMInstance.InitializeViewModel(this.container);
            weightControlVMInstance.InitializeViewModel(this.container);
            verticalAxisCalibrationVMInstance.InitializeViewModel(this.container);
            sSBaysVMInstance.InitializeViewModel(this.container);
        }

        #endregion

        #region Methods

        public void Initialize()
        {
            // HACK IModule interface requires the implementation of this method
        }

        #endregion
    }
}
