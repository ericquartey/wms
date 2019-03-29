using System.Threading.Tasks;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.WMS.Scheduler.Core.Models;

namespace Ferretto.WMS.Scheduler.Core.Interfaces
{
    public interface IMissionExecutionSchedulerProvider
    {
        #region Methods

        Task<IOperationResult<Mission>> CompleteAsync(int id, int quantity);

        Task<IOperationResult<Mission>> ExecuteAsync(int id);

        Task UpdateRowStatusAsync(ItemListRow row, System.DateTime now);

        #endregion
    }
}
