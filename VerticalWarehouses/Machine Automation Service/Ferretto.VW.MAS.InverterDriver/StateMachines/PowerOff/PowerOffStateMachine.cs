using Ferretto.VW.MAS.InverterDriver.Contracts;
using Ferretto.VW.MAS.InverterDriver.InverterStatus.Interfaces;
using Ferretto.VW.MAS.Utils.Enumerations;
using Ferretto.VW.MAS.Utils.Messages;
using Ferretto.VW.MAS.Utils.Messages.FieldData;
using Ferretto.VW.MAS.Utils.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Prism.Events;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS.InverterDriver.StateMachines.PowerOff
{
    internal class PowerOffStateMachine : InverterStateMachineBase
    {
        #region Fields

        private readonly IInverterStatusBase inverterStatus;

        private FieldCommandMessage nextCommandMessage;

        #endregion

        #region Constructors

        public PowerOffStateMachine(
            IInverterStatusBase inverterStatus,
            ILogger logger,
            IEventAggregator eventAggregator,
            BlockingConcurrentQueue<InverterMessage> inverterCommandQueue,
            IServiceScopeFactory serviceScopeFactory,
            FieldCommandMessage nextCommandMessage = null)
            : base(logger, eventAggregator, inverterCommandQueue, serviceScopeFactory)
        {
            this.nextCommandMessage = nextCommandMessage;
            this.inverterStatus = inverterStatus;
        }

        #endregion

        #region Methods

        public override void PublishNotificationEvent(FieldNotificationMessage notificationMessage)
        {
            if (this.CurrentState is PowerOffEndState)
            {
                if (this.nextCommandMessage?.Type != FieldMessageType.InverterPowerOff)
                {
                    ((InverterPowerOffFieldMessageData)notificationMessage.Data).NextCommandMessage = this.nextCommandMessage;
                }
            }
            base.PublishNotificationEvent(notificationMessage);
        }

        /// <inheritdoc />
        public override void Start()
        {
            this.CurrentState = new PowerOffStartState(this, this.inverterStatus, this.Logger);
            this.CurrentState?.Start();
        }

        protected override void OnStopLocked()
        {
            this.nextCommandMessage = null;
        }

        #endregion
    }
}
