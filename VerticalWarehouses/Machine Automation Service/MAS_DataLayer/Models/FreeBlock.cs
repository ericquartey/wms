using System;

namespace Ferretto.VW.MAS_DataLayer
{
    public class FreeBlock
    {
        #region Properties

        public Int32 BlockSize { get; set; }

        public Int32 BookedCellsNumber { get; set; }

        public Decimal Coord { get; set; }

        public Int32 FreeBlockId { get; set; }

        public Int32 Priority { get; set; }

        public Side Side { get; set; }

        public Int32 StartCell { get; set; }

        #endregion
    }
}
