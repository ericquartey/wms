using System.Collections.Generic;

namespace Ferretto.VW.MAS.DataModels
{
    public class LoadingUnit
    {
        #region Properties

        public decimal CellPosition { get; set; }

        public ICollection<Cell> Cells { get; set; }

        public string Description { get; set; }

        public ICollection<FreeBlock> FreeBlocks { get; set; }

        public decimal Height { get; set; }

        public int LoadingUnitId { get; set; }

        public decimal MaxGrossWeight { get; set; }

        public LoadingUnitStatus Status { get; set; }

        public decimal Weight { get; set; }

        #endregion
    }
}
