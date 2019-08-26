using Ferretto.VW.CommonUtils.Messages.Enumerations;

namespace Ferretto.VW.CommonUtils.Messages.Interfaces
{
    public interface IBayOperationalStatusChangedMessageData
    {


        #region Properties

        BayStatus BayStatus { get; }

        BayType BayType { get; }

        int? CurrentMissionOperationId { get; }

        int PendingMissionsCount { get; }

        #endregion
    }
}
