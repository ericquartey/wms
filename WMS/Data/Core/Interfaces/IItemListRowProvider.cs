using System.Collections.Generic;
using System.Threading.Tasks;
using Ferretto.Common.BLL.Interfaces.Providers;
using Ferretto.WMS.Data.Core.Models;

namespace Ferretto.WMS.Data.Core.Interfaces
{
    public interface IItemListRowProvider :
        ICreateAsyncProvider<ItemListRowDetails, int>,
        IReadAllPagedAsyncProvider<ItemListRow, int>,
        IReadSingleAsyncProvider<ItemListRowDetails, int>,
        IUpdateAsyncProvider<ItemListRowDetails, int>,
        IGetUniqueValuesAsyncProvider,
        IDeleteAsyncProvider<ItemListRowDetails, int>
    {
        #region Methods

        Task<IEnumerable<ItemListRow>> GetByItemListIdAsync(int id);

        #endregion
    }
}
