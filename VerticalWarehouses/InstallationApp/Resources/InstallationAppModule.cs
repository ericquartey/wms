using System.Diagnostics;
using Microsoft.Practices.Unity;
using Prism.Modularity;

namespace Ferretto.VW.InstallationApp
{
    public class InstallationAppModule : IModule
    {
        #region Fields

        private readonly BeltBurnishingViewModel beltBurnishingVMInstance;
        private readonly CellsControlViewModel cellsControlVMInstance;
        private readonly CellsPanelsControlViewModel cellsPanelsControlVMInstance;
        private readonly IUnityContainer container;
        private readonly Gate1ControlViewModel gate1ControlVMInstance;
        private readonly Gate1HeightControlViewModel gate1HeightControlVMInstance;
        private readonly Gate2ControlViewModel gate2ControlVMInstance;
        private readonly Gate2HeightControlViewModel gate2HeightControlVMInstance;
        private readonly Gate3ControlViewModel gate3ControlVMInstance;
        private readonly Gate3HeightControlViewModel gate3HeightControlVMInstance;
        private readonly IdleViewModel IdleVMInstance;
        private readonly InstallationStateViewModel InstallationStateVMInstance;
        private readonly LSMTGateEngineViewModel LSMTGateEngineVMInstance;
        private readonly LSMTHorizontalEngineViewModel LSMTHorizontalEngineVMInstance;
        private readonly LSMTMainViewModel LSMTMainVMInstance;
        private readonly LSMTNavigationButtonsViewModel LSMTNavigationButtonsVMInstance;
        private readonly LSMTVerticalEngineViewModel LSMTVerticalEngineVMInstance;
        private readonly MainWindowBackToIAPPButtonViewModel MainWindowBackToIAPPButtonVMInstance;
        private readonly MainWindow mainWindowInstance;
        private readonly MainWindowNavigationButtonsViewModel MainWindowNavigationButtonsVMInstance;
        private readonly MainWindowViewModel MainWindowVMInstance;
        private readonly ResolutionCalibrationVerticalAxisViewModel ResolutionCalibrationVerticalAxisVMInstance;
        private readonly SSBaysViewModel SSBaysVMInstance;
        private readonly SSCradleViewModel SSCradleVMInstance;
        private readonly SSGateViewModel SSGateVMInstance;
        private readonly SSMainViewModel SSMainVMInstance;
        private readonly SSNavigationButtonsViewModel SSNavigationButtonsVMInstance;
        private readonly SSVariousInputsViewModel SSVariousInputsVMInstance;
        private readonly SSVerticalAxisViewModel SSVerticalAxisVMInstance;
        private readonly VerticalAxisCalibrationViewModel VerticalAxisCalibrationVMInstance;
        private readonly VerticalOffsetCalibrationViewModel VerticalOffsetCalibrationVMInstance;
        private readonly WeightControlViewModel WeightControlVMInstance;

        #endregion Fields

        #region Constructors

