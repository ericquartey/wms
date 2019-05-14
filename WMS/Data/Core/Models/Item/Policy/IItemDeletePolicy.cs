using Ferretto.Common.BLL.Interfaces.Models;

namespace Ferretto.WMS.Data.Core.Models
{
    public interface IItemDeletePolicy : IModel<int>
    {
        #region Properties

        int CompartmentsCount { get; }

        int ItemListRowsCount { get; }

        int MissionsCount { get; }

        int SchedulerRequestsCount { get; }

        #endregion
    }
}
