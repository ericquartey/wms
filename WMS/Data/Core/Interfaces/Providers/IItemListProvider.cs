using Ferretto.Common.BLL.Interfaces.Base;
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
