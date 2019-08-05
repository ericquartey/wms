namespace Ferretto.VW.MAS.DataModels
{
    public sealed class CellStatusStatistics
    {
        #region Properties

        public double RatioBackCells { get; set; }

        public double RatioFrontCells { get; set; }

        public CellStatus Status { get; set; }

        public int TotalBackCells { get; set; }

        public int TotalFrontCells { get; set; }

        #endregion
    }
}
