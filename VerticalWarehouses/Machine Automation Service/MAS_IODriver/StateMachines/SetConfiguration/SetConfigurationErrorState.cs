using Ferretto.VW.MAS.IODriver.Interface;
using Microsoft.Extensions.Logging;

namespace Ferretto.VW.MAS.IODriver.StateMachines.SetConfiguration
{
    public class SetConfigurationErrorState : IoStateBase
    {

        #region Fields

        private readonly IoStatus status;

        private bool disposed;

        #endregion

        #region Constructors

        public SetConfigurationErrorState(
            IIoStateMachine parentStateMachine,
            IoStatus status,
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
            this.Logger.LogTrace("1:Method Start");
        }

        public override void ProcessResponseMessage(IoReadMessage message)
        {
            this.Logger.LogDebug($"1: Received Message = {message.ToString()}");
        }

        public override void Start()
        {
            this.Logger.LogTrace("1:Method Start");
        }

        #endregion
    }
}
