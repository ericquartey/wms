using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;
using Ferretto.WMS.Data.WebAPI.Contracts;

namespace Ferretto.VW.CommonUtils.Messages.Data
{
    public class ExecuteMissionMessageData : IExecuteMissionMessageData
    {
        #region Properties

        public string BayConnectionId { get; set; }

        public MissionInfo Mission { get; set; }

        public MissionOperationInfo MissionOperation { get; set; }

        public int PendingMissionsCount { get; set; }

        public MessageVerbosity Verbosity { get; set; }

        #endregion
    }
}
