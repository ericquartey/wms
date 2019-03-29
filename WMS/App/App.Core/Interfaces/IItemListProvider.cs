using System.Threading.Tasks;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.BLL.Interfaces.Providers;
using Ferretto.WMS.App.Core.Models;

namespace Ferretto.WMS.App.Core.Interfaces
{
    public interface IItemListProvider :
        ICreateAsyncProvider<ItemListDetails, int>,
        IPagedBusinessProvider<ItemList, int>,
        IReadSingleAsyncProvider<ItemListDetails, int>,
        IUpdateAsyncProvider<ItemListDetails, int>
    {
        #region Methods

        Task<IOperationResult<ItemList>> ExecuteImmediatelyAsync(int listId, int areaId, int bayId);

        ItemListDetails GetNew();

        Task<IOperationResult<ItemList>> ScheduleForExecutionAsync(int listId, int areaId);

        #endregion
    }
}
