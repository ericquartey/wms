using System.Collections.Generic;
using System.Threading.Tasks;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.BLL.Interfaces.Providers;
using Ferretto.WMS.Data.Core.Models;

namespace Ferretto.WMS.Data.Core.Interfaces
{
    public interface IAreaProvider :
        IReadAllAsyncProvider<Area, int>,
        IReadSingleAsyncProvider<Area, int>
    {
        #region Methods

        Task<IOperationResult<ItemArea>> CreateAllowedByItemIdAsync(int id, int itemId);

        Task<IOperationResult<ItemArea>> DeleteAllowedByItemIdAsync(int id, int itemId);

        Task<IEnumerable<Aisle>> GetAislesAsync(int id);

        Task<IEnumerable<AllowedItemArea>> GetAllowedByItemIdAsync(int id);

        Task<AreaAvailable> GetByIdForExecutionAsync(int id);

        Task<IEnumerable<Area>> GetByItemIdAsync(int id);

        Task<IEnumerable<Area>> GetByItemIdAvailabilityAsync(int id);

        #endregion
    }
}
