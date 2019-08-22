using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.InverterDriver.Interface.StateMachines;
using Ferretto.VW.MAS.InverterDriver.InverterStatus.Interfaces;
using Ferretto.VW.MAS.Utils.Enumerations;
using Ferretto.VW.MAS.Utils.Messages;
using Microsoft.Extensions.Logging;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS.InverterDriver.StateMachines.Positioning
{
    public class PositioningErrorState : InverterStateBase
    {


        #region Constructors

        public PositioningErrorState(
            IInverterStateMachine parentStateMachine,
            IInverterStatusBase inverterStatus,
            ILogger logger)
            : base(parentStateMachine, inverterStatus, logger)
        {
        }

        #endregion

        #region Destructors

        ~PositioningErrorState()
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
                "Positioning Error",
                FieldMessageActor.Any,
                FieldMessageActor.InverterDriver,
                FieldMessageType.Positioning,
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
