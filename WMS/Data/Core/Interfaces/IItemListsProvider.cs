using Ferretto.WMS.Data.Core.Interfaces.Base;
using Ferretto.WMS.Data.Core.Models;

namespace Ferretto.WMS.Data.Core.Interfaces
{
    public interface IItemListsProvider :
        IReadAllPagedAsyncProvider<ItemList>,
        IReadSingleAsyncProvider<ItemListDetails, int>,
        IUpdateAsyncProvider<ItemListDetails>,
        IGetUniqueValuesAsyncProvider
    {
    }
}
