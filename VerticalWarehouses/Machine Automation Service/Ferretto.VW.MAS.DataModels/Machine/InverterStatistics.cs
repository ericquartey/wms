using System;

namespace Ferretto.VW.MAS.DataModels
{
    public sealed class InverterStatistics : DataModel
    {
        #region Properties

        public double AverageActivePower { get; set; }

        public double AverageRMSCurrent { get; set; }

        public DateTimeOffset DateTime { get; set; }

        public double PeakHeatSinkTemperature { get; set; }

        public double PeakInsideTemperature { get; set; }

        #endregion
    }
}
