using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;
using Ferretto.WMS.Data.WebAPI.Contracts;

namespace Ferretto.VW.CommonUtils.Messages.Data
{
    public class BayOperationalStatusChangedMessageData : IBayOperationalStatusChangedMessageData, IMessageData
    {
        #region Properties

        public int BayId { get; set; }

        public BayStatus BayStatus { get; set; }

        public BayType BayType { get; set; }

        public string ClientIpAddress { get; set; }

        public string ConnectionId { get; set; }

        public MissionOperationInfo CurrentMissionOperation { get; set; }

        public int PendingMissionsCount { get; set; }

        public MessageVerbosity Verbosity { get; set; }

        #endregion
    }
}
