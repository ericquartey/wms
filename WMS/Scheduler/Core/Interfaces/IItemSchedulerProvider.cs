using Ferretto.Common.BLL.Interfaces.Base;
using Ferretto.WMS.Scheduler.Core.Models;

namespace Ferretto.WMS.Scheduler.Core.Interfaces
{
    public interface IItemSchedulerProvider
        : IUpdateAsyncProvider<Item, int>,
        IReadSingleAsyncProvider<Item, int>
    {
    }
}
