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
            BeltBurnishing = BeltBurnishingSetupStepStatus.Complete,
            CellsHeightCheck = SetupStepStatus.Complete,
            HorizontalHoming = SetupStepStatus.Complete,
            IsComplete = true,
            PanelsCheck = SetupStepStatus.Complete,
            VerticalOffsetCalibration = SetupStepStatus.Complete,
            VerticalOriginCalibration = SetupStepStatus.Complete,
            VerticalResolution = SetupStepStatus.Complete,
            WeightMeasurement = SetupStepStatus.Complete,
        };

        #endregion

        #region Properties

        public SetupStepStatus AllLoadingUnits { get; set; }

        public BaySetupStatus Bay1 { get; set; }

        public BaySetupStatus Bay2 { get; set; }

        public BaySetupStatus Bay3 { get; set; }

        public BeltBurnishingSetupStepStatus BeltBurnishing { get; set; }

        public SetupStepStatus CellsHeightCheck { get; set; }

        public System.DateTime? CompletedDate { get; set; }

        public SetupStepStatus HorizontalHoming { get; set; }

        public bool IsComplete { get; set; }

        public SetupStepStatus PanelsCheck { get; set; }

        public SetupStepStatus VerticalOffsetCalibration { get; set; }

        public SetupStepStatus VerticalOriginCalibration { get; set; }

        public SetupStepStatus VerticalResolution { get; set; }

        public SetupStepStatus WeightMeasurement { get; set; }

        #endregion
    }
}
