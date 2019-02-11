using Ferretto.WMS.Data.Core.Interfaces.Base;
using Ferretto.WMS.Data.Core.Models;

namespace Ferretto.WMS.Data.Core.Interfaces
{
    public interface IItemListsProvider :
        IReadAllPagedAsyncProvider<ItemList, int>,
        IReadSingleAsyncProvider<ItemListDetails, int>,
        IUpdateAsyncProvider<ItemListDetails, int>,
        IGetUniqueValuesAsyncProvider
    {
    }
}
