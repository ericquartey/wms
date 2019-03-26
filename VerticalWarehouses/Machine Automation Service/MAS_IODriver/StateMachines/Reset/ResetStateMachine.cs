using System.Threading;
using Ferretto.VW.Common_Utils.Utilities;
using Microsoft.Extensions.Logging;
using Prism.Events;

namespace Ferretto.VW.MAS_IODriver.StateMachines.Reset
{
    public class ResetStateMachine : IoStateMachineBase
    {
        #region Fields

        private Timer delayTimer;

        private const int PulseInterval = 350;

        private readonly ILogger logger;

        private bool disposed;

        #endregion

        #region Constructors

        public ResetStateMachine(BlockingConcurrentQueue<IoMessage> ioCommandQueue, IEventAggregator eventAggregator, ILogger logger)
        {
            this.ioCommandQueue = ioCommandQueue;
            this.eventAggregator = eventAggregator;
            this.logger = logger;
        }

        #endregion

        #region Destructors

        ~ResetStateMachine()
        {
            this.Dispose(false);
        }

        #endregion

        #region Methods

        public override void ProcessMessage(IoMessage message)
        {
            if (message.ValidOutputs && message.ResetSecurity)
            {
                this.delayTimer = new Timer(this.DelayElapsed, null, PulseInterval, -1);    //VALUE -1 period means timer does not fire multiple times
            }
            base.ProcessMessage(message);
        }

        public override void Start()
        {
            this.CurrentState = new ResetOutputsState(this, this.logger);
        }

        protected override void Dispose(bool disposing)
        {
            if (this.disposed)
            {
                return;
            }

            if (disposing)
            {
                this.delayTimer?.Dispose();
                this.CurrentState.Dispose();
            }

            this.disposed = true;

            base.Dispose(disposing);
        }

        private void DelayElapsed(object state)
        {
            var pulseIoMessage = new IoMessage(false);

            this.EnqueueMessage(pulseIoMessage);
        }

        #endregion
    }
}
