using Ferretto.Common.BLL.Interfaces.Base;
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
