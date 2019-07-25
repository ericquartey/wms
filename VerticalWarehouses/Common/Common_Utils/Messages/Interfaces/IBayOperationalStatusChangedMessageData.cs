using Ferretto.VW.CommonUtils.Messages.Enumerations;

namespace Ferretto.VW.CommonUtils.Messages.Interfaces
{
    public interface IBayOperationalStatusChangedMessageData
    {
        #region Properties

        int BayId { get; }

        BayStatus BayStatus { get; }

        BayType BayType { get; }

        string ClientIpAddress { get; }

        string ConnectionId { get; }

        Ferretto.WMS.Data.WebAPI.Contracts.MissionOperationInfo CurrentMissionOperation { get; }

        int PendingMissionsCount { get; }

        #endregion
    }
}
