using System.Linq;
using System.Threading.Tasks;
using Ferretto.Common.BusinessModels;

namespace Ferretto.Common.BusinessProviders
{
    public interface IItemListProvider : IBusinessProvider<ItemList, ItemListDetails>
    {
        #region Methods

        Task<OperationResult> ExecuteImmediately(int listId, int areaId, int bayId);

        IQueryable<ItemList> GetWithStatusCompleted(ItemListType? type);

        int GetWithStatusCompletedCount(ItemListType? type);

        IQueryable<ItemList> GetWithStatusWaiting(ItemListType? type);

        int GetWithStatusWaitingCount(ItemListType? type);

        IQueryable<ItemList> GetWithTypeInventory();

        int GetWithTypeInventoryCount();

        IQueryable<ItemList> GetWithTypePick();

        int GetWithTypePickCount();

        IQueryable<ItemList> GetWithTypePut();

        int GetWithTypePutCount();

        Task<OperationResult> ScheduleForExecution(int listId, int areaId);

        #endregion Methods
    }
}
