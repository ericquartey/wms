using Ferretto.Common.BLL.Interfaces.Providers;
using Ferretto.WMS.App.Core.Models;

namespace Ferretto.WMS.App.Core.Interfaces
{
    public interface ISchedulerRequestProvider :
        IPagedBusinessProvider<SchedulerRequest, int>,
        IReadSingleAsyncProvider<SchedulerRequest, int>
    {
    }
}
