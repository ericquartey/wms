using System.Collections.Generic;
using System.Threading.Tasks;
using Ferretto.Common.BLL.Interfaces.Providers;
using Ferretto.WMS.Data.Core.Models;

namespace Ferretto.WMS.Data.Core.Interfaces
{
    public interface ICellProvider :
        ICreateAsyncProvider<CellDetails, int>,
        IReadAllPagedAsyncProvider<Cell, int>,
        IReadSingleAsyncProvider<CellDetails, int>,
        IUpdateAsyncProvider<CellDetails, int>,
        IGetUniqueValuesAsyncProvider
    {
        #region Methods

        Task<IEnumerable<Cell>> GetByAisleIdAsync(int aisleId);

        Task<IEnumerable<Cell>> GetByAreaIdAsync(int areaId);

        #endregion
    }
}
