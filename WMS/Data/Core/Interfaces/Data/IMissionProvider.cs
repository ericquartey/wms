using System.Collections.Generic;
using System.Threading.Tasks;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.BLL.Interfaces.Providers;
using Ferretto.WMS.Data.Core.Models;

namespace Ferretto.WMS.Data.Core.Interfaces
{
    public interface IMissionProvider :
        IReadAllPagedAsyncProvider<MissionInfo, int>,
        IReadSingleAsyncProvider<Mission, int>,
        ICreateRangeAsyncProvider<Mission, int>,
        ICreateAsyncProvider<Mission, int>,
        IUpdateAsyncProvider<Mission, int>,
        IGetUniqueValuesAsyncProvider
    {
        #region Methods

        Task<IOperationResult<IEnumerable<MissionInfo>>> GetByMachineIdAsync(int id);

        Task<IOperationResult<MissionWithLoadingUnitDetails>> GetDetailsByIdAsync(int id);

        Task<MissionInfo> GetInfoByIdAsync(int id);

        Task<Mission> GetNewByLoadingUnitIdAsync(int loadingUnitId);

        #endregion
    }
}
