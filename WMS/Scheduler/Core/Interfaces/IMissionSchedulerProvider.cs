using System.Collections.Generic;
using System.Threading.Tasks;
using Ferretto.WMS.Scheduler.Core.Models;

namespace Ferretto.WMS.Scheduler.Core.Interfaces
{
    public interface IMissionSchedulerProvider
    {
        #region Methods

        Task<IEnumerable<Mission>> CreateForRequestsAsync(IEnumerable<SchedulerRequest> requests);

        #endregion
    }
}
