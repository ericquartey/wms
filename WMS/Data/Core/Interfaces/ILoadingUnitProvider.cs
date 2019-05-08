using System.Collections.Generic;
using System.Threading.Tasks;
using Ferretto.Common.BLL.Interfaces.Providers;
using Ferretto.Common.Utils.Expressions;
using Ferretto.WMS.Data.Core.Models;

namespace Ferretto.WMS.Data.Core.Interfaces
{
    public interface ILoadingUnitProvider :
        ICreateAsyncProvider<LoadingUnitCreating, int>,
        IReadAllPagedAsyncProvider<LoadingUnit, int>,
        IReadSingleAsyncProvider<LoadingUnitDetails, int>,
        IUpdateAsyncProvider<LoadingUnitDetails, int>,
        IUpdateAsyncProvider<LoadingUnitExecution, int>,
        IGetUniqueValuesAsyncProvider,
        IDeleteAsyncProvider<LoadingUnitDetails, int>
    {
        #region Methods

        Task<IEnumerable<LoadingUnitDetails>> GetAllByCellIdAsync(int id);

        Task<IEnumerable<LoadingUnitDetails>> GetAllByIdAisleAsync(
            int id, int skip, int take, IEnumerable<SortOption> orderBySortOptions, string where, string search);

        Task<LoadingUnitExecution> GetByIdForExecutionAsync(int id);

        Task<LoadingUnitSize> GetSizeByTypeIdAsync(int typeId);

        #endregion
    }
}
