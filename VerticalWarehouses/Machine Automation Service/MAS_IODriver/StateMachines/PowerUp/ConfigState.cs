using Ferretto.VW.MAS.IODriver.Interface;
using Ferretto.VW.MAS_Utils.Enumerations;
using Microsoft.Extensions.Logging;

namespace Ferretto.VW.MAS.IODriver.StateMachines.PowerUp
{
    public class ConfigState : IoStateBase
    {
        #region Fields

        private readonly IoIndex index;

        private readonly IoSHDStatus status;

        private bool disposed;

        #endregion

        #region Constructors

        public ConfigState(
            IIoStateMachine parentStateMachine,
            IoSHDStatus status,
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

        ~ConfigState()
        {
            this.Dispose(false);
        }

        #endregion

        #region Methods

        public override void ProcessMessage(IoSHDMessage message)
        {
            throw new System.NotImplementedException();
        }

        public override void ProcessResponseMessage(IoSHDReadMessage message)
        {
            this.Logger.LogTrace("1:Method Start");

            if (message.FormatDataOperation == Enumerations.SHDFormatDataOperation.Ack)
            {
                this.Logger.LogTrace($"2:Format data operation message={message.FormatDataOperation}");
                this.ParentStateMachine.ChangeState(new ClearOutputsState(this.ParentStateMachine, this.status, this.index, this.Logger));
            }
        }

        public override void Start()
        {
            var message = new IoSHDWriteMessage(
                this.status.ComunicationTimeOut,
                this.status.UseSetupOutputLines,
                this.status.SetupOutputLines,
                this.status.DebounceInput);

            this.Logger.LogDebug($"1: ConfigurationMessage [comTout={this.status.ComunicationTimeOut} ms - debounceTime={this.status.DebounceInput} ms]");

            this.ParentStateMachine.EnqueueMessage(message);
        }

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

        #endregion
    }
}
