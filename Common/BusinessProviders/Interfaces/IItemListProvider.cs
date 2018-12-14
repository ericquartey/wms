using System.Linq;
using System.Threading.Tasks;
using Ferretto.Common.BusinessModels;

namespace Ferretto.Common.BusinessProviders
{
    public interface IItemListProvider : IBusinessProvider<ItemList, ItemListDetails>
    {
        #region Methods

        Task<OperationResult> ExecuteImmediately(int areaId, int bayId);

        IQueryable<ItemList> GetWithStatusCompleted();

        int GetWithStatusCompletedCount();

        IQueryable<ItemList> GetWithStatusWaiting();

        int GetWithStatusWaitingCount();

        IQueryable<ItemList> GetWithTypePick();

        int GetWithTypePickCount();

        Task<OperationResult> ScheduleForExecution(int areaId);

        #endregion Methods
    }
}
