using Ferretto.Common.BLL.Interfaces.Models;
using Ferretto.WMS.Data.Core.Models;

namespace Ferretto.WMS.Data.Core.Interfaces
{
    public interface IItemListRowDeletePolicy : IModel<int>
    {
        #region Properties

        int ActiveMissionsCount { get; }

        int ActiveSchedulerRequestsCount { get; }

        ItemListRowStatus Status { get; }

        #endregion
    }
}
