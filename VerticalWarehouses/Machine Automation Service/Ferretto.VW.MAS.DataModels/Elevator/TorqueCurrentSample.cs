using System;

namespace Ferretto.VW.MAS.DataModels
{
    public sealed class TorqueCurrentSample : DataModel
    {
        #region Properties

        public TorqueCurrentMeasurementSession MeasurementSession { get; set; }

        public int MeasurementSessionId { get; set; }

        public DateTime RequestTimeStamp { get; set; }

        public DateTime TimeStamp { get; set; }

        public double Value { get; set; }

        #endregion
    }
}
