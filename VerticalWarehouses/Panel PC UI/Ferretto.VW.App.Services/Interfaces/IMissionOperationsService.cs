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

        /// <exception cref="MasWebApiException"></exception>
        Task CancelCurrentAsync();

        /// <exception cref="MasWebApiException"></exception>
        Task CompleteCurrentAsync(double quantity);

        /// <exception cref="MasWebApiException"></exception>
        Task PartiallyCompleteCurrentAsync(double quantity);

        #endregion
    }
}
