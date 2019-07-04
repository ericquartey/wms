using System.Collections.Generic;
using System.Threading.Tasks;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.WMS.Data.Core.Models;

namespace Ferretto.WMS.Data.Core.Interfaces
{
    public interface IItemListSchedulerService
    {
        #region Methods

        Task<IOperationResult<IEnumerable<ItemListRowSchedulerRequest>>> ExecuteListAsync(int listId, int areaId, int? bayId);

        Task<IOperationResult<IEnumerable<ItemListRowSchedulerRequest>>> ExecuteListRowAsync(int rowId, int areaId, int? bayId);

        Task<IOperationResult<ItemList>> SuspendListAsync(int listId);

        Task<IOperationResult<ItemListRow>> SuspendListRowAsync(int listId);

        #endregion
    }
}
