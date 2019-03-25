using Ferretto.Common.BLL.Interfaces.Providers;
using Ferretto.WMS.Data.Core.Models;

namespace Ferretto.WMS.Data.Core.Interfaces
{
    public interface IItemProvider :
        ICreateAsyncProvider<ItemDetails, int>,
        IReadAllPagedAsyncProvider<Item, int>,
        IReadSingleAsyncProvider<ItemDetails, int>,
        IUpdateAsyncProvider<ItemDetails, int>,
        IGetUniqueValuesAsyncProvider,
        IDeleteAsyncProvider<ItemDetails, int>
    {
    }
}
