using Ferretto.VW.Common_Utils.Messages.Enumerations;
using Ferretto.VW.MAS_InverterDriver.Interface.StateMachines;
using Ferretto.VW.MAS_InverterDriver.InverterStatus.Interfaces;
using Ferretto.VW.MAS_Utils.Enumerations;
using Ferretto.VW.MAS_Utils.Messages;
using Microsoft.Extensions.Logging;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS_InverterDriver.StateMachines.Positioning
{
    public class PositioningEndState : InverterStateBase
    {
        #region Constructors

        public PositioningEndState(
            IInverterStateMachine parentStateMachine,
            IInverterStatusBase inverterStatus,
            ILogger logger)
            : base(parentStateMachine, inverterStatus, logger)
        {
        }

        #endregion

        #region Destructors

        ~PositioningEndState()
        {
            this.Dispose(false);
        }

        #endregion

        #region Methods

        public override void Release()
        {
        }

        /// <inheritdoc />
        public override void Start()
        {
            var notificationMessage = new FieldNotificationMessage(
                null,
                "Message",
                FieldMessageActor.Any,
                FieldMessageActor.InverterDriver,
                FieldMessageType.Positioning,
                MessageStatus.OperationEnd);

            this.Logger.LogTrace($"1:Type={notificationMessage.Type}:Destination={notificationMessage.Destination}:Status={notificationMessage.Status}");

            this.ParentStateMachine.PublishNotificationEvent(notificationMessage);
        }

        /// <inheritdoc />
        public override bool ValidateCommandMessage(InverterMessage message)
        {
            this.Logger.LogTrace($"1:message={message}:Is Error={message.IsError}");

            return false;
        }

        /// <inheritdoc />
        public override bool ValidateCommandResponse(InverterMessage message)
        {
            this.Logger.LogTrace($"1:message={message}:Is Error={message.IsError}");

            return true;
        }

        #endregion
    }
}
