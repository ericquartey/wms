using System;
using System.Collections.Generic;
using System.Text;

namespace Ferretto.VW.MAS.DataModels
{
    public class Panel
    {
        #region Properties

        public IEnumerable<Cell> Cells { get; set; }

        public int Id { get; set; }

        public WarehouseSide Side { get; set; }

        #endregion
    }
}
