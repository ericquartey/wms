using System.Threading;
using Ferretto.VW.MAS.Utils.Utilities;
using Microsoft.Extensions.Logging;
using Prism.Events;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS.IODriver.StateMachines.ResetSecurity
{
    public class ResetSecurityStateMachine : IoStateMachineBase
    {

        #region Fields

        private const int PULSE_INTERVAL = 350;

        private readonly IoStatus status;

        private Timer delayTimer;

        private bool disposed;

        private bool pulseOneTime;

        #endregion

        #region Constructors

        public ResetSecurityStateMachine(
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

        ~ResetSecurityStateMachine()
        {
            this.Dispose(false);
        }

        #endregion



        #region Methods

        private void DelayElapsed(object state)
        {
            var pulseIoMessage = new IoWriteMessage();
            pulseIoMessage.SwitchResetSecurity(false);
            pulseIoMessage.SwitchPowerEnable(true);

            this.Logger.LogTrace($"1:Pulse IO={pulseIoMessage}");
            this.status.UpdateOutputStates(pulseIoMessage.Outputs);

            this.EnqueueMessage(pulseIoMessage);
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

        public override void ProcessMessage(IoMessage message)
        {
            this.Logger.LogTrace($"1:Valid Outputs={message.ValidOutputs}:Reset security={message.ResetSecurity}");

            if (message.ValidOutputs && message.ResetSecurity)
            {
                this.delayTimer = new Timer(this.DelayElapsed, null, PULSE_INTERVAL, -1);    //VALUE -1 period means timer does not fire multiple times
            }

            base.ProcessMessage(message);
        }

        public override void ProcessResponseMessage(IoReadMessage message)
        {
            this.Logger.LogTrace($"1:Valid Outputs={message.ValidOutputs}:Reset security={message.ResetSecurity}");

            var checkMessage = message.FormatDataOperation == Enumerations.SHDFormatDataOperation.Data &&
                message.ValidOutputs && message.ResetSecurity;

            if (this.CurrentState is ResetSecurityStartState && checkMessage && !this.pulseOneTime)
            {
                this.delayTimer = new Timer(this.DelayElapsed, null, PULSE_INTERVAL, -1);    //VALUE -1 period means timer does not fire multiple times
                this.pulseOneTime = true;
            }

            base.ProcessResponseMessage(message);
        }

        public override void Start()
        {
            this.pulseOneTime = false;
            this.CurrentState = new ResetSecurityStartState(this, this.status, this.Logger);
            this.CurrentState?.Start();
        }

        #endregion
    }
}
