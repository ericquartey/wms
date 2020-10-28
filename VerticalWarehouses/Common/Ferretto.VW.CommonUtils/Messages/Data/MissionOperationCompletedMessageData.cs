using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;

namespace Ferretto.VW.CommonUtils.Messages.Data
{
    public class MissionOperationCompletedMessageData : IMessageData
    {
        #region Constructors

        public MissionOperationCompletedMessageData()
        {
        }

        #endregion

        #region Properties

        public int MissionOperationId { get; set; }

        public MessageVerbosity Verbosity { get; set; }

        #endregion

        #region Methods

        public override string ToString()
        {
            return $"MissionOperationId:{this.MissionOperationId}";
        }

        #endregion
    }
}
