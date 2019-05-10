using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Ferretto.Common.BLL.Interfaces.Providers;
using Ferretto.WMS.Data.Core.Models;

namespace Ferretto.WMS.Data.Core.Interfaces
{
    public interface IMissionCreationProvider
    {
        #region Methods

        Task<MissionExecution> CreateWithdrawalMissionAsync(LoadingUnitSchedulerRequest request);

        Task<IEnumerable<MissionExecution>> CreateWithdrawalMissionsAsync(ItemSchedulerRequest request);

        #endregion
    }
}
