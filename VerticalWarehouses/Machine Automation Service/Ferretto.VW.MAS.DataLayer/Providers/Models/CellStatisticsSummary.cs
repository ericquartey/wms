using System.Collections.Generic;
using Ferretto.VW.MAS.DataModels;

namespace Ferretto.VW.MAS.DataLayer
{
    public sealed class CellStatisticsSummary
    {
        #region Properties

        public double CellOccupationPercentage { get; set; }

        public IEnumerable<CellStatusStatistics> CellStatusStatistics { get; set; }

        public double FragmentBackPercent { get; set; }

        public double FragmentFrontPercent { get; set; }

        public double FragmentTotalPercent { get; set; }

        public Dictionary<WarehouseSide, double> MaxSolidSpace { get; set; }

        public int TotalBackCells { get; set; }

        public int TotalCells { get; set; }

        public int TotalFrontCells { get; set; }

        #endregion
    }
}
