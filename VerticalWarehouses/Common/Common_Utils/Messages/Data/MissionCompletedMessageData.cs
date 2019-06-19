using Ferretto.VW.Common_Utils.Messages.Enumerations;
using Ferretto.VW.Common_Utils.Messages.Interfaces;

namespace Ferretto.VW.Common_Utils.Messages.Data
{
    public class MissionCompletedMessageData : IMissionCompletedMessageData
    {
        #region Properties

        public int BayId { get; set; }

        public int MissionId { get; set; }

        public MessageVerbosity Verbosity { get; set; }

        #endregion
    }
}
