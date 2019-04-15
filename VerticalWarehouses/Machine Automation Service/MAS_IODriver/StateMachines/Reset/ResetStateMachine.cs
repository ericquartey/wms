using System.Threading;
using Ferretto.VW.MAS_Utils.Utilities;
using Microsoft.Extensions.Logging;
using Prism.Events;
// ReSharper disable ArrangeThisQualifier

namespace Ferretto.VW.MAS_IODriver.StateMachines.Reset
{
    public class ResetStateMachine : IoStateMachineBase
    {
        #region Fields

        private const int PULSE_INTERVAL = 350;

        private Timer delayTimer;

        private bool disposed;

        #endregion

        #region Constructors

        public ResetStateMachine(BlockingConcurrentQueue<IoMessage> ioCommandQueue, IEventAggregator eventAggregator, ILogger logger)
        {
            logger.LogDebug("1:Method Start");

            this.IoCommandQueue = ioCommandQueue;
            this.EventAggregator = eventAggregator;
            this.Logger = logger;

            this.Logger.LogDebug("2:Method End");
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
            this.Logger.LogDebug("1:Method Start");
            this.Logger.LogTrace($"2:Valid Outputs={message.ValidOutputs}:Reset security={message.ResetSecurity}");

            if (message.ValidOutputs && message.ResetSecurity)
            {
                this.delayTimer = new Timer(this.DelayElapsed, null, PULSE_INTERVAL, -1);    //VALUE -1 period means timer does not fire multiple times
            }

            base.ProcessMessage(message);

            this.Logger.LogDebug("4:Method End");
        }

        public override void Start()
        {
            this.CurrentState = new ResetOutputsState(this, this.Logger);
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
            this.Logger.LogDebug("1:Method Start");

            var pulseIoMessage = new IoMessage(false);

            this.Logger.LogTrace($"2:Pulse IO={pulseIoMessage}");

            this.EnqueueMessage(pulseIoMessage);

            this.Logger.LogDebug("3:Method End");
        }

        #endregion
    }
}
