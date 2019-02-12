using Microsoft.Practices.Unity;
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
            var mainWindowInstance = new MainWindow();
            var beltBurnishingVMInstance = new BeltBurnishingViewModel();
            var cellsControlVMInstance = new CellsControlViewModel();
            var cellsPanelsControlVMInstance = new CellsPanelsControlViewModel();
            var gate1ControlVMInstance = new Gate1ControlViewModel();
            var gate2ControlVMInstance = new Gate2ControlViewModel();
            var gate3ControlVMInstance = new Gate3ControlViewModel();
            var gate1HeightControlVMInstance = new Gate1HeightControlViewModel();
            var gate2HeightControlVMInstance = new Gate2HeightControlViewModel();
            var gate3HeightControlVMInstance = new Gate3HeightControlViewModel();
            var idleVMInstance = new IdleViewModel();
            var installationStateVMInstance = new InstallationStateViewModel();
            var lSMTGateEngineVMInstance = new LSMTGateEngineViewModel();
            var lSMTHorizontalEngineVMInstance = new LSMTHorizontalEngineViewModel();
            var lSMTNavigationButtonsVMInstance = new LSMTNavigationButtonsViewModel();
            var lSMTMainVMInstance = new LSMTMainViewModel();
            var lSMTVerticalEngineVMInstance = new LSMTVerticalEngineViewModel();
            var mainWindowBackToIAPPButtonVMInstance = new MainWindowBackToIAPPButtonViewModel();
            var mainWindowNavigationButtonsVMInstance = new MainWindowNavigationButtonsViewModel();
            var resolutionCalibrationVerticalAxisVMInstance = new ResolutionCalibrationVerticalAxisViewModel();
            var sSBaysVMInstance = new SSBaysViewModel();
            var sSCradleVMInstance = new SSCradleViewModel();
            var sSGateVMInstance = new SSGateViewModel();
            var sSMainVMInstance = new SSMainViewModel();
            var sSNavigationButtonsVMInstance = new SSNavigationButtonsViewModel();
            var sSVariousInputsVMInstance = new SSVariousInputsViewModel();
            var sSVerticalAxisVMInstance = new SSVerticalAxisViewModel();
            var verticalAxisCalibrationVMInstance = new VerticalAxisCalibrationViewModel();
            var verticalOffsetCalibrationVMInstance = new VerticalOffsetCalibrationViewModel();
            var weightControlVMInstance = new WeightControlViewModel();
            var mainWindowVMInstance = new MainWindowViewModel();
            var helpMainWindowInstance = new HelpMainWindow();

            this.container.RegisterInstance<IMainWindow>(mainWindowInstance);
            this.container.RegisterInstance<IBeltBurnishingViewModel>(beltBurnishingVMInstance);
            this.container.RegisterInstance<ICellsControlViewModel>(cellsControlVMInstance);
            this.container.RegisterInstance<ICellsPanelsControlViewModel>(cellsPanelsControlVMInstance);
            this.container.RegisterInstance<IGate1ControlViewModel>(gate1ControlVMInstance);
            this.container.RegisterInstance<IGate2ControlViewModel>(gate2ControlVMInstance);
            this.container.RegisterInstance<IGate3ControlViewModel>(gate3ControlVMInstance);
            this.container.RegisterInstance<IGate1HeightControlViewModel>(gate1HeightControlVMInstance);
            this.container.RegisterInstance<IGate2HeightControlViewModel>(gate2HeightControlVMInstance);
            this.container.RegisterInstance<IGate3HeightControlViewModel>(gate3HeightControlVMInstance);
            this.container.RegisterInstance<IIdleViewModel>(idleVMInstance);
            this.container.RegisterInstance<IInstallationStateViewModel>(installationStateVMInstance);
            this.container.RegisterInstance<ILSMTGateEngineViewModel>(lSMTGateEngineVMInstance);
            this.container.RegisterInstance<ILSMTHorizontalEngineViewModel>(lSMTHorizontalEngineVMInstance);
            this.container.RegisterInstance<ILSMTMainViewModel>(lSMTMainVMInstance);
            this.container.RegisterInstance<ILSMTNavigationButtonsViewModel>(lSMTNavigationButtonsVMInstance);
            this.container.RegisterInstance<ILSMTVerticalEngineViewModel>(lSMTVerticalEngineVMInstance);
            this.container.RegisterInstance<IMainWindowBackToIAPPButtonViewModel>(mainWindowBackToIAPPButtonVMInstance);
            this.container.RegisterInstance<IMainWindowNavigationButtonsViewModel>(mainWindowNavigationButtonsVMInstance);
            this.container.RegisterInstance<IResolutionCalibrationVerticalAxisViewModel>(resolutionCalibrationVerticalAxisVMInstance);
            this.container.RegisterInstance<ISSBaysViewModel>(sSBaysVMInstance);
            this.container.RegisterInstance<ISSCradleViewModel>(sSCradleVMInstance);
            this.container.RegisterInstance<ISSGateViewModel>(sSGateVMInstance);
            this.container.RegisterInstance<ISSMainViewModel>(sSMainVMInstance);
            this.container.RegisterInstance<ISSNavigationButtonsViewModel>(sSNavigationButtonsVMInstance);
            this.container.RegisterInstance<ISSVariousInputsViewModel>(sSVariousInputsVMInstance);
            this.container.RegisterInstance<ISSVerticalAxisViewModel>(sSVerticalAxisVMInstance);
            this.container.RegisterInstance<IVerticalAxisCalibrationViewModel>(verticalAxisCalibrationVMInstance);
            this.container.RegisterInstance<IVerticalOffsetCalibrationViewModel>(verticalOffsetCalibrationVMInstance);
            this.container.RegisterInstance<IWeightControlViewModel>(weightControlVMInstance);
            this.container.RegisterInstance<IMainWindowViewModel>(mainWindowVMInstance);
            this.container.RegisterInstance<IHelpMainWindow>(helpMainWindowInstance);

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
