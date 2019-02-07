using Ferretto.WMS.Data.Core.Interfaces.Base;
using Ferretto.WMS.Data.Core.Models;

namespace Ferretto.WMS.Data.Core.Interfaces
{
    public interface IItemListsProvider :
        IReadAllPagedProvider<ItemList>,
        IReadSingleProvider<ItemListDetails, int>,
        IUpdateProvider<ItemListDetails>,
        IGetUniqueValuesProvider
    {
    }
}
