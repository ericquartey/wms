using System.Linq;
using System.Threading.Tasks;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.BusinessModels;

namespace Ferretto.Common.BusinessProviders
{
    public interface IItemListProvider : IBusinessProvider<ItemList, ItemListDetails, int>
    {
        #region Methods

        Task<IOperationResult<ItemList>> ExecuteImmediatelyAsync(int listId, int areaId, int bayId);

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

        Task<IOperationResult<ItemList>> ScheduleForExecutionAsync(int listId, int areaId);

        #endregion
    }
}
