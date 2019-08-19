namespace Ferretto.VW.MAS.DataLayer.Providers.Models
{
    public class SetupStatusCapabilities
    {
        #region Properties

        public SetupStepStatus AllLoadingUnits { get; set; }

        public BaySetupStatus Bay1 { get; set; }

        public BaySetupStatus Bay2 { get; set; }

        public BaySetupStatus Bay3 { get; set; }

        public SetupStepStatus BeltBurnishing { get; set; }

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
