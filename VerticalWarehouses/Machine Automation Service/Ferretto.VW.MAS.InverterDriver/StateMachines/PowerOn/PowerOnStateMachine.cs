using Ferretto.VW.MAS.InverterDriver.InverterStatus.Interfaces;
using Ferretto.VW.MAS.Utils.Enumerations;
using Ferretto.VW.MAS.Utils.Messages;
using Ferretto.VW.MAS.Utils.Messages.FieldData;
using Ferretto.VW.MAS.Utils.Utilities;
using Microsoft.Extensions.Logging;
using Prism.Events;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS.InverterDriver.StateMachines.PowerOn
{
    internal class PowerOnStateMachine : InverterStateMachineBase
    {
        #region Fields

        private readonly IInverterStatusBase inverterStatus;

        private readonly FieldCommandMessage nextCommandMessage;

        #endregion

        #region Constructors

        public PowerOnStateMachine(
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
            if (this.CurrentState is PowerOnEndState)
            {
                if (this.nextCommandMessage.Type != FieldMessageType.InverterPowerOn)
                {
                    ((InverterPowerOnFieldMessageData)notificationMessage.Data).NextCommandMessage = this.nextCommandMessage;
                }
            }
            base.PublishNotificationEvent(notificationMessage);
        }

        /// <inheritdoc />
        public override void Start()
        {
            this.CurrentState = new PowerOnStartState(this, this.inverterStatus, this.Logger);
            this.CurrentState?.Start();
        }

        /// <inheritdoc />
        public override void Stop()
        {
            this.CurrentState?.Stop();
        }

        #endregion
    }
}
