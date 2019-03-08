using Ferretto.Common.BLL.Interfaces.Base;
using Ferretto.WMS.Scheduler.Core.Models;

namespace Ferretto.WMS.Scheduler.Core.Interfaces
{
    public interface ILoadingUnitSchedulerProvider :
        IUpdateAsyncProvider<LoadingUnit, int>,
        IReadSingleAsyncProvider<LoadingUnit, int>
    {
    }
}
