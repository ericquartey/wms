using Ferretto.VW.Common_Utils.Messages.Enumerations;
using Ferretto.VW.MAS_InverterDriver.Interface.StateMachines;
using Ferretto.VW.MAS_Utils.Enumerations;
using Ferretto.VW.MAS_Utils.Messages;
using Ferretto.VW.MAS_Utils.Messages.FieldData;
using Microsoft.Extensions.Logging;

namespace Ferretto.VW.MAS_InverterDriver.StateMachines.ShutterPositioning
{
    public class ShutterPositioningErrorState : InverterStateBase
    {
        #region Fields

        private readonly ILogger logger;

        private ShutterPosition shutterPosition;

        private byte systemIndex;

        private bool disposed;

        #endregion

        #region Constructurs

        public ShutterPositioningErrorState(IInverterStateMachine parentStateMachine, ShutterPosition shutterPosition, ILogger logg)
        {
            logger.LogDebug("1:Method Start");
            this.logger = logger;

            this.ParentStateMachine = parentStateMachine;
            this.shutterPosition = shutterPosition;

            this.logger.LogDebug("2:Method End");
        }

        #endregion

        #region Destructors

        ~ShutterPositioningErrorState()
        {
            this.Dispose(false);
        }

        #endregion

        #region Methods

        public override void Start()
        {
            this.logger.LogDebug("1:Method Start");

            var messageData = new ShutterPositioningFieldMessageData(this.shutterPosition, this.systemIndex);

            var errorNotification = new FieldNotificationMessage(messageData,
                "Inverter operation error",
                FieldMessageActor.Any,
                FieldMessageActor.InverterDriver,
                FieldMessageType.ShutterPositioning,
                MessageStatus.OperationError,
                ErrorLevel.Error);

            this.logger.LogTrace($"2:Type={errorNotification.Type}:Destination={errorNotification.Destination}:Status={errorNotification.Status}");

            this.ParentStateMachine.PublishNotificationEvent(errorNotification);

            this.logger.LogDebug("3:Method End");
        }

        public override bool ValidateCommandMessage(InverterMessage message)
        {
            this.logger.LogDebug("1:Method Start");

            this.logger.LogTrace($"2:message={message}:Is Error={message.IsError}");

            this.logger.LogDebug("3:Method End");

            return false;
        }


        public override bool ValidateCommandResponse(InverterMessage message)
        {
            this.logger.LogDebug("1:Method Start");

            this.logger.LogTrace($"2:message={message}:Is Error={message.IsError}");

            this.logger.LogDebug("3:Method End");

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
