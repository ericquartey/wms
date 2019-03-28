using System.Collections.Generic;
using System.Threading.Tasks;
using Ferretto.Common.BLL.Interfaces.Providers;
using Ferretto.WMS.Scheduler.Core.Models;

namespace Ferretto.WMS.Scheduler.Core.Interfaces
{
    public interface ISchedulerRequestProvider :
        IUpdateAsyncProvider<SchedulerRequest, int>,
        ICreateAsyncProvider<SchedulerRequest, int>
    {
        #region Methods

        Task<IEnumerable<SchedulerRequest>> CreateRangeAsync(IEnumerable<SchedulerRequest> models);

        Task<SchedulerRequest> FullyQualifyWithdrawalRequestAsync(int itemId, ItemWithdrawOptions options);

        Task<IEnumerable<SchedulerRequest>> GetRequestsToProcessAsync();

        #endregion
    }
}
