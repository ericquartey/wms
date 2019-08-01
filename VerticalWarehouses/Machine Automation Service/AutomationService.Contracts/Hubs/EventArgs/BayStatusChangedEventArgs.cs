using Ferretto.VW.CommonUtils.Messages.Interfaces;

namespace Ferretto.VW.MAS.AutomationService.Contracts.Hubs.EventArgs
{
    public class BayStatusChangedEventArgs : System.EventArgs, IBayOperationalStatusChangedMessageData
    {
        #region Constructors

        // TODO change type from int to BayType
        public BayStatusChangedEventArgs(
            int bayId,
            CommonUtils.Messages.Enumerations.BayType bayType,
            CommonUtils.Messages.Enumerations.BayStatus bayStatus,
            int pendingMissionsCount,
            int? currentMissionOperation)
        {
            this.BayId = bayId;
            this.BayType = bayType;
            this.BayStatus = bayStatus;
            this.PendingMissionsCount = pendingMissionsCount;
            this.CurrentMissionOperationId = currentMissionOperation;
        }

        #endregion

        #region Properties

        public int BayId { get; }

        public CommonUtils.Messages.Enumerations.BayStatus BayStatus { get; }

        public CommonUtils.Messages.Enumerations.BayType BayType { get; }

        public int? CurrentMissionOperationId { get; }

        public int PendingMissionsCount { get; }

        #endregion
    }
}
