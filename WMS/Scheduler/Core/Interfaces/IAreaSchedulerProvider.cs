using Ferretto.Common.BLL.Interfaces.Providers;
using Ferretto.WMS.Scheduler.Core.Models;

namespace Ferretto.WMS.Scheduler.Core.Interfaces
{
    public interface IAreaSchedulerProvider
        : IReadSingleAsyncProvider<Area, int>
    {
    }
}
