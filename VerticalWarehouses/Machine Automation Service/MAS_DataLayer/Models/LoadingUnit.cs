using System.Collections.Generic;

namespace Ferretto.VW.MAS_DataLayer
{
    public class LoadingUnit
    {
        #region Properties

        public decimal CellPosition { get; set; }

        public ICollection<Cell> Cells { get; set; }

        public ICollection<FreeBlock> FreeBlocks { get; set; }

        public decimal Height { get; set; }

        public int LoadingUnitId { get; set; }

        public LoadingUnitStatus Status { get; set; }

        public decimal Weight { get; set; }

        #endregion
    }
}
