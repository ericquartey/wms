using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;

namespace Ferretto.VW.CommonUtils.Messages.Data
{
    public class MissionOperationCompletedMessageData : IMissionOperationCompletedMessageData
    {
        #region Properties

        public int MissionId { get; set; }

        public int MissionOperationId { get; set; }

        public MessageVerbosity Verbosity { get; set; }

        #endregion
    }
}
