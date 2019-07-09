using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;

namespace Ferretto.VW.CommonUtils.Messages.Data
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
