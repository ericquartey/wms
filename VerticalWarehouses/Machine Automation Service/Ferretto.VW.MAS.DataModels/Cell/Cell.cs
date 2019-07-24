namespace Ferretto.VW.MAS.DataModels.Cell
{
    public sealed class Cell
    {
        #region Properties

        public int Id { get; set; }

        public decimal Coord { get; set; }

        public LoadingUnit.LoadingUnit LoadingUnit { get; set; }

        public int Priority { get; set; }

        public CellSide Side { get; set; }

        public CellStatus Status { get; set; }

        public CellStatus WorkingStatus { get; set; }

        #endregion
    }
}
