using System.Collections.Generic;

namespace Ferretto.WMS.Scheduler.Core
{
    public class ItemList : BusinessObject
    {
        #region Properties

        public IEnumerable<ItemListRow> Rows { get; set; }

        public ListStatus Status { get; set; }

        #endregion
    }
}
