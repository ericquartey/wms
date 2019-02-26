using System;

namespace Ferretto.VW.MAS_DataLayer
{
    public class FreeBlock
    {
        #region Properties

        public int BlockSize { get; set; }

        public int BookedCellsNumber { get; set; }

        public decimal Coord { get; set; }

        public int FreeBlockId { get; set; }

        public int Priority { get; set; }

        public Side Side { get; set; }

        public int StartCell { get; set; }

        #endregion
    }
}
