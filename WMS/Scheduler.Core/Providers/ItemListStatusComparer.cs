using System.Collections.Generic;

namespace Ferretto.WMS.Scheduler.Core
{
    public class ItemListStatusComparer : IComparer<ListStatus>
    {
        #region Fields

        private static readonly Dictionary<ListStatus, int> priorities = new Dictionary<ListStatus, int>()
        {
            { ListStatus.NotSpecified, 0 },
            { ListStatus.Completed, 1 },
            { ListStatus.Waiting, 2 },
            { ListStatus.Suspended, 3 },
            { ListStatus.Incomplete, 4 },
            { ListStatus.Executing, 5 },
        };

        #endregion Fields

        #region Methods

        public int Compare(ListStatus x, ListStatus y)
        {
            return priorities[x] - priorities[y];
        }

        #endregion Methods
    }
}
