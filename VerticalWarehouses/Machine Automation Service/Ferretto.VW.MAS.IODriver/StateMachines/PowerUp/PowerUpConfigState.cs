using Ferretto.VW.MAS.DataModels;
using Microsoft.Extensions.Logging;

namespace Ferretto.VW.MAS.IODriver.StateMachines.PowerUp
{
    internal sealed class PowerUpConfigState : IoStateBase
    {
        #region Fields

        private readonly IoIndex index;

        private readonly IoStatus status;

        #endregion

        #region Constructors

        public PowerUpConfigState(
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
            throw new System.NotImplementedException();
        }

        public override void ProcessResponseMessage(IoReadMessage message)
        {
            this.Logger.LogTrace("1:Method Start");

            if (message.FormatDataOperation == ShdFormatDataOperation.Ack)
            {
                this.Logger.LogTrace($"2:Format data operation message={message.FormatDataOperation}");
                this.ParentStateMachine.ChangeState(new PowerUpClearOutputsState(this.ParentStateMachine, this.status, this.index, this.Logger));
            }
        }

        public override void Start()
        {
            var message = new IoWriteMessage(
                this.status.ComunicationTimeOut,
                this.status.UseSetupOutputLines,
                this.status.SetupOutputLines,
                this.status.DebounceInput);

            this.Logger.LogDebug($"1:ConfigurationMessage [comTout={this.status.ComunicationTimeOut} ms - debounceTime={this.status.DebounceInput} ms]");

            this.ParentStateMachine.EnqueueMessage(message);
        }

        #endregion
    }
}
