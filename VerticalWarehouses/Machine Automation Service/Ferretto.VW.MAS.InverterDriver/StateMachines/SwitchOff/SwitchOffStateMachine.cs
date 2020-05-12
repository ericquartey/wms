using Ferretto.VW.MAS.InverterDriver.Contracts;
using Ferretto.VW.MAS.InverterDriver.InverterStatus.Interfaces;
using Ferretto.VW.MAS.Utils.Messages;
using Ferretto.VW.MAS.Utils.Messages.FieldData;
using Ferretto.VW.MAS.Utils.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Prism.Events;


namespace Ferretto.VW.MAS.InverterDriver.StateMachines.SwitchOff
{
    internal class SwitchOffStateMachine : InverterStateMachineBase
    {
        #region Fields

        private readonly IInverterStatusBase inverterStatus;

        private FieldCommandMessage nextCommandMessage;

        #endregion

        #region Constructors

        public SwitchOffStateMachine(
            IInverterStatusBase inverterStatus,
            ILogger logger,
            IEventAggregator eventAggregator,
            BlockingConcurrentQueue<InverterMessage> inverterCommandQueue,
            IServiceScopeFactory serviceScopeFactory,
            FieldCommandMessage nextCommandMessage = null)
            : base(logger, eventAggregator, inverterCommandQueue, serviceScopeFactory)
        {
            if (inverterStatus is null)
            {
                throw new System.ArgumentNullException(nameof(inverterStatus));
            }

            this.nextCommandMessage = nextCommandMessage;
            this.inverterStatus = inverterStatus;
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

        protected override void OnStopLocked()
        {
            this.nextCommandMessage = null;
        }

        #endregion
    }
}
