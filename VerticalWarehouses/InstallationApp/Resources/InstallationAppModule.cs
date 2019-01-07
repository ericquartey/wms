using Microsoft.Practices.Unity;
using Prism.Modularity;

namespace Ferretto.VW.InstallationApp
{
    public class InstallationAppModule : IModule
    {
        #region Fields

        private readonly IUnityContainer container;

        #endregion Fields

        #region Constructors

        public InstallationAppModule(IUnityContainer _container)
        {
            this.container = _container;
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
            var IdleVMInstance = new IdleViewModel();
            var InstallationStateVMInstance = new InstallationStateViewModel();
            var LSMTGateEngineVMInstance = new LSMTGateEngineViewModel();
            var LSMTHorizontalEngineVMInstance = new LSMTHorizontalEngineViewModel();
            var LSMTNavigationButtonsVMInstance = new LSMTNavigationButtonsViewModel();
            var LSMTMainVMInstance = new LSMTMainViewModel();
            var LSMTVerticalEngineVMInstance = new LSMTVerticalEngineViewModel();
            var MainWindowBackToIAPPButtonVMInstance = new MainWindowBackToIAPPButtonViewModel();
            var MainWindowNavigationButtonsVMInstance = new MainWindowNavigationButtonsViewModel();
            var ResolutionCalibrationVerticalAxisVMInstance = new ResolutionCalibrationVerticalAxisViewModel();
            var SSBaysVMInstance = new SSBaysViewModel();
            var SSCradleVMInstance = new SSCradleViewModel();
            var SSGateVMInstance = new SSGateViewModel();
            var SSMainVMInstance = new SSMainViewModel();
            var SSNavigationButtonsVMInstance = new SSNavigationButtonsViewModel();
            var SSVariousInputsVMInstance = new SSVariousInputsViewModel();
            var SSVerticalAxisVMInstance = new SSVerticalAxisViewModel();
            var VerticalAxisCalibrationVMInstance = new VerticalAxisCalibrationViewModel();
            var VerticalOffsetCalibrationVMInstance = new VerticalOffsetCalibrationViewModel();
            var WeightControlVMInstance = new WeightControlViewModel();
            var MainWindowVMInstance = new MainWindowViewModel();

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
            this.container.RegisterInstance<IIdleViewModel>(IdleVMInstance);
            this.container.RegisterInstance<IInstallationStateViewModel>(InstallationStateVMInstance);
            this.container.RegisterInstance<ILSMTGateEngineViewModel>(LSMTGateEngineVMInstance);
            this.container.RegisterInstance<ILSMTHorizontalEngineViewModel>(LSMTHorizontalEngineVMInstance);
            this.container.RegisterInstance<ILSMTMainViewModel>(LSMTMainVMInstance);
            this.container.RegisterInstance<ILSMTNavigationButtonsViewModel>(LSMTNavigationButtonsVMInstance);
            this.container.RegisterInstance<ILSMTVerticalEngineViewModel>(LSMTVerticalEngineVMInstance);
            this.container.RegisterInstance<IMainWindowBackToIAPPButtonViewModel>(MainWindowBackToIAPPButtonVMInstance);
            this.container.RegisterInstance<IMainWindowNavigationButtonsViewModel>(MainWindowNavigationButtonsVMInstance);
            this.container.RegisterInstance<IResolutionCalibrationVerticalAxisViewModel>(ResolutionCalibrationVerticalAxisVMInstance);
            this.container.RegisterInstance<ISSBaysViewModel>(SSBaysVMInstance);
            this.container.RegisterInstance<ISSCradleViewModel>(SSCradleVMInstance);
            this.container.RegisterInstance<ISSGateViewModel>(SSGateVMInstance);
            this.container.RegisterInstance<ISSMainViewModel>(SSMainVMInstance);
            this.container.RegisterInstance<ISSNavigationButtonsViewModel>(SSNavigationButtonsVMInstance);
            this.container.RegisterInstance<ISSVariousInputsViewModel>(SSVariousInputsVMInstance);
            this.container.RegisterInstance<ISSVerticalAxisViewModel>(SSVerticalAxisVMInstance);
            this.container.RegisterInstance<IVerticalAxisCalibrationViewModel>(VerticalAxisCalibrationVMInstance);
            this.container.RegisterInstance<IVerticalOffsetCalibrationViewModel>(VerticalOffsetCalibrationVMInstance);
            this.container.RegisterInstance<IWeightControlViewModel>(WeightControlVMInstance);
            this.container.RegisterInstance<IMainWindowViewModel>(MainWindowVMInstance);

            LSMTNavigationButtonsVMInstance.InitializeViewModel(this.container);
            LSMTMainVMInstance.InitializeViewModel(this.container);
            MainWindowBackToIAPPButtonVMInstance.InitializeViewModel(this.container);
            ResolutionCalibrationVerticalAxisVMInstance.InitializeViewModel(this.container);
            SSMainVMInstance.InitializeViewModel(this.container);
            SSNavigationButtonsVMInstance.InitializeViewModel(this.container);
            MainWindowVMInstance.InitializeViewModel(this.container);
            VerticalOffsetCalibrationVMInstance.InitializeViewModel(this.container);
            InstallationStateVMInstance.InitializeViewModel(this.container);
            MainWindowNavigationButtonsVMInstance.InitializeViewModel(this.container);
            WeightControlVMInstance.InitializeViewModel(this.container);
            VerticalAxisCalibrationVMInstance.InitializeViewModel(this.container);
        }

        #endregion Constructors

        #region Methods

        public void Initialize()
        {
        }

        #endregion Methods
    }
}
