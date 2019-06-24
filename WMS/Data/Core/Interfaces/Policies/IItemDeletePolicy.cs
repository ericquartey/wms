using Ferretto.Common.BLL.Interfaces.Models;

namespace Ferretto.WMS.Data.Core.Interfaces
{
    public interface IItemDeletePolicy : IModel<int>
    {
        #region Properties

        int CompartmentsCount { get; }

        int ItemListRowsCount { get; }

        int MissionOperationsCount { get; }

        int SchedulerRequestsCount { get; }

        #endregion
    }
}
