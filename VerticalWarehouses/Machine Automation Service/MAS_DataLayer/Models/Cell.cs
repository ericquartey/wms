namespace Ferretto.VW.MAS_DataLayer
{
    public class Cell
    {
        #region Properties

        public int CellId { get; set; }

        public decimal Coord { get; set; }

        public int Priority { get; set; }

        public Side Side { get; set; }

        public Status Status { get; set; }

        #endregion
    }
}
