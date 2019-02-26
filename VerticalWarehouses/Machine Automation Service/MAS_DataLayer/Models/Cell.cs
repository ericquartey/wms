using System;

namespace Ferretto.VW.MAS_DataLayer
{
    public class Cell
    {
        #region Properties

        public Int32 CellId { get; set; }

        public Decimal Coord { get; set; }

        public Int32 Priority { get; set; }

        public Side Side { get; set; }

        public Status Status { get; set; }

        #endregion
    }
}
