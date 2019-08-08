using Ferretto.Common.BLL.Interfaces.Models;
using Enums = Ferretto.Common.Resources.Enums;

namespace Ferretto.WMS.Data.Core.Interfaces
{
    public interface IItemListRowDeletePolicy : IModel<int>
    {
        #region Properties

        int ActiveMissionsCount { get; }

        int ActiveSchedulerRequestsCount { get; }

        Enums.ItemListRowStatus Status { get; }

        #endregion
    }
}
