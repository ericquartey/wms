using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;

namespace Ferretto.VW.CommonUtils.Messages.Data
{
    public class BayOperationalStatusChangedMessageData : IBayOperationalStatusChangedMessageData, IMessageData
    {
        #region Properties

        public BayStatus BayStatus { get; set; }

        public int? CurrentMissionOperationId { get; set; }

        public BayNumber Index { get; }

        public int PendingMissionsCount { get; set; }

        public MessageVerbosity Verbosity { get; set; }

        #endregion
    }
}
