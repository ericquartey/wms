using System.Collections.Generic;
using Ferretto.VW.MAS.DataLayer.Enumerations;

namespace Ferretto.VW.MAS.DataLayer.Models
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

        public decimal MaxWeight { get; set; }

        public LoadingUnitStatus Status { get; set; }

        public decimal Weight { get; set; }

        #endregion
    }
}
