using Ferretto.VW.CommonUtils.Messages.Enumerations;

namespace Ferretto.VW.CommonUtils.Messages.Interfaces
{
    public interface IBayOperationalStatusChangedMessageData
    {
        #region Properties

        BayStatus BayStatus { get; }

        int? CurrentMissionOperationId { get; }

        BayNumber Index { get; }

        int PendingMissionsCount { get; }

        #endregion
    }
}
