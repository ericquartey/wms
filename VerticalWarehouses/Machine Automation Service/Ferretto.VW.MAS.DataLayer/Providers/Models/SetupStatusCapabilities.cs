namespace Ferretto.VW.MAS.DataLayer
{
    public class SetupStatusCapabilities
    {
        #region Fields

        internal static readonly SetupStatusCapabilities Complete = new SetupStatusCapabilities
        {
            AllLoadingUnits = SetupStepStatus.Complete,
            Bay1 = BaySetupStatus.Complete,
            Bay2 = BaySetupStatus.Complete,
            Bay3 = BaySetupStatus.Complete,
            BeltBurnishing = SetupStepStatus.Complete,
            CellsHeightCheck = SetupStepStatus.Complete,
            HorizontalHoming = SetupStepStatus.Complete,
            IsComplete = true,
            CellPanelsCheck = SetupStepStatus.Complete,
            VerticalOffsetCalibration = SetupStepStatus.Complete,
            VerticalOriginCalibration = SetupStepStatus.Complete,
            VerticalResolutionCalibration = SetupStepStatus.Complete,
            WeightMeasurement = SetupStepStatus.Complete,
            HorizontalChainCalibration = SetupStepStatus.Complete,
            HorizontalResolutionCalibration = SetupStepStatus.Complete,
            DepositAndPickUpTest = SetupStepStatus.Complete,
            FullTest = SetupStepStatus.Complete
        };

        #endregion

        #region Properties

        public SetupStepStatus AllLoadingUnits { get; set; }

        public BaySetupStatus Bay1 { get; set; }

        public BaySetupStatus Bay2 { get; set; }

        public BaySetupStatus Bay3 { get; set; }

        public SetupStepStatus BeltBurnishing { get; set; }

        public SetupStepStatus CellPanelsCheck { get; set; }

        public SetupStepStatus CellsHeightCheck { get; set; }

        public System.DateTime? CompletedDate { get; set; }

        public SetupStepStatus DepositAndPickUpTest { get; set; }

        public SetupStepStatus FullTest { get; set; }

        public SetupStepStatus HorizontalChainCalibration { get; set; }

        public SetupStepStatus HorizontalHoming { get; set; }

        public SetupStepStatus HorizontalResolutionCalibration { get; set; }

        public bool IsComplete { get; set; }

        public SetupStepStatus LoadFirstDrawerTest { get; set; }

        public SetupStepStatus VerticalOffsetCalibration { get; set; }

        public SetupStepStatus VerticalOriginCalibration { get; set; }

        public SetupStepStatus VerticalResolutionCalibration { get; set; }

        public SetupStepStatus WeightMeasurement { get; set; }

        #endregion
    }
}
