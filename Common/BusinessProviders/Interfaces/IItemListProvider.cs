using System.Linq;
using System.Threading.Tasks;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.BLL.Interfaces.Base;
using Ferretto.Common.BusinessModels;

namespace Ferretto.Common.BusinessProviders
{
    public interface IItemListProvider :
        ICreateAsyncProvider<ItemListDetails>,
        IPagedBusinessProvider<ItemList>,
        IReadSingleAsyncProvider<ItemListDetails, int>,
        IUpdateAsyncProvider<ItemListDetails>
    {
        #region Methods

        Task<IOperationResult<ItemList>> ExecuteImmediatelyAsync(int listId, int areaId, int bayId);

        ItemListDetails GetNew();

        Task<IOperationResult<ItemList>> ScheduleForExecutionAsync(int listId, int areaId);

        #endregion
    }
}
