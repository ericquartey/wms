using Ferretto.Common.BLL.Interfaces.Models;

namespace Ferretto.WMS.Data.Core.Interfaces
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
