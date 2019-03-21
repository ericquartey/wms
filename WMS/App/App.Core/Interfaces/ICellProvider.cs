using System.Collections.Generic;
using System.Threading.Tasks;
using Ferretto.Common.BLL.Interfaces.Providers;
using Ferretto.WMS.App.Core.Models;

namespace Ferretto.WMS.App.Core.Interfaces
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
