using System.Threading.Tasks;

namespace Ferretto.WMS.Scheduler.Core
{
    public interface IWarehouse
    {
        #region Methods

        Task<SchedulerRequest> WithdrawAsync(SchedulerRequest request);

        #endregion
    }
}
