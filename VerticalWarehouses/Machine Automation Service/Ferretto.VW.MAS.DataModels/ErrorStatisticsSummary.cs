using System.Collections.Generic;

namespace Ferretto.VW.MAS.DataModels
{
    public class ErrorStatisticsDetail
    {
        #region Properties

        public int Code { get; set; }

        public string Description { get; set; }

        public double RatioTotal { get; set; }

        public int Total { get; set; }

        #endregion
    }

    public class ErrorStatisticsSummary
    {
        #region Properties

        public IEnumerable<ErrorStatisticsDetail> Errors { get; set; }

        public double RatioRealiability { get; set; }

        public int TotalErrors { get; set; }

        public int TotalLoadingUnits { get; set; }

        public int TotalLoadingUnitsBetweenErrors { get; set; }

        #endregion
    }
}
