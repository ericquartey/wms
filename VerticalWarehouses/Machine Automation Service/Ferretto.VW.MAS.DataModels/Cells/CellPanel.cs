using System;
using System.Collections.Generic;
using System.Linq;

namespace Ferretto.VW.MAS.DataModels
{
    public sealed class CellPanel : DataModel, IValidable
    {
        #region Properties

        public IEnumerable<Cell> Cells { get; set; }

        public bool IsChecked { get; set; }

        public WarehouseSide Side { get; set; }

        #endregion

        #region Methods

        public void Validate()
        {
            this.Cells.ForEach(c => c.Validate());
        }

        #endregion
    }
}
