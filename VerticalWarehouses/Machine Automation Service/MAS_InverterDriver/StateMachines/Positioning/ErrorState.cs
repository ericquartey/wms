using Ferretto.VW.Common_Utils.Messages.Enumerations;
using Ferretto.VW.MAS_InverterDriver.Interface.StateMachines;
using Ferretto.VW.MAS_Utils.Enumerations;
using Ferretto.VW.MAS_Utils.Messages;
using Ferretto.VW.MAS_Utils.Messages.FieldData;
using Ferretto.VW.MAS_Utils.Messages.FieldInterfaces;
using Microsoft.Extensions.Logging;

namespace Ferretto.VW.MAS_InverterDriver.StateMachines.Positioning
{
    public class ErrorState : InverterStateBase
    {
        #region Fields

        private readonly IPositioningFieldMessageData data;

        private readonly ILogger logger;

        private bool disposed;

        #endregion

        #region Constructors

        public ErrorState(IInverterStateMachine parentStateMachine, IPositioningFieldMessageData data, ILogger logger)
        {
            this.logger = logger;
            this.logger.LogDebug("1:Method Start");

            this.ParentStateMachine = parentStateMachine;
            this.data = data;

            var messageData = new PositioningFieldMessageData(
                    this.data.AxisMovement,
                    this.data.MovementType,
                    this.data.TargetPosition,
                    this.data.TargetSpeed,
                    this.data.TargetAcceleration,
                    this.data.TargetDeceleration,
                    this.data.NumberCycles);

            var errorNotification = new FieldNotificationMessage(messageData,
                "Inverter operation error",
                FieldMessageActor.Any,
                FieldMessageActor.InverterDriver,
                FieldMessageType.Positioning,
                MessageStatus.OperationError,
                ErrorLevel.Error);

            this.logger.LogTrace($"2:Type={errorNotification.Type}:Destination={errorNotification.Destination}:Status={errorNotification.Status}");

            parentStateMachine.PublishNotificationEvent(errorNotification);

            this.logger.LogDebug("3:Method End");
        }

        #endregion

        #region Destructors

        ~ErrorState()
        {
            this.Dispose(false);
        }

        #endregion

        #region Methods

        public override bool ProcessMessage(InverterMessage message)
        {
            this.logger.LogDebug("1:Method Start");

            this.logger.LogTrace($"2:message={message}:Is Error={message.IsError}");

            this.logger.LogDebug("4:Method End");

            return false;
        }

        public override void Stop()
        {
            this.logger.LogDebug("1:Method Start");

            this.ParentStateMachine.ChangeState(new EndState(this.ParentStateMachine, this.data, this.logger, true));

            this.logger.LogDebug("2:Method End");
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
