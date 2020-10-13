using System;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;

namespace Ferretto.VW.CommonUtils.Messages.Data
{
    public class ChangeRunningStateMessageData : IChangeRunningStateMessageData
    {
        #region Constructors

        public ChangeRunningStateMessageData()
        {
        }

        public ChangeRunningStateMessageData(
            bool enable,
            Guid? missionId = null,
            CommandAction commandAction = CommandAction.Start,
            StopRequestReason stopReason = StopRequestReason.NoReason,
            MessageVerbosity verbosity = MessageVerbosity.Debug)
        {
            this.Enable = enable;
            this.MissionId = missionId;
            this.CommandAction = commandAction;
            this.StopReason = stopReason;
            this.Verbosity = verbosity;
        }

        #endregion

        #region Properties

        public CommandAction CommandAction { get; }

        public bool Enable { get; }

        public Guid? MissionId { get; }

        public StopRequestReason StopReason { get; }

        public MessageVerbosity Verbosity { get; }

        #endregion
    }
}
