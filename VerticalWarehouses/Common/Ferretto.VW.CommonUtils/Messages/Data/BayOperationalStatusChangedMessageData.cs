using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;

namespace Ferretto.VW.CommonUtils.Messages.Data
{
    public class BayOperationalStatusChangedMessageData : IBayOperationalStatusChangedMessageData, IMessageData
    {
        #region Properties

        public int BayNumber { get; set; }

        public BayStatus BayStatus { get; set; }

        public BayType BayType { get; set; }

        public int? CurrentMissionOperationId { get; set; }

        public int PendingMissionsCount { get; set; }

        public MessageVerbosity Verbosity { get; set; }

        #endregion
    }
}
