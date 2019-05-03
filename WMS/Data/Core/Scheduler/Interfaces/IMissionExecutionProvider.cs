using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.BLL.Interfaces.Providers;
using Ferretto.WMS.Data.Core.Models;

namespace Ferretto.WMS.Data.Core.Interfaces
{
    public interface IMissionExecutionProvider :
        IReadSingleAsyncProvider<MissionExecution, int>,
        IUpdateAsyncProvider<MissionExecution, int>,
        IReadAllAsyncProvider<MissionExecution, int>
    {
        #region Methods

        Task<IOperationResult<MissionExecution>> CompleteItemAsync(int id, double quantity);

        Task<IOperationResult<MissionExecution>> CompleteLoadingUnitAsync(int id);

        Task<IEnumerable<MissionExecution>> CreateForRequestsAsync(IEnumerable<ISchedulerRequest> requests);

        Task<IOperationResult<MissionExecution>> ExecuteAsync(int id);

        Task<IEnumerable<MissionExecution>> GetByListRowIdAsync(int listRowId);

        Task UpdateRowStatusAsync(ItemListRowExecution row, DateTime now);

        #endregion
    }
}