        public InstallationAppModule(IUnityContainer _container)
        {
            Debug.Print("Module ctor begin...\n");
            this.container = _container;
            this.mainWindowInstance = new MainWindow();
            this.beltBurnishingVMInstance = new BeltBurnishingViewModel();
            this.cellsControlVMInstance = new CellsControlViewModel();
            this.cellsPanelsControlVMInstance = new CellsPanelsControlViewModel();
            this.gate1ControlVMInstance = new Gate1ControlViewModel();
            this.gate2ControlVMInstance = new Gate2ControlViewModel();
            this.gate3ControlVMInstance = new Gate3ControlViewModel();
            this.gate1HeightControlVMInstance = new Gate1HeightControlViewModel();
            this.gate2HeightControlVMInstance = new Gate2HeightControlViewModel();
            this.gate3HeightControlVMInstance = new Gate3HeightControlViewModel();
            this.IdleVMInstance = new IdleViewModel();
            this.InstallationStateVMInstance = new InstallationStateViewModel();
            this.LSMTGateEngineVMInstance = new LSMTGateEngineViewModel();
            this.LSMTHorizontalEngineVMInstance = new LSMTHorizontalEngineViewModel();
            this.LSMTNavigationButtonsVMInstance = new LSMTNavigationButtonsViewModel();
            this.LSMTMainVMInstance = new LSMTMainViewModel();
            this.LSMTVerticalEngineVMInstance = new LSMTVerticalEngineViewModel();
            this.MainWindowBackToIAPPButtonVMInstance = new MainWindowBackToIAPPButtonViewModel();
            this.MainWindowNavigationButtonsVMInstance = new MainWindowNavigationButtonsViewModel();
            this.ResolutionCalibrationVerticalAxisVMInstance = new ResolutionCalibrationVerticalAxisViewModel();
            this.SSBaysVMInstance = new SSBaysViewModel();
            this.SSCradleVMInstance = new SSCradleViewModel();
            this.SSGateVMInstance = new SSGateViewModel();
            this.SSMainVMInstance = new SSMainViewModel();
            this.SSNavigationButtonsVMInstance = new SSNavigationButtonsViewModel();
            this.SSVariousInputsVMInstance = new SSVariousInputsViewModel();
            this.SSVerticalAxisVMInstance = new SSVerticalAxisViewModel();
            this.VerticalAxisCalibrationVMInstance = new VerticalAxisCalibrationViewModel();
            this.VerticalOffsetCalibrationVMInstance = new VerticalOffsetCalibrationViewModel();
            this.WeightControlVMInstance = new WeightControlViewModel();
            this.MainWindowVMInstance = new MainWindowViewModel();

            this.container.RegisterInstance<IMainWindow>(this.mainWindowInstance);
            this.container.RegisterInstance<IBeltBurnishingViewModel>(this.beltBurnishingVMInstance);
            this.container.RegisterInstance<ICellsControlViewModel>(this.cellsControlVMInstance);
            this.container.RegisterInstance<ICellsPanelsControlViewModel>(this.cellsPanelsControlVMInstance);
            this.container.RegisterInstance<IGate1ControlViewModel>(this.gate1ControlVMInstance);
            this.container.RegisterInstance<IGate2ControlViewModel>(this.gate2ControlVMInstance);
            this.container.RegisterInstance<IGate3ControlViewModel>(this.gate3ControlVMInstance);
            this.container.RegisterInstance<IGate1HeightControlViewModel>(this.gate1HeightControlVMInstance);
            this.container.RegisterInstance<IGate2HeightControlViewModel>(this.gate2HeightControlVMInstance);
            this.container.RegisterInstance<IGate3HeightControlViewModel>(this.gate3HeightControlVMInstance);
            this.container.RegisterInstance<IIdleViewModel>(this.IdleVMInstance);
            this.container.RegisterInstance<IInstallationStateViewModel>(this.InstallationStateVMInstance);
            this.container.RegisterInstance<ILSMTGateEngineViewModel>(this.LSMTGateEngineVMInstance);
            this.container.RegisterInstance<ILSMTHorizontalEngineViewModel>(this.LSMTHorizontalEngineVMInstance);
            this.container.RegisterInstance<ILSMTMainViewModel>(this.LSMTMainVMInstance);
            this.container.RegisterInstance<ILSMTNavigationButtonsViewModel>(this.LSMTNavigationButtonsVMInstance);
            this.container.RegisterInstance<ILSMTVerticalEngineViewModel>(this.LSMTVerticalEngineVMInstance);
            this.container.RegisterInstance<IMainWindowBackToIAPPButtonViewModel>(this.MainWindowBackToIAPPButtonVMInstance);
            this.container.RegisterInstance<IMainWindowNavigationButtonsViewModel>(this.MainWindowNavigationButtonsVMInstance);
            this.container.RegisterInstance<IResolutionCalibrationVerticalAxisViewModel>(this.ResolutionCalibrationVerticalAxisVMInstance);
            this.container.RegisterInstance<ISSBaysViewModel>(this.SSBaysVMInstance);
            this.container.RegisterInstance<ISSCradleViewModel>(this.SSCradleVMInstance);
            this.container.RegisterInstance<ISSGateViewModel>(this.SSGateVMInstance);
            this.container.RegisterInstance<ISSMainViewModel>(this.SSMainVMInstance);
            this.container.RegisterInstance<ISSNavigationButtonsViewModel>(this.SSNavigationButtonsVMInstance);
            this.container.RegisterInstance<ISSVariousInputsViewModel>(this.SSVariousInputsVMInstance);
            this.container.RegisterInstance<ISSVerticalAxisViewModel>(this.SSVerticalAxisVMInstance);
            this.container.RegisterInstance<IVerticalAxisCalibrationViewModel>(this.VerticalAxisCalibrationVMInstance);
            this.container.RegisterInstance<IVerticalOffsetCalibrationViewModel>(this.VerticalOffsetCalibrationVMInstance);
            this.container.RegisterInstance<IWeightControlViewModel>(this.WeightControlVMInstance);
            this.container.RegisterInstance<IMainWindowViewModel>(this.MainWindowVMInstance);

            this.LSMTNavigationButtonsVMInstance.InitializeViewModel(this.container);
            this.LSMTMainVMInstance.InitializeViewModel(this.container);
            this.MainWindowBackToIAPPButtonVMInstance.InitializeViewModel(this.container);
            this.ResolutionCalibrationVerticalAxisVMInstance.InitializeViewModel(this.container);
            this.SSMainVMInstance.InitializeViewModel(this.container);
            this.SSNavigationButtonsVMInstance.InitializeViewModel(this.container);
            this.MainWindowVMInstance.InitializeViewModel(this.container);
            Debug.Print("Module ctor ended...\n");
        }

