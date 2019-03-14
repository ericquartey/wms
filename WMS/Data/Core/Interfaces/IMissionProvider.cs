using Ferretto.Common.BLL.Interfaces.Providers;
using Ferretto.WMS.Data.Core.Models;

namespace Ferretto.WMS.Data.Core.Interfaces
{
    public interface IMissionProvider :
        IReadAllPagedAsyncProvider<Mission, int>,
        IReadSingleAsyncProvider<Mission, int>,
        IGetUniqueValuesAsyncProvider
    {
    }
}
