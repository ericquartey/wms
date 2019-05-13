using Ferretto.VW.Common_Utils.Messages.Enumerations;
using Ferretto.VW.MAS_InverterDriver.Enumerations;
using Ferretto.VW.MAS_InverterDriver.Interface.StateMachines;
using Ferretto.VW.MAS_InverterDriver.InverterStatus.Interfaces;
using Ferretto.VW.MAS_Utils.Enumerations;
using Ferretto.VW.MAS_Utils.Messages;
using Ferretto.VW.MAS_Utils.Messages.FieldInterfaces;
using Ferretto.VW.MAS_Utils.Utilities;
using Microsoft.Extensions.Logging;

// ReSharper disable ArrangeThisQualifier

namespace Ferretto.VW.MAS_InverterDriver.StateMachines.VerticalPositioning
{
    public class VerticalPositioningStartState : InverterStateBase
    {
        #region Fields

        protected BlockingConcurrentQueue<InverterMessage> InverterCommandQueue;

        private readonly IPositioningFieldMessageData data;

        private readonly IInverterStatusBase inverterStatus;

        private readonly ILogger logger;

        private bool disposed;

        #endregion

        #region Constructors

        public VerticalPositioningStartState(IInverterStateMachine parentStateMachine, IPositioningFieldMessageData data,
            BlockingConcurrentQueue<InverterMessage> inverterCommandQueue, IInverterStatusBase inverterStatus, ILogger logger)
        {
            logger.LogDebug("1:Method Start");
            this.logger = logger;

            this.ParentStateMachine = parentStateMachine;
            this.InverterCommandQueue = inverterCommandQueue;
            this.inverterStatus = inverterStatus;
            this.data = data;

            this.logger.LogDebug("2:Method End");
        }

        #endregion

        #region Destructors

        ~VerticalPositioningStartState()
        {
            this.Dispose(false);
        }

        #endregion

        #region Methods

        public override void Start()
        {
            this.logger.LogDebug("1:Method Start");

            this.inverterStatus.CommonControlWord.QuickStop = false;

            var inverterMessage = new InverterMessage(this.inverterStatus.SystemIndex, (short)InverterParameterId.ControlWordParam, this.inverterStatus.CommonControlWord.Value);

            this.logger.LogTrace($"2:inverterMessage={inverterMessage}");

            this.ParentStateMachine.EnqueueMessage(inverterMessage);

            var notificationMessage = new FieldNotificationMessage(null,
                $"Positioning Start",
                FieldMessageActor.Any,
                FieldMessageActor.InverterDriver,
                FieldMessageType.InverterStop,
                MessageStatus.OperationStart);

            this.logger.LogTrace($"3:Publishing Field Notification Message {notificationMessage.Type} Destination {notificationMessage.Destination} Status {notificationMessage.Status}");

            this.ParentStateMachine.PublishNotificationEvent(notificationMessage);

            this.logger.LogDebug("4:Method End");
        }

        /// <inheritdoc />
        public override bool ValidateCommandMessage(InverterMessage message)
        {
            this.logger.LogDebug("1:Method Start");

            this.logger.LogTrace($"2:message={message}:Is Error={message.IsError}");

            this.logger.LogDebug("3:Method End");

            return true;
        }

        public override bool ValidateCommandResponse(InverterMessage message)
        {
            this.logger.LogDebug("1:Method Start");
            this.logger.LogTrace($"2:message={message}:Is Error={message.IsError}");

            var returnValue = false;

            if (message.IsError)
            {
                this.ParentStateMachine.ChangeState(new VerticalPositioningErrorState(this.ParentStateMachine, this.inverterStatus, this.logger));
            }

            this.inverterStatus.CommonStatusWord.Value = message.UShortPayload;

            if (!this.inverterStatus.CommonStatusWord.IsQuickStopTrue)
            {
                this.ParentStateMachine.ChangeState(new VerticalPositioningEnableOperationState(this.ParentStateMachine, this.data,
                    this.InverterCommandQueue, this.inverterStatus, this.logger));
                returnValue = true;
            }

            this.logger.LogDebug("3:Method End");

            return returnValue;
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
