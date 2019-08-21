using Ferretto.VW.MAS.IODriver.Interface;
using Ferretto.VW.MAS.Utils.Enumerations;
using Microsoft.Extensions.Logging;

namespace Ferretto.VW.MAS.IODriver.StateMachines.PowerUp
{
    public class PowerUpStartState : IoStateBase
    {

        #region Fields

        private readonly IoIndex index;

        private readonly IoStatus status;

        private bool disposed;

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

        #region Destructors

        ~PowerUpStartState()
        {
            this.Dispose(false);
        }

        #endregion



        #region Methods

        protected override void Dispose(bool disposing)
        {
            if (this.disposed)
            {
                return;
            }

            if (disposing)
            {
            }

            this.disposed = true;

            base.Dispose(disposing);
        }

        public override void ProcessMessage(IoMessage message)
        {
            this.Logger.LogTrace($"1:Valid Outputs={message.ValidOutputs}:Outputs Cleared={message.OutputsCleared}");

            if (message.CodeOperation == Enumerations.SHDCodeOperation.Configuration)
            {
                this.ParentStateMachine.ChangeState(new PowerUpClearOutputsState(this.ParentStateMachine, this.status, this.index, this.Logger));
            }
        }

        public override void ProcessResponseMessage(IoReadMessage message)
        {
            this.Logger.LogTrace("1:Method Start");

            if (message.FormatDataOperation == Enumerations.SHDFormatDataOperation.Data)
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
