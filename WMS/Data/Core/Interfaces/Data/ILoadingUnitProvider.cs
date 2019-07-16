using System.Collections.Generic;
using System.Threading.Tasks;
using Ferretto.Common.BLL.Interfaces;
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
        IUpdateAsyncProvider<LoadingUnitOperation, int>,
        IGetUniqueValuesAsyncProvider,
        IDeleteAsyncProvider<LoadingUnitDetails, int>
    {
        #region Methods

        Task<IEnumerable<LoadingUnit>> GetAllAllowedByItemIdAsync(
            int id, int skip, int take, IEnumerable<SortOption> orderBySortOptions, string where, string search);

        Task<IEnumerable<LoadingUnitDetails>> GetAllByCellIdAsync(int id);

        Task<IEnumerable<LoadingUnitDetails>> GetAllByIdAisleAsync(
            int id, int skip, int take, IEnumerable<SortOption> orderBySortOptions, string where, string search);

        Task<LoadingUnitOperation> GetByIdForExecutionAsync(int id);

        Task<LoadingUnitSize> GetSizeByTypeIdAsync(int typeId);

        Task<IEnumerable<LoadingUnitDetails>> GetAllByMachineIdAsync(int machineId);

        Task<IOperationResult<LoadingUnitDetails>> UpdateMissionsCountAsync(int id);

        Task<IOperationResult<LoadingUnitOperationalInfoUpdate>> UpdateOperationalInfoAsync(
            LoadingUnitOperationalInfoUpdate model);

        #endregion
    }
}
