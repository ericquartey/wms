using System.Collections.Generic;

namespace Ferretto.WMS.Scheduler.Core
{
    public class ItemListRowStatusComparer : IComparer<ListRowStatus>
    {
        #region Fields

        private static readonly Dictionary<ListRowStatus, int> priorities = new Dictionary<ListRowStatus, int>()
        {
            { ListRowStatus.NotSpecified, 0 },
            { ListRowStatus.Completed, 1 },
            { ListRowStatus.Waiting, 2 },
            { ListRowStatus.Suspended, 3 },
            { ListRowStatus.Incomplete, 4 },
            { ListRowStatus.Executing, 5 },
        };

        #endregion Fields

        #region Methods

        public int Compare(ListRowStatus x, ListRowStatus y)
        {
            return priorities[x] - priorities[y];
        }

        #endregion Methods
    }
}
