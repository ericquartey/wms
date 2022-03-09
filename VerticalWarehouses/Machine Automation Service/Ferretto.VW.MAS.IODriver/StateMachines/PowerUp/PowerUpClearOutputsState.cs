using Ferretto.VW.MAS.DataModels;
using Microsoft.Extensions.Logging;

namespace Ferretto.VW.MAS.IODriver.StateMachines.PowerUp
{
    internal sealed class PowerUpClearOutputsState : IoStateBase
    {
        #region Fields

        private readonly IoIndex index;

        private readonly IoStatus status;

        #endregion

        #region Constructors

        public PowerUpClearOutputsState(
            IIoStateMachine parentStateMachine,
            IoStatus status,
            IoIndex index,
            ILogger logger)
            : base(parentStateMachine, logger)
        {
            this.status = status;
            this.index = index;

            logger.LogTrace("1:Method Start");
        }

        #endregion

        #region Methods

        public override void ProcessResponseMessage(IoReadMessage message)
        {
            this.Logger.LogTrace($"1:Valid Outputs={message.ValidOutputs}:Outputs Cleared={message.OutputsCleared}");

            var checkMessage = message.FormatDataOperation == ShdFormatDataOperation.Data &&
                message.ValidOutputs &&
                message.OutputsCleared;

            // TEMP Check the matching between the status output flags and the message output flags (i.e. the clear output message has been processed)
            if (this.status.MatchOutputs(message.Outputs))
            {
                this.ParentStateMachine.ChangeState(new PowerUpEndState(this.ParentStateMachine, this.status, this.index, this.Logger));
            }
        }

        public override void Start()
        {
            var clearIoMessage = new IoWriteMessage();

            lock (this.status)
            {
                this.status.UpdateOutputStates(clearIoMessage.Outputs);
            }

            this.Logger.LogDebug($"1:Clear IO={clearIoMessage}");

            this.ParentStateMachine.EnqueueMessage(clearIoMessage);
        }

        #endregion
    }
}
