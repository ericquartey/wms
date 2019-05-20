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

        private readonly IoSHDStatus status;

        private Timer delayTimer;

        private bool disposed;

        private bool pulseOneTime;

        #endregion

        #region Constructors

        public PowerUpStateMachine(BlockingConcurrentQueue<IoSHDWriteMessage> ioCommandQueue, IoSHDStatus status, IEventAggregator eventAggregator, ILogger logger)
        {
            logger.LogDebug("1:Method Start");

            this.Logger = logger;
            this.IoCommandQueue = ioCommandQueue;
            this.status = status;
            this.pulseOneTime = false;
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

        #region Methods

        public override void ProcessMessage(IoSHDMessage message)
        {
            this.Logger.LogDebug("1:Method Start");

            this.Logger.LogTrace($"2:Valid Outputs={message.ValidOutputs}:Reset Security={message.ResetSecurity}");

            if (message.CodeOperation == Enumerations.SHDCodeOperation.Data &&
                message.ValidOutputs &&
                message.ResetSecurity)
            {
                this.delayTimer = new Timer(this.DelayElapsed, null, PULSE_INTERVAL, -1);    //VALUE -1 period means timer does not fire multiple times
            }

            base.ProcessMessage(message);

            this.Logger.LogDebug("3:Method End");
        }

        public override void ProcessResponseMessage(IoSHDReadMessage message)
        {
            this.Logger.LogDebug("1:Method Start");

            this.Logger.LogTrace($"2:Valid Outputs={message.ValidOutputs}:Reset Security={message.ResetSecurity}");

            var checkMessage = message.FormatDataOperation == Enumerations.SHDFormatDataOperation.Data &&
                               message.ValidOutputs &&
                               message.ResetSecurity;

            if (this.CurrentState is PulseResetState && checkMessage && !this.pulseOneTime)
            {
                // Start the timer for the PulseResetSecurity message in state ON according to the device specifications
                this.delayTimer = new Timer(this.DelayElapsed, null, PULSE_INTERVAL, -1);    //VALUE -1 period means timer does not fire multiple times
                this.pulseOneTime = true;
            }

            base.ProcessResponseMessage(message);

            this.Logger.LogDebug("3:Method End");
        }

        public override void Start()
        {
            this.pulseOneTime = false;
            this.CurrentState = new PowerUpStartState(this, this.status, this.Logger);
            this.CurrentState?.Start();
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

            // Clear message IO
            var clearIoMessage = new IoSHDWriteMessage();

            this.Logger.LogTrace($"2:Clear IO={clearIoMessage}");
            lock (this.status)
            {
                this.status.UpdateOutputStates(clearIoMessage.Outputs);
            }

            this.EnqueueMessage(clearIoMessage);

            this.Logger.LogDebug("3:Method End");
        }

        #endregion
    }
}
