using System.Linq;
using System.Threading.Tasks;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.BusinessModels;

namespace Ferretto.Common.BusinessProviders
{
    public interface IItemListRowProvider : IBusinessProvider<ItemListRow, ItemListRowDetails, int>
    {
        #region Methods

        Task<IOperationResult<ItemListRow>> ExecuteImmediatelyAsync(int listRowId, int areaId, int bayId);

        IQueryable<ItemListRow> GetByItemListId(int id);

        Task<IOperationResult<ItemListRow>> ScheduleForExecutionAsync(int listRowId, int areaId);

        #endregion
    }
}
