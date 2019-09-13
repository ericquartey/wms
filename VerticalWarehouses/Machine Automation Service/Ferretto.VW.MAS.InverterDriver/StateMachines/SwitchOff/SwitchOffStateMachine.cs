using Ferretto.VW.MAS.InverterDriver.InverterStatus.Interfaces;
using Ferretto.VW.MAS.Utils.Messages;
using Ferretto.VW.MAS.Utils.Messages.FieldData;
using Ferretto.VW.MAS.Utils.Utilities;
using Microsoft.Extensions.Logging;
using Prism.Events;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS.InverterDriver.StateMachines.SwitchOff
{
    internal class SwitchOffStateMachine : InverterStateMachineBase
    {
        #region Fields

        private readonly IInverterStatusBase inverterStatus;

        private readonly FieldCommandMessage nextCommandMessage;

        private bool disposed;

        #endregion

        #region Constructors

        public SwitchOffStateMachine(
            IInverterStatusBase inverterStatus,
            BlockingConcurrentQueue<InverterMessage> inverterCommandQueue,
            IEventAggregator eventAggregator,
            ILogger logger,
            FieldCommandMessage nextCommandMessage = null)
            : base(logger, eventAggregator, inverterCommandQueue)
        {
            this.nextCommandMessage = nextCommandMessage;
            this.inverterStatus = inverterStatus;

            this.Logger.LogTrace("1:Method Start");
        }

        #endregion

        #region Methods

        public override void PublishNotificationEvent(FieldNotificationMessage notificationMessage)
        {
            if (this.CurrentState is SwitchOffEndState)
            {
                ((InverterSwitchOffFieldMessageData)notificationMessage.Data).NextCommandMessage = this.nextCommandMessage;
            }
            base.PublishNotificationEvent(notificationMessage);
        }

        /// <inheritdoc />
        public override void Start()
        {
            this.CurrentState = new SwitchOffStartState(this, this.inverterStatus, this.Logger);
            this.CurrentState?.Start();
        }

        public override void Stop()
        {
            this.CurrentState?.Stop();
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
