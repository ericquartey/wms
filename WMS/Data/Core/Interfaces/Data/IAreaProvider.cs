using System.Collections.Generic;
using System.Threading.Tasks;
using Ferretto.Common.BLL.Interfaces.Providers;
using Ferretto.WMS.Data.Core.Models;

namespace Ferretto.WMS.Data.Core.Interfaces
{
    public interface IAreaProvider :
        IReadAllAsyncProvider<Area, int>,
        IReadSingleAsyncProvider<Area, int>
    {
        #region Methods

        Task<IEnumerable<Aisle>> GetAislesAsync(int id);

        Task<AreaAvailable> GetByIdForExecutionAsync(int id);

        Task<IEnumerable<Area>> GetByItemIdAsync(int id);

        Task<IEnumerable<Area>> GetByItemIdAvailabilityAsync(int id);

        #endregion
    }
}
