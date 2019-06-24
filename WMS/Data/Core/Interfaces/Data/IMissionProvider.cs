using System.Collections.Generic;
using System.Threading.Tasks;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.BLL.Interfaces.Providers;
using Ferretto.WMS.Data.Core.Models;

namespace Ferretto.WMS.Data.Core.Interfaces
{
    public interface IMissionProvider :
        IReadAllPagedAsyncProvider<MissionInfo, int>,
        IReadSingleAsyncProvider<MissionWithLoadingUnitDetails, int>,
        ICreateRangeAsyncProvider<Mission, int>,
        ICreateAsyncProvider<Mission, int>,
        IGetUniqueValuesAsyncProvider
    {
        #region Methods

        Task<IOperationResult<IEnumerable<Mission>>> GetByMachineIdAsync(int id);

        Task<IOperationResult<MissionWithLoadingUnitDetails>> GetDetailsByIdAsync(int id);

        Task<Mission> GetNewByLoadingUnitIdAsync(int loadingUnitId);

        #endregion
    }
}
