using System.Threading.Tasks;
using Ferretto.WMS.Scheduler.Core.Models;

namespace Ferretto.WMS.Scheduler.Core.Interfaces
{
    public interface ISchedulerService
    {
        #region Methods

        Task ProcessPendingRequestsAsync();

        Task<SchedulerRequest> WithdrawItemAsync(SchedulerRequest request);

        #endregion
    }
}
