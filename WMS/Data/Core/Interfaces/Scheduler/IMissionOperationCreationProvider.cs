using System.Collections.Generic;
using System.Threading.Tasks;
using Ferretto.WMS.Data.Core.Models;

namespace Ferretto.WMS.Data.Core.Interfaces
{
    public interface IMissionOperationCreationProvider
    {
        #region Methods

        Task<IEnumerable<MissionOperation>> CreateForRequestsAsync(IEnumerable<ISchedulerRequest> requests);

        Task<IEnumerable<MissionOperation>> CreatePickOperationsAsync(ItemSchedulerRequest request);

        Task<IEnumerable<MissionOperation>> CreatePutOperationsAsync(ItemSchedulerRequest request);

        #endregion
    }
}
