using System.Collections.Generic;
using System.Threading.Tasks;
using Ferretto.Common.BLL.Interfaces.Providers;
using Ferretto.Common.BusinessModels;

namespace Ferretto.Common.BusinessProviders
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
