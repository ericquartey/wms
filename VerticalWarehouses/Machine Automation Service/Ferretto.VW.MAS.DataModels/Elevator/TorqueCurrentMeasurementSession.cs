using System.Collections.Generic;

namespace Ferretto.VW.MAS.DataModels
{
    public sealed class TorqueCurrentMeasurementSession : DataModel
    {
        #region Properties

        public IEnumerable<TorqueCurrentSample> DataSamples { get; set; }

        public double LoadedNetWeight { get; set; }

        public int? LoadingUnitId { get; set; }

        #endregion
    }
}
