using System.Linq;
using System.Threading.Tasks;
using Ferretto.Common.BusinessModels;

namespace Ferretto.Common.BusinessProviders
{
    public interface IItemListRowProvider : IBusinessProvider<ItemListRow, ItemListRowDetails>
    {
        #region Methods

        Task<OperationResult> ExecuteImmediately(int listRowId, int areaId, int bayId);

        IQueryable<ItemListRow> GetByItemListId(int id);

        Task<OperationResult> ScheduleForExecution(int listRowId, int areaId);

        #endregion Methods
    }
}
