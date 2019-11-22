using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;

namespace Ferretto.VW.CommonUtils.Messages.Data
{
    public class AssignedMissionOperationChangedMessageData : IMessageData
    {
        #region Properties

        public BayNumber BayNumber { get; set; }

        public int? MissionId { get; set; }

        public int? MissionOperationId { get; set; }

        public int PendingMissionsCount { get; set; }

        public MessageVerbosity Verbosity { get; set; }

        #endregion

        #region Methods

        public override string ToString()
        {
            return $"BayId:{this.BayNumber} MissionId:{this.MissionId} MissionOperationId:{this.MissionOperationId} PendingMissionsCount:{this.PendingMissionsCount}";
        }

        #endregion
    }
}
