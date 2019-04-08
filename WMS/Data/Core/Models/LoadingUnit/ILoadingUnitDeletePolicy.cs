using Ferretto.Common.BLL.Interfaces.Models;

namespace Ferretto.WMS.Data.Core.Models
{
    public interface ILoadingUnitDeletePolicy : IModel<int>
    {
        #region Properties

        int ActiveMissionsCount { get; }

        int ActiveSchedulerRequestsCount { get; }

        int CompartmentsCount { get; }

        #endregion
    }
}
