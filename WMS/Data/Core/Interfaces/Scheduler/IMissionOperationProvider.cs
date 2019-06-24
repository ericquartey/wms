using System.Collections.Generic;
using System.Threading.Tasks;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.BLL.Interfaces.Providers;
using Ferretto.WMS.Data.Core.Models;

namespace Ferretto.WMS.Data.Core.Interfaces
{
    public interface IMissionOperationProvider :
        IReadSingleAsyncProvider<MissionOperation, int>,
        IReadAllPagedAsyncProvider<MissionOperation, int>,
        ICreateRangeAsyncProvider<MissionOperation, int>,
        IUpdateAsyncProvider<MissionOperation, int>
    {
        #region Methods

        Task<IOperationResult<MissionOperation>> AbortAsync(int id);

        Task<IOperationResult<MissionOperation>> CompleteAsync(int id, double quantity);

        Task<IOperationResult<MissionOperation>> ExecuteAsync(int id);

        Task<IEnumerable<MissionOperation>> GetByListRowIdAsync(int listRowId);

        Task UpdateRowStatusAsync(ItemListRowOperation row, System.DateTime now);

        #endregion
    }
}
