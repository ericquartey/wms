using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.BLL.Interfaces.Base;
using Ferretto.Common.BusinessModels;

namespace Ferretto.Common.BusinessProviders
{
    public interface IItemListRowProvider :
        IPagedBusinessProvider<ItemListRow>,
        IReadSingleAsyncProvider<ItemListRowDetails, int>,
        ICreateAsyncProvider<ItemListRowDetails>,
        IUpdateAsyncProvider<ItemListRowDetails>
    {
        #region Methods

        Task<OperationResult> ExecuteImmediatelyAsync(int listRowId, int areaId, int bayId);

        Task<IEnumerable<BusinessModels.ItemListRow>> GetByItemListIdAsync(int id);

        Task<OperationResult> ScheduleForExecutionAsync(int listRowId, int areaId);

        #endregion
    }
}
