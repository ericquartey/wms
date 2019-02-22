using Ferretto.Common.BLL.Interfaces.Base;
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
