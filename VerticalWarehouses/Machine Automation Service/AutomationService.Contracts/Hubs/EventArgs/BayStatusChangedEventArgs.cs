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
            WMS.Data.WebAPI.Contracts.MissionOperationInfo missionOperation)
        {
            this.BayId = bayId;
            this.BayType = bayType;
            this.BayStatus = bayStatus;
            this.PendingMissionsCount = pendingMissionsCount;
            this.CurrentMissionOperation = missionOperation;
        }

        #endregion

        #region Properties

        public int BayId { get; set; }

        public CommonUtils.Messages.Enumerations.BayStatus BayStatus { get; set; }

        public CommonUtils.Messages.Enumerations.BayType BayType { get; set; }

        public string ClientIpAddress { get; set; }

        public string ConnectionId { get; set; }

        public WMS.Data.WebAPI.Contracts.MissionOperationInfo CurrentMissionOperation { get; set; }

        public int PendingMissionsCount { get; set; }

        #endregion
    }
}
