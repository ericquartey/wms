using Ferretto.VW.Common_Utils.Messages.Enumerations;
using Ferretto.VW.MAS_InverterDriver.Interface.StateMachines;
using Ferretto.VW.MAS_InverterDriver.InverterStatus.Interfaces;
using Ferretto.VW.MAS_Utils.Enumerations;
using Ferretto.VW.MAS_Utils.Messages;
using Microsoft.Extensions.Logging;

// ReSharper disable ArrangeThisQualifier

namespace Ferretto.VW.MAS_InverterDriver.StateMachines.Positioning
{
    public class PositioningErrorState : InverterStateBase
    {
        #region Fields

        private readonly IInverterStatusBase inverterStatus;

        private readonly ILogger logger;

        private bool disposed;

        #endregion

        #region Constructors

        public PositioningErrorState(IInverterStateMachine parentStateMachine, IInverterStatusBase inverterStatus, ILogger logger)
        {
            logger.LogTrace("1:Method Start");
            this.logger = logger;

            this.ParentStateMachine = parentStateMachine;
            this.inverterStatus = inverterStatus;
        }

        #endregion

        #region Destructors

        ~PositioningErrorState()
        {
            this.Dispose(false);
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public override void Start()
        {
            var notificationMessage = new FieldNotificationMessage(null,
                "Positioning Error",
                FieldMessageActor.Any,
                FieldMessageActor.InverterDriver,
                FieldMessageType.InverterStop,
                MessageStatus.OperationError,
                ErrorLevel.Error);

            this.logger.LogTrace($"1:Type={notificationMessage.Type}:Destination={notificationMessage.Destination}:Status={notificationMessage.Status}");

            this.ParentStateMachine.PublishNotificationEvent(notificationMessage);
        }

        /// <inheritdoc />
        public override bool ValidateCommandMessage(InverterMessage message)
        {
            this.logger.LogTrace($"1:message={message}:Is Error={message.IsError}");

            return false;
        }

        /// <inheritdoc />
        public override bool ValidateCommandResponse(InverterMessage message)
        {
            this.logger.LogTrace($"1:message={message}:Is Error={message.IsError}");

            return true;
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
