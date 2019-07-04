using System;
using Ferretto.VW.Common_Utils.Messages.Enumerations;
using Ferretto.VW.MAS_InverterDriver.Enumerations;
using Ferretto.VW.MAS_InverterDriver.Interface.StateMachines;
using Ferretto.VW.MAS_InverterDriver.InverterStatus.Interfaces;
using Ferretto.VW.MAS_Utils.Enumerations;
using Ferretto.VW.MAS_Utils.Messages;
using Ferretto.VW.MAS_Utils.Messages.FieldData;
using Microsoft.Extensions.Logging;

// ReSharper disable ArrangeThisQualifier

namespace Ferretto.VW.MAS_InverterDriver.StateMachines.SwitchOff
{
    public class SwitchOffStartState : InverterStateBase
    {
        #region Fields

        private readonly IInverterStatusBase inverterStatus;

        private readonly ILogger logger;

        private bool disposed;

        #endregion

        #region Constructors

        public SwitchOffStartState(IInverterStateMachine parentStateMachine, IInverterStatusBase inverterStatus, ILogger logger)
        {
            logger.LogTrace("1:Method Start");
            this.logger = logger;

            this.ParentStateMachine = parentStateMachine;
            this.inverterStatus = inverterStatus;
        }

        #endregion

        #region Destructors

        ~SwitchOffStartState()
        {
            this.Dispose(false);
        }

        #endregion

        #region Methods

        public override void Release()
        {
        }

        public override void Start()
        {
            this.inverterStatus.CommonControlWord.SwitchOn = false;

            var inverterMessage = new InverterMessage(this.inverterStatus.SystemIndex, (short)InverterParameterId.ControlWordParam, this.inverterStatus.CommonControlWord.Value);

            this.logger.LogTrace($"1:inverterMessage={inverterMessage}");

            this.ParentStateMachine.EnqueueMessage(inverterMessage);

            Enum.TryParse(this.inverterStatus.SystemIndex.ToString(), out InverterIndex inverterIndex);

            var notificationMessageData = new InverterSwitchOffFieldMessageData(inverterIndex);
            var notificationMessage = new FieldNotificationMessage(notificationMessageData,
                $"SwitchOff Inverter {inverterIndex}",
                FieldMessageActor.Any,
                FieldMessageActor.InverterDriver,
                FieldMessageType.InverterSwitchOff,
                MessageStatus.OperationStart);

            this.logger.LogTrace($"2:Publishing Field Notification Message {notificationMessage.Type} Destination {notificationMessage.Destination} Status {notificationMessage.Status}");

            this.ParentStateMachine.PublishNotificationEvent(notificationMessage);
        }

        /// <inheritdoc />
        public override bool ValidateCommandMessage(InverterMessage message)
        {
            this.logger.LogTrace($"1:message={message}:Is Error={message.IsError}");

            return true;
        }

        public override bool ValidateCommandResponse(InverterMessage message)
        {
            this.logger.LogTrace($"1:message={message}:Is Error={message.IsError}");

            var returnValue = false;

            if (message.IsError)
            {
                this.ParentStateMachine.ChangeState(new SwitchOffErrorState(this.ParentStateMachine, this.inverterStatus, this.logger));
            }

            this.inverterStatus.CommonStatusWord.Value = message.UShortPayload;

            if (!this.inverterStatus.CommonStatusWord.IsSwitchedOn)
            {
                this.ParentStateMachine.ChangeState(new SwitchOffEndState(this.ParentStateMachine, this.inverterStatus, this.logger));
                returnValue = true;
            }

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
