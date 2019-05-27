using System.Collections.Generic;
using System.Threading.Tasks;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.BLL.Interfaces.Providers;
using Ferretto.WMS.App.Core.Models;

namespace Ferretto.WMS.App.Core.Interfaces
{
    public interface IAreaProvider :
        IReadAllAsyncProvider<Area, int>,
        IReadSingleAsyncProvider<Area, int>
    {
        #region Methods

        Task<IOperationResult<ItemArea>> CreateAllowedByItemIdAsync(int id, int itemId);

        Task<IOperationResult<AllowedItemArea>> DeleteAllowedByItemIdAsync(int id, int itemId);

        Task<IOperationResult<IEnumerable<AllowedItemArea>>> GetAllowedByItemIdAsync(int id);

        Task<IOperationResult<IEnumerable<Area>>> GetAreasWithAvailabilityAsync(int id);

        Task<IOperationResult<IEnumerable<Area>>> GetByItemIdAsync(int id);

        #endregion
    }
}
