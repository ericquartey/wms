namespace Ferretto.VW.MAS.DataModels
{
    public sealed class SetupStatus : DataModel
    {
        #region Properties

        public bool AllLoadingUnits { get; set; }

        public bool Bay1Check { get; set; }

        public bool Bay1FirstLoadingUnit { get; set; }

        public bool Bay1Laser { get; set; }

        public bool Bay1Shape { get; set; }

        public bool Bay1Shutter { get; set; }

        public bool Bay2Check { get; set; }

        public bool Bay2FirstLoadingUnit { get; set; }

        public bool Bay2Laser { get; set; }

        public bool Bay2Shape { get; set; }

        public bool Bay2Shutter { get; set; }

        public bool Bay3Check { get; set; }

        public bool Bay3FirstLoadingUnit { get; set; }

        public bool Bay3Laser { get; set; }

        public bool Bay3Shape { get; set; }

        public bool Bay3Shutter { get; set; }

        public bool BeltBurnishingCompleted { get; set; }

        public int BeltBurnishingCompletedCycles { get; set; }

        public int BeltBurnishingRequiredCycles { get; set; }

        public bool CellsHeightCheck { get; set; }

        public System.DateTime? CompletedDate { get; set; }

        public bool HorizontalHoming { get; set; }

        public bool PanelsCheck { get; set; }

        public bool VerticalOffsetCalibration { get; set; }

        public bool VerticalResolution { get; set; }

        public bool WeightMeasurement { get; set; }

        #endregion
    }
}
