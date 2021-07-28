using Ferretto.VW.MAS.DataModels;
using Microsoft.Extensions.Logging;

namespace Ferretto.VW.MAS.IODriver.StateMachines.EndMissionRobot
{
    internal sealed class EndMissionRobotStartState : IoStateBase
    {
        #region Fields

        private readonly bool enable;

        private readonly IoIndex index;

        private readonly IoStatus status;

        #endregion

        #region Constructors

        public EndMissionRobotStartState(
            bool enable,
            IoStatus status,
            ILogger logger,
            IIoStateMachine parentStateMachine,
            IoIndex index)
            : base(parentStateMachine, logger)
        {
            this.enable = enable;
            this.status = status;
            this.index = index;

            logger.LogTrace("1:Method Start");
        }

        #endregion

        #region Methods

        public override void ProcessResponseMessage(IoReadMessage message)
        {
            this.Logger.LogTrace("1:Method Start");

            var checkMessage = message.FormatDataOperation == ShdFormatDataOperation.Data
                               &&
                               message.ValidOutputs
                               &&
                               message.EndMissionRobotOn == this.enable;

            if (this.status.MatchOutputs(message.Outputs) && checkMessage)
            {
                this.Logger.LogTrace("2:Change State to EndMissionRobotEndState");
                this.ParentStateMachine.ChangeState(new EndMissionRobotEndState(this.enable, this.status, this.Logger, this.ParentStateMachine, this.index));
            }
        }

        public override void Start()
        {
            var message = new IoWriteMessage(this.status.OutputData);
            message.SwitchEndMissionRobot(this.enable);

            this.Logger.LogDebug($"1:EndMissionRobot ={message}, enable {this.enable}");

            lock (this.status)
            {
                this.status.UpdateOutputStates(message.Outputs);
            }

            this.ParentStateMachine.EnqueueMessage(message);
        }

        #endregion
    }
}
