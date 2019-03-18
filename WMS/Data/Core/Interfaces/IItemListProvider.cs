using Ferretto.Common.BLL.Interfaces.Providers;
using Ferretto.WMS.Data.Core.Models;

namespace Ferretto.WMS.Data.Core.Interfaces
{
    public interface IItemListProvider :
        ICreateAsyncProvider<ItemListDetails, int>,
        IReadAllPagedAsyncProvider<ItemList, int>,
        IReadSingleAsyncProvider<ItemListDetails, int>,
        IUpdateAsyncProvider<ItemListDetails, int>,
        IGetUniqueValuesAsyncProvider
    {
    }
}
