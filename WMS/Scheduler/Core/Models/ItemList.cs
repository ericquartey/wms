using System.Collections.Generic;

namespace Ferretto.WMS.Scheduler.Core
{
    public class ItemList : Model
    {
        #region Properties

        public string Code { get; set; }

        public IEnumerable<ItemListRow> Rows { get; set; }

        public ListStatus Status { get; set; }

        #endregion
    }
}
