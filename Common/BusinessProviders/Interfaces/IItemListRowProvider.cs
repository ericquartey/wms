using System.Collections.Generic;
using System.Threading.Tasks;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.BLL.Interfaces.Base;
using Ferretto.Common.BusinessModels;

namespace Ferretto.Common.BusinessProviders
{
    public interface IItemListRowProvider :
        IPagedBusinessProvider<ItemListRow, int>,
        IReadSingleAsyncProvider<ItemListRowDetails, int>,
        ICreateAsyncProvider<ItemListRowDetails, int>,
        IUpdateAsyncProvider<ItemListRowDetails, int>
    {
        #region Methods

        Task<IOperationResult<ItemListRow>> ExecuteImmediatelyAsync(int listRowId, int areaId, int bayId);

        Task<IEnumerable<BusinessModels.ItemListRow>> GetByItemListIdAsync(int id);

        Task<IOperationResult<ItemListRow>> ScheduleForExecutionAsync(int listRowId, int areaId);

        #endregion
    }
}
