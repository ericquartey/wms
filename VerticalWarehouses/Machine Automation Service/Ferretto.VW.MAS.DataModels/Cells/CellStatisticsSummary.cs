using System.Collections.Generic;

namespace Ferretto.VW.MAS.DataModels.Cells
{
    public sealed class CellStatisticsSummary
    {
        #region Properties

        public double CellOccupationPercentage { get; set; }

        public IEnumerable<CellStatusStatistics> CellStatusStatistics { get; set; }

        public int TotalBackCells { get; set; }

        public int TotalCells { get; set; }

        public int TotalFrontCells { get; set; }

        #endregion
    }
}
