using System.Collections.Generic;

namespace Ferretto.VW.MAS.DataModels
{
    public class TorqueCurrentMeasurementSession
    {
        #region Properties

        public IEnumerable<TorqueCurrentSample> DataSamples { get; set; }

        public int Id { get; set; }

        public double LoadedNetWeight { get; set; }

        public int? LoadingUnitId { get; set; }

        #endregion
    }
}
