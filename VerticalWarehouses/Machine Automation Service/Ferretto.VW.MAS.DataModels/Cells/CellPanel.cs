using System.Collections.Generic;

namespace Ferretto.VW.MAS.DataModels
{
    public sealed class CellPanel : DataModel
    {
        #region Properties

        public IEnumerable<Cell> Cells { get; set; }

        public Machine Machine { get; set; }

        public WarehouseSide Side { get; set; }

        #endregion
    }
}
