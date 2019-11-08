using Ferretto.VW.MAS.DataModels;
using Microsoft.Extensions.Logging;

namespace Ferretto.VW.MAS.IODriver.StateMachines.PowerUp
{
    internal sealed class PowerUpStartState : IoStateBase
    {
        #region Fields

        private readonly IoIndex index;

        private readonly IoStatus status;

        #endregion

        #region Constructors

        public PowerUpStartState(
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

        public override void ProcessMessage(IoMessage message)
        {
            this.Logger.LogTrace($"1:Valid Outputs={message.ValidOutputs}:Outputs Cleared={message.OutputsCleared}");
        }

        public override void ProcessResponseMessage(IoReadMessage message)
        {
            this.Logger.LogTrace("1:Method Start");

            if (message.FormatDataOperation == ShdFormatDataOperation.Data)
            {
                this.Logger.LogTrace($"2:Format data operation message={message.FormatDataOperation}");

                this.ParentStateMachine.ChangeState(new PowerUpConfigState(this.ParentStateMachine, this.status, this.index, this.Logger));
            }
        }

        public override void Start()
        {
            var message = new IoWriteMessage(this.status.OutputData);

            this.ParentStateMachine.EnqueueMessage(message);
        }

        #endregion
    }
}
