using System.Threading.Tasks;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.WMS.Data.Core.Models;

namespace Ferretto.WMS.Data.Core.Interfaces
{
    public interface IMissionLoadingUnitProvider
    {
        #region Methods

        Task<IOperationResult<Mission>> AbortAsync(int id);

        Task<IOperationResult<Mission>> CompleteAsync(int id);

        Task<Mission> CreateWithdrawalOperationAsync(LoadingUnitSchedulerRequest request);

        Task<IOperationResult<Mission>> ExecuteAsync(int id);

        #endregion
    }
}
