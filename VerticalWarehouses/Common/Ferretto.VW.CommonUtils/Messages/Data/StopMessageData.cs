using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;

namespace Ferretto.VW.CommonUtils.Messages.Data
{
    public class StopMessageData : IStopMessageData
    {
        #region Constructors

        public StopMessageData()
        {
        }

        public StopMessageData(StopRequestReason stopReason, MessageVerbosity verbosity = MessageVerbosity.Debug)
        {
            this.StopReason = stopReason;
            this.Verbosity = verbosity;
        }

        #endregion

        #region Properties

        public StopRequestReason StopReason { get; }

        public MessageVerbosity Verbosity { get; }

        #endregion

        #region Methods

        public override string ToString()
        {
            return $"Stop Reason:{this.StopReason}";
        }

        #endregion
    }
}
