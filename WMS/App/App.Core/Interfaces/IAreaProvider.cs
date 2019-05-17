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

        Task<IOperationResult<IEnumerable<Area>>> GetAreasWithAvailabilityAsync(int id);

        #endregion
    }
}
