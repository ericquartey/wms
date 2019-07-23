using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;

namespace Ferretto.VW.CommonUtils.Messages.Data
{
    public class BayConnectedMessageData : IBayConnectedMessageData
    {
        #region Properties

        public int BayId { get; set; }

        // TODO change type from int to BayType
        public int BayType { get; set; }

        public string ConnectionId { get; set; }

        public int PendingMissionsCount { get; set; }

        public MessageVerbosity Verbosity { get; set; }

        #endregion
    }
}
