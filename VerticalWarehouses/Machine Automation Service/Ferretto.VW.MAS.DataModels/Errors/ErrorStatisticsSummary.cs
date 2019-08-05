using System.Collections.Generic;

namespace Ferretto.VW.MAS.DataModels
{
    public class ErrorStatisticsSummary
    {
        #region Properties

        public IEnumerable<ErrorStatisticsDetail> Errors { get; set; }

        public double ReliabilityPercentage { get; set; }

        public int TotalErrors { get; set; }

        public int TotalLoadingUnits { get; set; }

        public int TotalLoadingUnitsBetweenErrors { get; set; }

        #endregion
    }
}
