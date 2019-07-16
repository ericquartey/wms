using System.Collections.Generic;
using System.Threading.Tasks;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.BLL.Interfaces.Providers;
using Ferretto.WMS.Data.Core.Models;

namespace Ferretto.WMS.Data.Core.Interfaces
{
    public interface ICellProvider :
        IReadAllPagedAsyncProvider<Cell, int>,
        IReadSingleAsyncProvider<CellDetails, int>,
        IUpdateAsyncProvider<CellDetails, int>,
        IGetUniqueValuesAsyncProvider
    {
        #region Methods

        Task<IEnumerable<Cell>> GetByAisleIdAsync(int aisleId);

        Task<IEnumerable<Cell>> GetByAreaIdAsync(int areaId);

        Task<IEnumerable<Cell>> GetByLoadingUniTypeIdAsync(int loadingUnitTypeId);

        Task<IOperationResult<CellOperationalInfoUpdate>> UpdateOperationalInfoAsync(
            CellOperationalInfoUpdate model);

        #endregion
    }
}
