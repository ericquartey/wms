using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ferretto.WMS.Scheduler.Core.Interfaces
{
    public interface IMissionSchedulerProvider
    {
        #region Methods

        Task<IEnumerable<Mission>> CreateForPendingRequestsAsync();

        #endregion
    }
}
