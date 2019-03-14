using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.BLL.Interfaces.Providers;
using Ferretto.Common.BusinessModels;

namespace Ferretto.Common.BusinessProviders
{
    public interface ISchedulerRequestProvider :
        IPagedBusinessProvider<SchedulerRequest, int>,
        IReadSingleAsyncProvider<SchedulerRequest, int>
    {
    }
}
