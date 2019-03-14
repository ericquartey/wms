using System.Collections.Generic;
using System.Threading.Tasks;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.BLL.Interfaces.Providers;
using Ferretto.Common.BusinessModels;

namespace Ferretto.Common.BusinessProviders
{
    public interface ICellProvider :
        IPagedBusinessProvider<Cell, int>,
        IReadSingleAsyncProvider<CellDetails, int>,
        IUpdateAsyncProvider<CellDetails, int>
    {
        #region Methods

        Task<IEnumerable<Enumeration>> GetByAisleIdAsync(int aisleId);

        Task<IEnumerable<Enumeration>> GetByAreaIdAsync(int areaId);

        #endregion
    }
}
