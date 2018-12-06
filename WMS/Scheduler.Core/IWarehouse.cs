using System.Threading.Tasks;

namespace Ferretto.WMS.Scheduler.Core
{
    public interface IWarehouse
    {
        #region Methods

        Task<SchedulerRequest> ExecuteListAsync(int listId, int bayId);

        Task<SchedulerRequest> Withdraw(SchedulerRequest request);

        #endregion Methods
    }
}
