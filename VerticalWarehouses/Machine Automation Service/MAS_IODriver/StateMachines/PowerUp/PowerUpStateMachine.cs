using System.Threading;
using Ferretto.VW.MAS_Utils.Utilities;
using Microsoft.Extensions.Logging;
using Prism.Events;
// ReSharper disable ArrangeThisQualifier

namespace Ferretto.VW.MAS_IODriver.StateMachines.PowerUp
{
    public class PowerUpStateMachine : IoStateMachineBase
    {
        #region Fields

        private const int PULSE_INTERVAL = 350;

        private Timer delayTimer;

        private bool disposed;

        #endregion

        //public PowerUpStateMachine(BlockingConcurrentQueue<IoMessage> ioCommandQueue, IEventAggregator eventAggregator, ILogger logger)
        //{
        //    logger.LogDebug("1:Method Start");

        //    this.Logger = logger;
        //    this.IoCommandQueue = ioCommandQueue;
        //    this.EventAggregator = eventAggregator;

        //    this.Logger.LogDebug("2:Method End");
        //}

        #region Constructors

        public PowerUpStateMachine(BlockingConcurrentQueue<IoSHDMessage> ioCommandQueue, IEventAggregator eventAggregator, ILogger logger)
        {
            logger.LogDebug("1:Method Start");

            this.Logger = logger;
            this.IoCommandQueue = ioCommandQueue;
            this.EventAggregator = eventAggregator;

            this.Logger.LogDebug("2:Method End");
        }

        #endregion

        #region Destructors

        ~PowerUpStateMachine()
        {
            this.Dispose(false);
        }

        #endregion

        //public override void ProcessMessage(IoMessage message)
        //{
        //    this.Logger.LogDebug("1:Method Start");

        //    this.Logger.LogTrace($"2:Valid Outputs={message.ValidOutputs}:Reset Security={message.ResetSecurity}");

        //    if (message.ValidOutputs && message.ResetSecurity)
        //    {
        //        this.delayTimer = new Timer(this.DelayElapsed, null, PULSE_INTERVAL, -1);    //VALUE -1 period means timer does not fire multiple times
        //    }

        //    base.ProcessMessage(message);

        //    this.Logger.LogDebug("3:Method End");
        //}

        #region Methods

        public override void ProcessMessage(IoSHDMessage message)
        {
            this.Logger.LogDebug("1:Method Start");

            this.Logger.LogTrace($"2:Valid Outputs={message.ValidOutputs}:Reset Security={message.ResetSecurity}");

            if (message.ValidOutputs && message.ResetSecurity)
            {
                this.delayTimer = new Timer(this.DelayElapsed, null, PULSE_INTERVAL, -1);    //VALUE -1 period means timer does not fire multiple times
            }

            base.ProcessMessage(message);

            this.Logger.LogDebug("3:Method End");
        }

        public override void Start()
        {
            this.CurrentState = new ClearOutputsState(this, this.Logger);
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

        //private void DelayElapsed(object state)
        //{
        //    var pulseIoMessage = new IoMessage(false);

        //    this.EnqueueMessage(pulseIoMessage);
        //}

        private void DelayElapsed(object state)
        {
            var pulseIoMessage = new IoSHDMessage(false);

            this.EnqueueMessage(pulseIoMessage);
        }

        #endregion
    }
}
