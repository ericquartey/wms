using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.CommonUtils.Messages.Data
{
    public class ChangeRunningStateMessageData : IChangeRunningStateMessageData
    {
        #region Constructors

        public ChangeRunningStateMessageData(bool enable, CommandAction commandAction = CommandAction.Start, StopRequestReason stopReason = StopRequestReason.NoReason, MessageVerbosity verbosity = MessageVerbosity.Debug)
        {
            this.Enable = enable;
            this.CommandAction = commandAction;
            this.StopReason = stopReason;
            this.Verbosity = verbosity;
        }

        #endregion

        #region Properties

        public CommandAction CommandAction { get; }

        public bool Enable { get; }

        public StopRequestReason StopReason { get; }

        public MessageVerbosity Verbosity { get; }

        #endregion
    }
}
