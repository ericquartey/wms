using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.InverterDriver.Contracts;
using Ferretto.VW.MAS.InverterDriver.InverterStatus.Interfaces;
using Ferretto.VW.MAS.Utils.Enumerations;
using Ferretto.VW.MAS.Utils.Messages;
using Ferretto.VW.MAS.Utils.Messages.FieldData;
using Microsoft.Extensions.Logging;


namespace Ferretto.VW.MAS.InverterDriver.StateMachines.ResetFault
{
    internal class ResetFaultErrorState : InverterStateBase
    {
        #region Fields

        private readonly InverterIndex inverterIndex;

        #endregion

        #region Constructors

        public ResetFaultErrorState(
            IInverterStateMachine parentStateMachine,
            IInverterStatusBase inverterStatus,
            InverterIndex inverterIndex,
            ILogger logger)
            : base(parentStateMachine, inverterStatus, logger)
        {
            this.inverterIndex = inverterIndex;
        }

        #endregion

        #region Methods

        public override void Start()
        {
            var notificationMessageData = new InverterFaultFieldMessageData();
            var notificationMessage = new FieldNotificationMessage(
                notificationMessageData,
                "Inverter Fault Reset Error",
                FieldMessageActor.Any,
                FieldMessageActor.InverterDriver,
                FieldMessageType.InverterFaultReset,
                MessageStatus.OperationError,
                this.InverterStatus.SystemIndex,
                ErrorLevel.Error);

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

            return true;
        }

        public override bool ValidateCommandResponse(InverterMessage message)
        {
            this.Logger.LogTrace($"1:message={message}:Is Error={message.IsError}");

            return true;
        }

        #endregion
    }
}
