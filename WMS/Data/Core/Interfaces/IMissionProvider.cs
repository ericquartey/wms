using System.Collections.Generic;
using System.Threading.Tasks;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.BLL.Interfaces.Providers;
using Ferretto.WMS.Data.Core.Models;

namespace Ferretto.WMS.Data.Core.Interfaces
{
    public interface IMissionProvider :
        IReadAllPagedAsyncProvider<Mission, int>,
        IReadSingleAsyncProvider<Mission, int>,
        IGetUniqueValuesAsyncProvider
    {
        #region Methods

        Task<IOperationResult<IEnumerable<Mission>>> GetByMachineIdAsync(int id);

        Task<IOperationResult<MissionDetails>> GetDetailsByIdAsync(int id);

        #endregion
    }
}
