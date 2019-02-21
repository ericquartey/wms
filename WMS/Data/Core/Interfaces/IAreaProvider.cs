using System.Collections.Generic;
using System.Threading.Tasks;
using Ferretto.Common.BLL.Interfaces.Base;
using Ferretto.WMS.Data.Core.Models;

namespace Ferretto.WMS.Data.Core.Interfaces
{
    public interface IAreaProvider :
        IReadAllAsyncProvider<Area, int>,
        IReadSingleAsyncProvider<Area, int>
    {
        #region Methods

        Task<IEnumerable<Aisle>> GetAislesAsync(int id);

        Task<IEnumerable<Area>> GetByItemIdAvailabilityAsync(int id);

        #endregion
    }
}
