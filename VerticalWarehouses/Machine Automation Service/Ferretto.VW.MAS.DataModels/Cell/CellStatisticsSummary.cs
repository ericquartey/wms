using System.Collections.Generic;

namespace Ferretto.VW.MAS.DataModels
{
    public sealed class CellStatisticsSummary
    {
        #region Properties

        public double CellOccupationRatio { get; set; }

        public IEnumerable<CellStatusStatistics> CellStatusStatistics { get; set; }

        public int TotalBackCells { get; set; }

        public int TotalCells { get; set; }

        public int TotalFrontCells { get; set; }

        #endregion
    }
}
