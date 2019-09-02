using System.Collections.Generic;

namespace Ferretto.VW.MAS.DataModels
{
    public class CellPanel
    {
        #region Properties

        public IEnumerable<Cell> Cells { get; set; }

        public int Id { get; set; }

        public WarehouseSide Side { get; set; }

        #endregion
    }
}
