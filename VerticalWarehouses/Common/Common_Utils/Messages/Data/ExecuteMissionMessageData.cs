using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;

namespace Ferretto.VW.CommonUtils.Messages.Data
{
    public class NewMissionOperationAvailable : INewMissionOperationAvailable, IMessageData
    {
        #region Properties

        public int BayId { get; set; }

        public int MissionId { get; set; }

        public int MissionOperationId { get; set; }

        public int PendingMissionsCount { get; set; }

        public MessageVerbosity Verbosity { get; set; }

        #endregion

        #region Methods

        public override string ToString()
        {
            return $"BayConnectionId:{this.BayConnectionId} Mission:{this.Mission} MissionsQuantity:{this.MissionsQuantity}";
        }

        #endregion
    }
}
