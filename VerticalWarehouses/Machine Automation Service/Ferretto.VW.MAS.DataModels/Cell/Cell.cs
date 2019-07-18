namespace Ferretto.VW.MAS.DataModels
{
    public sealed class Cell
    {
        #region Properties

        public int CellId { get; set; }

        public decimal Coord { get; set; }

        public LoadingUnit LoadingUnit { get; set; }

        public int LoadingUnitId { get; set; }

        public int Priority { get; set; }

        public CellSide Side { get; set; }

        public CellStatus Status { get; set; }

        public CellStatus WorkingStatus { get; set; }

        #endregion
    }
}
