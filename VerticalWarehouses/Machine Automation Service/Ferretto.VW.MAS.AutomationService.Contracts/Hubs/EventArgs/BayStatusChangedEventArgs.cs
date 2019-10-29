using Ferretto.VW.CommonUtils.Messages.Interfaces;

namespace Ferretto.VW.MAS.AutomationService.Contracts.Hubs
{
    public class BayStatusChangedEventArgs : System.EventArgs, IBayOperationalStatusChangedMessageData
    {
        #region Constructors

        public BayStatusChangedEventArgs(
            CommonUtils.Messages.Enumerations.BayNumber bayNumber,
            CommonUtils.Messages.Enumerations.BayStatus bayStatus,
            int pendingMissionsCount,
            int? currentMissionOperation)
        {
            this.Index = bayNumber;
            this.BayStatus = bayStatus;
            this.PendingMissionsCount = pendingMissionsCount;
            this.CurrentMissionOperationId = currentMissionOperation;
        }

        #endregion

        #region Properties

        public CommonUtils.Messages.Enumerations.BayStatus BayStatus { get; }

        public int? CurrentMissionOperationId { get; }

        public CommonUtils.Messages.Enumerations.BayNumber Index { get; }

        public int PendingMissionsCount { get; }

        #endregion
    }
}
