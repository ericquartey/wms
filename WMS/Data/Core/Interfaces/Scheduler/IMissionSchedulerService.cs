using System.Threading.Tasks;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.WMS.Data.Core.Models;

namespace Ferretto.WMS.Data.Core.Interfaces
{
    public interface IMissionSchedulerService
    {
        #region Methods

        Task<IOperationResult<MissionOperation>> AbortOperationAsync(int operationId);

        Task<IOperationResult<MissionOperation>> CompleteOperationAsync(int operationId, double quantity);

        Task<IOperationResult<MissionOperation>> ExecuteOperationAsync(int operationId);

        #endregion
    }
}
