using Ferretto.Common.BLL.Interfaces.Providers;
using Ferretto.WMS.Scheduler.Core.Models;

namespace Ferretto.WMS.Scheduler.Core.Interfaces
{
    public interface ILoadingUnitSchedulerProvider :
        IUpdateAsyncProvider<LoadingUnit, int>,
        IReadSingleAsyncProvider<LoadingUnit, int>
    {
    }
}