        #endregion Constructors

        #region Methods

        public void Initialize()
        {
            Debug.Print("Module initialize method called...\n");
        }

        #endregion Methods

        //public void Initialize()
        //{
        //    this.container.RegisterInstance<IMainWindow>(this.mainWindowInstance);
        //    this.container.RegisterInstance<IBeltBurnishingViewModel>(this.beltBurnishingVMInstance);
        //    this.container.RegisterInstance<ICellsControlViewModel>(this.cellsControlVMInstance);
        //    this.container.RegisterInstance<ICellsPanelsControlViewModel>(this.cellsPanelsControlVMInstance);
        //    this.container.RegisterInstance<IGate1ControlViewModel>(this.gate1ControlVMInstance);
        //    this.container.RegisterInstance<IGate2ControlViewModel>(this.gate2ControlVMInstance);
        //    this.container.RegisterInstance<IGate3ControlViewModel>(this.gate3ControlVMInstance);
        //    this.container.RegisterInstance<IGate1HeightControlViewModel>(this.gate1HeightControlVMInstance);
        //    this.container.RegisterInstance<IGate2HeightControlViewModel>(this.gate2HeightControlVMInstance);
        //    this.container.RegisterInstance<IGate3HeightControlViewModel>(this.gate3HeightControlVMInstance);
        //    this.container.RegisterInstance<IIdleViewModel>(this.IdleVMInstance);
        //    this.container.RegisterInstance<IInstallationStateViewModel>(this.InstallationStateVMInstance);
        //    this.container.RegisterInstance<ILSMTGateEngineViewModel>(this.LSMTGateEngineVMInstance);
        //    this.container.RegisterInstance<ILSMTHorizontalEngineViewModel>(this.LSMTHorizontalEngineVMInstance);
        //    this.container.RegisterInstance<ILSMTMainViewModel>(this.LSMTMainVMInstance);
        //    this.container.RegisterInstance<ILSMTNavigationButtonsViewModel>(this.LSMTNavigationButtonsVMInstance);
        //    this.container.RegisterInstance<ILSMTVerticalEngineViewModel>(this.LSMTVerticalEngineVMInstance);
        //    this.container.RegisterInstance<IMainWindowBackToIAPPButtonViewModel>(this.MainWindowBackToIAPPButtonVMInstance);
        //    this.container.RegisterInstance<IMainWindowNavigationButtonsViewModel>(this.MainWindowNavigationButtonsVMInstance);
        //    this.container.RegisterInstance<IResolutionCalibrationVerticalAxisViewModel>(this.ResolutionCalibrationVerticalAxisVMInstance);
        //    this.container.RegisterInstance<ISSBaysViewModel>(this.SSBaysVMInstance);
        //    this.container.RegisterInstance<ISSCradleViewModel>(this.SSCradleVMInstance);
        //    this.container.RegisterInstance<ISSGateViewModel>(this.SSGateVMInstance);
        //    this.container.RegisterInstance<ISSMainViewModel>(this.SSMainVMInstance);
        //    this.container.RegisterInstance<ISSNavigationButtonsViewModel>(this.SSNavigationButtonsVMInstance);
        //    this.container.RegisterInstance<ISSVariousInputsViewModel>(this.SSVariousInputsVMInstance);
        //    this.container.RegisterInstance<ISSVerticalAxisViewModel>(this.SSVerticalAxisVMInstance);
        //    this.container.RegisterInstance<IVerticalAxisCalibrationViewModel>(this.VerticalAxisCalibrationVMInstance);
        //    this.container.RegisterInstance<IVerticalOffsetCalibrationViewModel>(this.VerticalOffsetCalibrationVMInstance);
        //    this.container.RegisterInstance<IWeightControlViewModel>(this.WeightControlVMInstance);
        //    this.container.RegisterInstance<IMainWindowViewModel>(this.MainWindowVMInstance);

        //    this.LSMTNavigationButtonsVMInstance.InitializeViewModel(this.container);
        //    this.LSMTMainVMInstance.InitializeViewModel(this.container);
        //    this.MainWindowBackToIAPPButtonVMInstance.InitializeViewModel(this.container);
        //    this.ResolutionCalibrationVerticalAxisVMInstance.InitializeViewModel(this.container);
        //    this.SSMainVMInstance.InitializeViewModel(this.container);
        //    this.SSNavigationButtonsVMInstance.InitializeViewModel(this.container);
        //    this.MainWindowVMInstance.InitializeViewModel(this.container);
        //}
    }
}
