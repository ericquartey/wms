using System.Threading;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS.Utils.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Prism.Events;

namespace Ferretto.VW.MAS.IODriver.StateMachines.PowerUp
{
    internal sealed class PowerUpStateMachine : IoStateMachineBase
    {
        #region Fields

        private const int PulseInterval = 350;

        private readonly IoIndex index;

        private readonly IoStatus status;

        private Timer delayTimer;

        private bool isDisposed;

        private bool pulseOneTime;

        #endregion

        #region Constructors

        public PowerUpStateMachine(
            BlockingConcurrentQueue<IoWriteMessage> ioCommandQueue,
            IoStatus status,
            IoIndex index,
            IEventAggregator eventAggregator,
            ILogger logger,
            IServiceScopeFactory serviceScopeFactory)
            : base(eventAggregator, logger, ioCommandQueue, serviceScopeFactory)
        {
            this.status = status;
            this.index = index;

            logger.LogTrace("1:Method Start");
        }

        #endregion

        #region Methods

        public override void ProcessResponseMessage(IoReadMessage message)
        {
            this.Logger.LogTrace($"1:Valid Outputs={message.ValidOutputs}:Reset Security={message.ResetSecurity}");

            var checkMessage = message.FormatDataOperation == ShdFormatDataOperation.Data
                &&
                message.ValidOutputs
                &&
                message.ResetSecurity;

            if (checkMessage && !this.pulseOneTime)
            {
                // TEMP Start the timer for the PulseResetSecurity message in state ON according to the device specifications
                this.delayTimer = new Timer(this.DelayElapsed, null, PulseInterval, Timeout.Infinite);
                this.pulseOneTime = true;
            }

            base.ProcessResponseMessage(message);
        }

        public override void Start()
        {
            this.pulseOneTime = false;
            this.ChangeState(new PowerUpStartState(this, this.status, this.index, this.Logger));
        }

        protected override void Dispose(bool disposing)
        {
            if (this.isDisposed)
            {
                return;
            }

            if (disposing)
            {
                this.delayTimer?.Dispose();

                if (this.CurrentState is System.IDisposable disposableState)
                {
                    disposableState.Dispose();
                }
            }

            this.isDisposed = true;

            base.Dispose(disposing);
        }

        private void DelayElapsed(object state)
        {
            // TEMP Clear message IO
            var clearIoMessage = new IoWriteMessage();

            this.Logger.LogDebug($"1:Clear IO={clearIoMessage}");
            lock (this.status)
            {
                this.status.UpdateOutputStates(clearIoMessage.Outputs);
            }

            this.EnqueueMessage(clearIoMessage);
        }

        #endregion
    }
}
