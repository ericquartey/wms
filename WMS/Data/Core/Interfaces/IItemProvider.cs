using Ferretto.WMS.Data.Core.Interfaces.Base;
using Ferretto.WMS.Data.Core.Models;

namespace Ferretto.WMS.Data.Core.Interfaces
{
    public interface IItemProvider :
        ICreateAsyncProvider<ItemDetails>,
        IReadAllPagedAsyncProvider<Item>,
        IReadSingleAsyncProvider<ItemDetails, int>,
        IUpdateAsyncProvider<ItemDetails>,
        IGetUniqueValuesAsyncProvider
    {
    }
}
