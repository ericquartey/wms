using Ferretto.VW.MAS.DataModels;

namespace Ferretto.VW.MAS.DataLayer
{
    public sealed class CellStatusStatistics
    {
        #region Properties

        public bool IsFree { get; set; }

        public double RatioBackCells { get; set; }

        public double RatioFrontCells { get; set; }

        public int TotalBackCells { get; set; }

        public int TotalFrontCells { get; set; }

        #endregion
    }
}
