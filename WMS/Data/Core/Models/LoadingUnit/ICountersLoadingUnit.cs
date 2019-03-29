using Ferretto.Common.BLL.Interfaces.Models;

namespace Ferretto.WMS.Data.Core.Models
{
    public interface ICountersLoadingUnit : IModel<int>
    {
        #region Properties

        int CompartmentsCount { get; set; }

        int MissionsCount { get; set; }

        int SchedulerRequestsCount { get; set; }

        #endregion
    }
}
