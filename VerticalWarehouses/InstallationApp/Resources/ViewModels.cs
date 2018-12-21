namespace Ferretto.VW.InstallationApp
{
    public static class ViewModels
    {
        #region Fields

        public static readonly BeltBurnishingViewModel BeltBurnishingVMInstance;
        public static readonly CellsControlViewModel CellsControlVMInstance;
        public static readonly CellsPanelsControlViewModel CellsPanelControlVMInsance;
        public static readonly Gate1ControlViewModel Gate1ControlVMInstance;
        public static readonly Gate1HeightControlViewModel Gate1HeightControlVMInstance;
        public static readonly Gate2ControlViewModel Gate2ControlVMInstance;
        public static readonly Gate2HeightControlViewModel Gate2HeightControlVMInstance;
        public static readonly Gate3ControlViewModel Gate3ControlVMInstance;
        public static readonly Gate3HeightControlViewModel Gate3HeightControlVMInstance;
        public static readonly IdleViewModel IdleVMInstance;
        public static readonly InstallationStateViewModel InstallationStateVMInstance;
        public static readonly LSMTGateEngineViewModel LSMTGateEngineVMInstance;
        public static readonly LSMTHorizontalEngineViewModel LSMTHorizontalEngineVMInstance;
        public static readonly LSMTMainViewModel LSMTMainVMInstance;
        public static readonly LSMTNavigationButtonsViewModel LSMTNavigationButtonsVMInstance;
        public static readonly LSMTVerticalEngineViewModel LSMTVerticalEngineVMInstance;
        public static readonly MainWindowBackToIAPPButtonViewModel MainWindowBackToIAPPButtonVMInstance;
        public static readonly MainWindowNavigationButtonsViewModel MainWindowNavigationButtonsVMInstance;
        public static readonly MainWindowViewModel MainWindowVMInstance;
        public static readonly ResolutionCalibrationVerticalAxisViewModel ResolutionCalibrationVerticalAxisVMInstance;
        public static readonly SSBaysViewModel SSBaysVMInstance;
        public static readonly SSCradleViewModel SSCradleVMInstance;
        public static readonly SSGateViewModel SSGateVMInstance;
        public static readonly SSMainViewModel SSMainVMInstance;
        public static readonly SSNavigationButtonsViewModel SSNavigationButtonsVMInstance;
        public static readonly SSVariousInputsViewModel SSVariousInputsVMInstance;
        public static readonly SSVerticalAxisViewModel SSVerticalAxisVMInstance;
        public static readonly VerticalAxisCalibrationViewModel VerticalAxisCalibrationVMInstance;
        public static readonly VerticalOffsetCalibrationViewModel VerticalOffsetCalibrationVMInstance;
        public static readonly WeightControlViewModel WeightControlVMInstance;

        #endregion Fields

        #region Constructors

        static ViewModels()
        {
            BeltBurnishingVMInstance = new BeltBurnishingViewModel();
            CellsControlVMInstance = new CellsControlViewModel();
            CellsPanelControlVMInsance = new CellsPanelsControlViewModel();
            Gate1ControlVMInstance = new Gate1ControlViewModel();
            Gate1HeightControlVMInstance = new Gate1HeightControlViewModel();
            Gate2ControlVMInstance = new Gate2ControlViewModel();
            Gate2HeightControlVMInstance = new Gate2HeightControlViewModel();
            Gate3ControlVMInstance = new Gate3ControlViewModel();
            Gate3HeightControlVMInstance = new Gate3HeightControlViewModel();
            IdleVMInstance = new IdleViewModel();
            InstallationStateVMInstance = new InstallationStateViewModel();
            LSMTGateEngineVMInstance = new LSMTGateEngineViewModel();
            LSMTHorizontalEngineVMInstance = new LSMTHorizontalEngineViewModel();
            LSMTNavigationButtonsVMInstance = new LSMTNavigationButtonsViewModel();
            LSMTVerticalEngineVMInstance = new LSMTVerticalEngineViewModel();
            MainWindowNavigationButtonsVMInstance = new MainWindowNavigationButtonsViewModel();
            ResolutionCalibrationVerticalAxisVMInstance = new ResolutionCalibrationVerticalAxisViewModel();
            SSBaysVMInstance = new SSBaysViewModel();
            SSCradleVMInstance = new SSCradleViewModel();
            SSGateVMInstance = new SSGateViewModel();
            SSNavigationButtonsVMInstance = new SSNavigationButtonsViewModel();
            SSVariousInputsVMInstance = new SSVariousInputsViewModel();
            SSVerticalAxisVMInstance = new SSVerticalAxisViewModel();
            VerticalAxisCalibrationVMInstance = new VerticalAxisCalibrationViewModel();
            VerticalOffsetCalibrationVMInstance = new VerticalOffsetCalibrationViewModel();
            WeightControlVMInstance = new WeightControlViewModel();
            MainWindowBackToIAPPButtonVMInstance = new MainWindowBackToIAPPButtonViewModel();
            MainWindowVMInstance = new MainWindowViewModel();
            LSMTMainVMInstance = new LSMTMainViewModel();
            SSMainVMInstance = new SSMainViewModel();
        }

        #endregion Constructors
    }
}
