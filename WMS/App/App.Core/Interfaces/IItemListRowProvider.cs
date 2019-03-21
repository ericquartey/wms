using System.Collections.Generic;
using System.Threading.Tasks;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.BLL.Interfaces.Providers;
using Ferretto.WMS.App.Core.Models;

namespace Ferretto.WMS.App.Core.Interfaces
{
    public interface IItemListRowProvider :
        IPagedBusinessProvider<ItemListRow, int>,
        IReadSingleAsyncProvider<ItemListRowDetails, int>,
        ICreateAsyncProvider<ItemListRowDetails, int>,
        IUpdateAsyncProvider<ItemListRowDetails, int>,
        IDeleteAsyncProvider<ItemListRowDetails, int>
    {
        #region Methods

        Task<ActionModel> CanDeleteAsync(int id);

        Task<IOperationResult<ItemListRow>> ExecuteImmediatelyAsync(int listRowId, int areaId, int bayId);

        Task<IEnumerable<ItemListRow>> GetByItemListIdAsync(int id);

        Task<IOperationResult<ItemListRow>> ScheduleForExecutionAsync(int listRowId, int areaId);

        #endregion
    }
}
