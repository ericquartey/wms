using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;

namespace Ferretto.VW.MAS.AutomationService.Contracts.Hubs
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
            this.Index = bayId;
            this.BayType = bayType;
            this.BayStatus = bayStatus;
            this.PendingMissionsCount = pendingMissionsCount;
            this.CurrentMissionOperationId = currentMissionOperation;
        }

        #endregion

        #region Properties

        public BayIndex Index { get; }

        public CommonUtils.Messages.Enumerations.BayStatus BayStatus { get; }

        public CommonUtils.Messages.Enumerations.BayType BayType { get; }

        public int? CurrentMissionOperationId { get; }

        public int PendingMissionsCount { get; }

        #endregion
    }
}
