using System.Collections.Generic;
using Ferretto.VW.MAS.DataModels;

namespace Ferretto.VW.MAS.DataLayer
{
    public sealed class ErrorStatisticsSummary
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
