using Ferretto.Common.BLL.Interfaces.Base;
using Ferretto.WMS.Data.Core.Models;

namespace Ferretto.WMS.Data.Core.Interfaces
{
    public interface IAisleProvider :
        IReadAllAsyncProvider<Aisle, int>,
        IReadSingleAsyncProvider<Aisle, int>
    {
    }
}
