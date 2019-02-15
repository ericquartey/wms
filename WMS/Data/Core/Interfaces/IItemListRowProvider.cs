using System.Collections.Generic;
using System.Threading.Tasks;
using Ferretto.WMS.Data.Core.Interfaces.Base;
using Ferretto.WMS.Data.Core.Models;

namespace Ferretto.WMS.Data.Core.Interfaces
{
    public interface IItemListRowProvider :
        ICreateAsyncProvider<ItemListRowDetails, int>,
        IReadAllPagedAsyncProvider<ItemListRow, int>,
        IReadSingleAsyncProvider<ItemListRowDetails, int>,
        IUpdateAsyncProvider<ItemListRowDetails, int>,
        IGetUniqueValuesAsyncProvider
    {
        Task<IEnumerable<ItemListRow>> GetByItemListIdAsync(int id);
    }
}
