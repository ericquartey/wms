using System.Linq;
using System.Threading.Tasks;
using Ferretto.Common.BusinessModels;

namespace Ferretto.Common.BusinessProviders
{
    public interface IItemListRowProvider : IBusinessProvider<ItemListRow, ItemListRowDetails>
    {
        #region Methods

        Task<OperationResult> ExecuteImmediatelyAsync(int listRowId, int areaId, int bayId);

        IQueryable<ItemListRow> GetByItemListId(int id);

        Task<OperationResult> ScheduleForExecutionAsync(int listRowId, int areaId);

        #endregion
    }
}
