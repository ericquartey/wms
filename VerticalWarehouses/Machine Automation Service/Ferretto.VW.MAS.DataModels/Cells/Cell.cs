using Ferretto.VW.MAS.DataModels.LoadingUnits;

namespace Ferretto.VW.MAS.DataModels.Cells
{
    public sealed class Cell
    {
        #region Properties

        public decimal Coord { get; set; }

        public int Id { get; set; }

        public LoadingUnit LoadingUnit { get; set; }

        public int Priority { get; set; }

        public CellSide Side { get; set; }

        public CellStatus Status { get; set; }

        public CellStatus WorkingStatus { get; set; }

        #endregion
    }
}
