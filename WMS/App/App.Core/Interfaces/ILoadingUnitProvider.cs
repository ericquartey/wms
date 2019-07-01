using System.Collections.Generic;
using System.Threading.Tasks;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.BLL.Interfaces.Providers;
using Ferretto.Common.Utils.Expressions;
using Ferretto.WMS.App.Core.Models;

namespace Ferretto.WMS.App.Core.Interfaces
{
    public interface ILoadingUnitProvider :
        IPagedBusinessProvider<LoadingUnit, int>,
        IReadSingleAsyncProvider<LoadingUnitDetails, int>,
        ICreateAsyncProvider<LoadingUnitDetails, int>,
        IUpdateAsyncProvider<LoadingUnitDetails, int>,
        IDeleteAsyncProvider<LoadingUnit, int>
    {
        #region Methods

        Task<IOperationResult<IEnumerable<LoadingUnit>>> GetAllAllowedByItemIdAsync(
            int itemId,
            int skip,
            int take,
            IEnumerable<SortOption> orderBySortOptions = null);

        Task<IOperationResult<IEnumerable<LoadingUnitDetails>>> GetByCellIdAsync(int id);

        Task<IOperationResult<LoadingUnitDetails>> GetNewAsync();

        Task<IOperationResult<SchedulerRequest>> WithdrawAsync(int loadingUnitId, int bayId);

        #endregion
    }
}
