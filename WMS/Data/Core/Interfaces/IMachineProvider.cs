using Ferretto.Common.BLL.Interfaces.Providers;
using Ferretto.WMS.Data.Core.Models;

namespace Ferretto.WMS.Data.Core.Interfaces
{
    public interface IMachineProvider :
        IReadAllPagedAsyncProvider<Machine, int>,
        IReadSingleAsyncProvider<Machine, int>,
        IGetUniqueValuesAsyncProvider
    {
    }
}
