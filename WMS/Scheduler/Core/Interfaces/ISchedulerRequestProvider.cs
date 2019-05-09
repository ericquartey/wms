using System.Collections.Generic;
using System.Threading.Tasks;
using Ferretto.Common.BLL.Interfaces.Models;
using Ferretto.Common.BLL.Interfaces.Providers;
using Ferretto.WMS.Scheduler.Core.Models;

namespace Ferretto.WMS.Scheduler.Core.Interfaces
{
    public interface ISchedulerRequestProvider :
        IUpdateAsyncProvider<ItemSchedulerRequest, int>,
        IUpdateAsyncProvider<LoadingUnitSchedulerRequest, int>,
        ICreateAsyncProvider<ItemSchedulerRequest, int>,
        ICreateAsyncProvider<ItemListRowSchedulerRequest, int>,
        ICreateAsyncProvider<LoadingUnitSchedulerRequest, int>
    {
        #region Methods

        Task<IEnumerable<ItemSchedulerRequest>> CreateRangeAsync(IEnumerable<ItemSchedulerRequest> models);

        Task<ItemSchedulerRequest> FullyQualifyWithdrawalRequestAsync(
            int itemId,
            ItemWithdrawOptions options,
            ItemListRow row = null,
            int? previousRowRequestPriority = null);

        Task<IEnumerable<ISchedulerRequest>> GetRequestsToProcessAsync();

        #endregion
    }
}
