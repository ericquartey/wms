using Ferretto.Common.BLL.Interfaces.Providers;
using Ferretto.WMS.Data.Core.Models;

namespace Ferretto.WMS.Data.Core.Interfaces
{
    public interface IAisleProvider :
        IReadAllAsyncProvider<Aisle, int>,
        IReadSingleAsyncProvider<Aisle, int>
    {
    }
}
