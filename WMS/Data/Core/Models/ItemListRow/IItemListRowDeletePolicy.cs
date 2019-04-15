using Ferretto.Common.BLL.Interfaces.Models;

namespace Ferretto.WMS.Data.Core.Models
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
