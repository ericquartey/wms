using Ferretto.VW.MAS.Utils.Utilities;
using Microsoft.Extensions.Logging;
using Prism.Events;

namespace Ferretto.VW.MAS.IODriver.StateMachines.SetConfiguration
{
    public class SetConfigurationStateMachine : IoStateMachineBase
    {

        #region Fields

        private readonly IoStatus status;

        private bool disposed;

        #endregion

        #region Constructors

        public SetConfigurationStateMachine(
            BlockingConcurrentQueue<IoWriteMessage> ioCommandQueue,
            IoStatus status,
            IEventAggregator eventAggregator,
            ILogger logger)
            : base(eventAggregator, logger)
        {
            this.IoCommandQueue = ioCommandQueue;
            this.status = status;

            logger.LogTrace("1:Method Start");
        }

        #endregion

        #region Destructors

        ~SetConfigurationStateMachine()
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

            base.ProcessMessage(message);
        }

        public override void ProcessResponseMessage(IoReadMessage message)
        {
            this.Logger.LogTrace("1:Method Start");

            base.ProcessResponseMessage(message);
        }

        public override void Start()
        {
            this.CurrentState = new SetConfigurationStartState(this, this.status, this.Logger);
            this.CurrentState?.Start();
        }

        #endregion
    }
}
