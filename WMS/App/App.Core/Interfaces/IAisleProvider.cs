using System.Collections.Generic;
using System.Threading.Tasks;
using Ferretto.Common.BLL.Interfaces.Providers;
using Ferretto.WMS.App.Core.Models;

namespace Ferretto.WMS.App.Core.Interfaces
{
    public interface IAisleProvider :
        IReadAllAsyncProvider<Aisle, int>,
        IReadSingleAsyncProvider<Aisle, int>
    {
        #region Methods

        Task<IEnumerable<Aisle>> GetAislesByAreaIdAsync(int areaId);

        #endregion
    }
}
