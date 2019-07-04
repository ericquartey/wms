using Ferretto.VW.Common_Utils.Messages.Enumerations;
using Ferretto.VW.MAS_InverterDriver.Enumerations;
using Ferretto.VW.MAS_InverterDriver.Interface.StateMachines;
using Ferretto.VW.MAS_InverterDriver.InverterStatus.Interfaces;
using Ferretto.VW.MAS_Utils.Enumerations;
using Ferretto.VW.MAS_Utils.Messages;
using Ferretto.VW.MAS_Utils.Messages.FieldInterfaces;
using Microsoft.Extensions.Logging;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS_InverterDriver.StateMachines.Positioning
{
    public class PositioningStartState : InverterStateBase
    {
        #region Fields

        private readonly IInverterPositioningFieldMessageData data;

        private readonly IInverterStatusBase inverterStatus;

        private readonly ILogger logger;

        private bool disposed;

        #endregion

        #region Constructors

        public PositioningStartState(IInverterStateMachine parentStateMachine, IInverterPositioningFieldMessageData data,
            IInverterStatusBase inverterStatus, ILogger logger)
        {
            logger.LogTrace("1:Method Start");
            this.logger = logger;

            this.ParentStateMachine = parentStateMachine;
            this.inverterStatus = inverterStatus;
            this.data = data;
        }

        #endregion

        #region Destructors

        ~PositioningStartState()
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
            this.inverterStatus.OperatingMode = (ushort)InverterOperationMode.Position;

            var inverterMessage = new InverterMessage(this.inverterStatus.SystemIndex, (short)InverterParameterId.SetOperatingModeParam, this.inverterStatus.OperatingMode);

            this.logger.LogTrace($"1:inverterMessage={inverterMessage}");

            this.ParentStateMachine.EnqueueMessage(inverterMessage);

            var notificationMessage = new FieldNotificationMessage(
                this.data,
                $"Positioning Start",
                FieldMessageActor.Any,
                FieldMessageActor.InverterDriver,
                FieldMessageType.Positioning,
                MessageStatus.OperationStart);

            this.logger.LogTrace($"2:Publishing Field Notification Message {notificationMessage.Type} Destination {notificationMessage.Destination} Status {notificationMessage.Status}");

            this.ParentStateMachine.PublishNotificationEvent(notificationMessage);
        }

        /// <inheritdoc />
        public override bool ValidateCommandMessage(InverterMessage message)
        {
            this.logger.LogTrace($"1:message={message}:Is Error={message.IsError}");

            if (message.IsError)
            {
                this.ParentStateMachine.ChangeState(new PositioningErrorState(this.ParentStateMachine, this.inverterStatus, this.logger));
            }

            this.logger.LogTrace($"2:message={message}:Parameter Id={message.ParameterId}");

            if (message.ParameterId == InverterParameterId.SetOperatingModeParam)
            {
                this.ParentStateMachine.ChangeState(new PositioningSetParametersState(this.ParentStateMachine, this.data, this.inverterStatus, this.logger));
            }

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
