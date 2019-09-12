using System;

namespace Ferretto.VW.MAS.DataModels
{
    public class TorqueCurrentSample
    {
        #region Properties

        public int Id { get; set; }

        public TorqueCurrentMeasurementSession MeasurementSession { get; set; }

        public int MeasurementSessionId { get; set; }

        public DateTime TimeStamp { get; set; }

        public decimal Value { get; set; }

        #endregion
    }
}
