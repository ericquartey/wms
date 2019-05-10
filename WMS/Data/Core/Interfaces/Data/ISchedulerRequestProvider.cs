using Ferretto.Common.BLL.Interfaces.Providers;
using Ferretto.WMS.Data.Core.Models;

namespace Ferretto.WMS.Data.Core.Interfaces
{
    public interface ISchedulerRequestProvider :
        IReadAllPagedAsyncProvider<SchedulerRequest, int>,
        IReadSingleAsyncProvider<SchedulerRequest, int>,
        IGetUniqueValuesAsyncProvider
    {
    }
}
