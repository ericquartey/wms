using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ferretto.VW.App.Services
{
    public interface IMissionOperationsService
    {
        #region Properties

        WMS.Data.WebAPI.Contracts.MissionInfo CurrentMission { get; }

        WMS.Data.WebAPI.Contracts.MissionOperation CurrentMissionOperation { get; }

        int PendingMissionOperationsCount { get; }

        #endregion

        #region Methods

        Task CancelCurrentAsync();

        Task CompleteCurrentAsync(double quantity);

        Task PartiallyCompleteCurrentAsync(double quantity);

        #endregion
    }
}
