using System.Collections.Generic;
using System.Threading.Tasks;
using Ferretto.Common.BLL.Interfaces.Providers;
using Ferretto.WMS.Data.Core.Models;

namespace Ferretto.WMS.Data.Core.Interfaces
{
    public interface ISchedulerRequestExecutionProvider :
        ICreateAsyncProvider<ItemSchedulerRequest, int>,
        IUpdateAsyncProvider<ItemSchedulerRequest, int>,
        IUpdateAsyncProvider<LoadingUnitSchedulerRequest, int>,
        ICreateAsyncProvider<ItemListRowSchedulerRequest, int>,
        ICreateAsyncProvider<LoadingUnitSchedulerRequest, int>
    {
        #region Methods

        Task<IEnumerable<ItemSchedulerRequest>> CreateRangeAsync(IEnumerable<ItemSchedulerRequest> models);

        Task<IEnumerable<ISchedulerRequest>> GetRequestsToProcessAsync();

        #endregion
    }
}
