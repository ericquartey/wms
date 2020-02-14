using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.InverterDriver.Contracts;
using Ferretto.VW.MAS.InverterDriver.InverterStatus.Interfaces;
using Ferretto.VW.MAS.Utils.Enumerations;
using Ferretto.VW.MAS.Utils.Messages;
using Ferretto.VW.MAS.Utils.Messages.FieldData;
using Microsoft.Extensions.Logging;


namespace Ferretto.VW.MAS.InverterDriver.StateMachines.PowerOff
{
    internal class PowerOffEndState : InverterStateBase
    {
        #region Constructors

        public PowerOffEndState(
            IInverterStateMachine parentStateMachine,
            IInverterStatusBase inverterStatus,
            ILogger logger)
            : base(parentStateMachine, inverterStatus, logger)
        {
        }

        #endregion

        #region Methods

        public override void Start()
        {
            var notificationMessageData = new InverterPowerOffFieldMessageData();
            var notificationMessage = new FieldNotificationMessage(
                notificationMessageData,
                "Inverter Power Off End",
                FieldMessageActor.InverterDriver,
                FieldMessageActor.InverterDriver,
                FieldMessageType.InverterPowerOff,
                MessageStatus.OperationEnd,
                this.InverterStatus.SystemIndex);

            this.Logger.LogTrace($"1:Type={notificationMessage.Type}:Destination={notificationMessage.Destination}:Status={notificationMessage.Status}");

            this.ParentStateMachine.PublishNotificationEvent(notificationMessage);
        }

        /// <inheritdoc />
        public override void Stop()
        {
            this.Logger.LogTrace("1:Method Start");
        }

        /// <inheritdoc />
        public override bool ValidateCommandMessage(InverterMessage message)
        {
            this.Logger.LogTrace($"1:message={message}:Is Error={message.IsError}");

            return false;
        }

        public override bool ValidateCommandResponse(InverterMessage message)
        {
            this.Logger.LogTrace($"1:message={message}:Is Error={message.IsError}");

            return true;
        }

        #endregion
    }
}
