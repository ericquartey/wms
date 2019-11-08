using System.Threading;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS.Utils.Utilities;
using Microsoft.Extensions.Logging;
using Prism.Events;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS.IODriver.StateMachines.ResetSecurity
{
    internal sealed class ResetSecurityStateMachine : IoStateMachineBase
    {
        #region Fields

        private const int PulseInterval = 350;

        private readonly IoIndex index;

        private readonly IoStatus mainIoDevice;

        private readonly IoStatus status;

        private Timer delayTimer;

        private bool isDisposed;

        private bool pulseOneTime;

        #endregion

        #region Constructors

        public ResetSecurityStateMachine(
            BlockingConcurrentQueue<IoWriteMessage> ioCommandQueue,
            IoStatus status,
            IoStatus mainIoDevice,
            IoIndex index,
            IEventAggregator eventAggregator,
            ILogger logger)
            : base(eventAggregator, logger, ioCommandQueue)
        {
            this.status = status ?? throw new System.ArgumentNullException(nameof(status));
            this.mainIoDevice = mainIoDevice ?? throw new System.ArgumentNullException(nameof(mainIoDevice));

            this.index = index;
        }

        #endregion

        #region Methods

        public override void ProcessMessage(IoMessage message)
        {
            this.Logger.LogTrace($"1:Valid Outputs={message.ValidOutputs}:Reset security={message.ResetSecurity}");

            if (message.ValidOutputs && message.ResetSecurity)
            {
                this.delayTimer = new Timer(this.DelayElapsed, null, PulseInterval, Timeout.Infinite);
            }

            base.ProcessMessage(message);
        }

        public override void ProcessResponseMessage(IoReadMessage message)
        {
            this.Logger.LogTrace($"1:Valid Outputs={message.ValidOutputs}:Reset security={message.ResetSecurity}");

            var checkMessage = message.FormatDataOperation == ShdFormatDataOperation.Data &&
                message.ValidOutputs && message.ResetSecurity;

            if (this.CurrentState is ResetSecurityStartState && checkMessage && !this.pulseOneTime)
            {
                this.delayTimer = new Timer(this.DelayElapsed, null, PulseInterval, Timeout.Infinite);
                this.pulseOneTime = true;
            }

            base.ProcessResponseMessage(message);
        }

        public override void Start()
        {
            this.pulseOneTime = false;
            this.ChangeState(new ResetSecurityStartState(this, this.status, this.mainIoDevice, this.index, this.Logger));
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
            var pulseIoMessage = new IoWriteMessage
            {
                ResetSecurity = false,
                PowerEnable = true,
            };

            this.Logger.LogTrace($"1:Pulse IO={pulseIoMessage}");
            this.status.UpdateOutputStates(pulseIoMessage.Outputs);

            this.EnqueueMessage(pulseIoMessage);
        }

        #endregion
    }
}
