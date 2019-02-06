using Ferretto.WMS.Data.Core.Interfaces.Base;
using Ferretto.WMS.Data.Core.Models;

namespace Ferretto.WMS.Data.Core.Interfaces
{
    public interface IItemProvider :
        IReadAllPagedProvider<Item>,
        IReadSingleProvider<ItemDetails, int>,
        IUpdateProvider<Item>,
        IGetUniqueValuesProvider
    {
    }
}
