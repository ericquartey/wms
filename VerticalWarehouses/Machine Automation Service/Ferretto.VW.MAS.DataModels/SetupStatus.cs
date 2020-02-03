using System;

namespace Ferretto.VW.MAS.DataModels
{
    public sealed class SetupStatus : DataModel
    {
        #region Properties

        public bool AllLoadingUnits { get; set; }

        public bool Bay1FirstLoadingUnit { get; set; }

        public bool Bay1HeightCheck { get; set; }

        public bool Bay1Laser { get; set; }

        public bool Bay1Shape { get; set; }

        public bool Bay2FirstLoadingUnit { get; set; }

        public bool Bay2HeightCheck { get; set; }

        public bool Bay2Laser { get; set; }

        public bool Bay2Shape { get; set; }

        public bool Bay3FirstLoadingUnit { get; set; }

        public bool Bay3HeightCheck { get; set; }

        public bool Bay3Laser { get; set; }

        public bool Bay3Shape { get; set; }

        public DateTime? CompletedDate { get; set; }

        public bool HorizontalHoming { get; set; }

        public bool WeightMeasurement { get; set; }

        #endregion
    }
}
