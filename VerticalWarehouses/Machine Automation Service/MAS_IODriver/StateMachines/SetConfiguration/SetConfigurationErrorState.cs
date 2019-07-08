using Ferretto.VW.MAS_IODriver.Interface;
using Microsoft.Extensions.Logging;

namespace Ferretto.VW.MAS_IODriver.StateMachines.SetConfiguration
{
    public class SetConfigurationErrorState : IoStateBase
    {
        #region Fields

        private readonly IoSHDStatus status;

        private bool disposed;

        #endregion

        #region Constructors

        public SetConfigurationErrorState(
            IIoStateMachine parentStateMachine,
            IoSHDStatus status,
            ILogger logger)
            : base(parentStateMachine, logger)
        {
            this.status = status;

            logger.LogTrace("1:Method Start");
        }

        #endregion

        #region Destructors

        ~SetConfigurationErrorState()
        {
            this.Dispose(false);
        }

        #endregion

        #region Methods

        public override void ProcessMessage(IoSHDMessage message)
        {
            this.Logger.LogTrace("1:Method Start");
        }

        public override void ProcessResponseMessage(IoSHDReadMessage message)
        {
            this.Logger.LogDebug($"1: Received Message = {message.ToString()}");
        }

        public override void Start()
        {
            this.Logger.LogTrace("1:Method Start");
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
