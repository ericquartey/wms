using System.Collections.Generic;
using System.Linq;

namespace Ferretto.WMS.Scheduler.Core.Models
{
    public class ItemList : Model
    {
        #region Properties

        public string Code { get; set; }

        public IEnumerable<ItemListRow> Rows { get; set; }

        public ListStatus Status
        {
            get
            {
                if (this.Rows.All(r => r.Status == ListRowStatus.Completed))
                {
                    return ListStatus.Completed;
                }

                if (this.Rows.Any(r => r.Status == ListRowStatus.Executing))
                {
                    return ListStatus.Executing;
                }

                if (this.Rows.Any(r => r.Status == ListRowStatus.Suspended))
                {
                    return ListStatus.Waiting;
                }

                if (this.Rows.Any(r => r.Status == ListRowStatus.Incomplete))
                {
                    return ListStatus.Incomplete;
                }

                if (this.Rows.Any(r => r.Status == ListRowStatus.Waiting))
                {
                    return ListStatus.Waiting;
                }

                return ListStatus.NotSpecified;
            }
        }

        #endregion
    }
}
