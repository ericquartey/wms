using System.Collections.Generic;
using Ferretto.VW.MAS.DataLayer.Enumerations;

namespace Ferretto.VW.MAS.DataLayer.Models
{
    public class CellStatistics
    {
        #region Properties

        public double CellOccupationRatio { get; set; }

        public IEnumerable<CellStatusStatistic> CellStatusStatistics { get; set; }

        public int TotalBackCells { get; set; }

        public int TotalCells { get; set; }

        public int TotalFrontCells { get; set; }

        #endregion
    }

    public class CellStatusStatistic
    {
        #region Properties

        public double RatioBackCells { get; set; }

        public double RatioFrontCells { get; set; }

        public Status Status { get; set; }

        public int TotalBackCells { get; set; }

        public int TotalFrontCells { get; set; }

        #endregion
    }
}
