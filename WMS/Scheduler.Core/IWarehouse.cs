using System.Threading.Tasks;
using Ferretto.Common.BusinessModels;

namespace Ferretto.WMS.Scheduler.Core
{
    public interface IWarehouse
    {
        #region Methods

        Task<SchedulerRequest> Withdraw(SchedulerRequest request);

        #endregion Methods
    }
}
