﻿using Ferretto.VW.MAS.DataModels.LoadingUnits;

namespace Ferretto.VW.MAS.DataModels.Cells
{
    public class FreeBlock
    {
        #region Properties

        public int BlockSize { get; set; }

        public int BookedCellsNumber { get; set; }

        public decimal Coord { get; set; }

        public int FreeBlockId { get; set; }

        public LoadingUnit LoadingUnit { get; set; }

        public int LoadingUnitId { get; set; }

        public int Priority { get; set; }

        public CellSide Side { get; set; }

        public int StartCell { get; set; }

        #endregion
    }
}
