using System;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS_InverterDriver.Enumerations;
using Ferretto.VW.MAS_InverterDriver.Interface.StateMachines;
using Ferretto.VW.MAS_InverterDriver.InverterStatus.Interfaces;
using Ferretto.VW.MAS_Utils.Enumerations;
using Ferretto.VW.MAS_Utils.Messages;
using Ferretto.VW.MAS_Utils.Messages.FieldData;
using Microsoft.Extensions.Logging;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS_InverterDriver.StateMachines.PowerOff
{
    public class PowerOffStartState : InverterStateBase
    {
        #region Constructors

        public PowerOffStartState(
            IInverterStateMachine parentStateMachine,
            IInverterStatusBase inverterStatus,
            ILogger logger)
            : base(parentStateMachine, inverterStatus, logger)
        {
        }

        #endregion

        #region Destructors

        ~PowerOffStartState()
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
            this.InverterStatus.CommonControlWord.QuickStop = false;

            var inverterMessage = new InverterMessage(this.InverterStatus.SystemIndex, (short)InverterParameterId.ControlWordParam, this.InverterStatus.CommonControlWord.Value);

            this.Logger.LogTrace($"1:inverterMessage={inverterMessage}");

            this.ParentStateMachine.EnqueueMessage(inverterMessage);

            Enum.TryParse(this.InverterStatus.SystemIndex.ToString(), out InverterIndex inverterIndex);

            var notificationMessageData = new InverterPowerOffFieldMessageData(inverterIndex);
            var notificationMessage = new FieldNotificationMessage(
                notificationMessageData,
                $"PowerOff Inverter {inverterIndex}",
                FieldMessageActor.Any,
                FieldMessageActor.InverterDriver,
                FieldMessageType.InverterPowerOff,
                MessageStatus.OperationStart);

            this.Logger.LogTrace($"2:Publishing Field Notification Message {notificationMessage.Type} Destination {notificationMessage.Destination} Status {notificationMessage.Status}");

            this.ParentStateMachine.PublishNotificationEvent(notificationMessage);
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

            var returnValue = false;

            if (message.IsError)
            {
                this.ParentStateMachine.ChangeState(new PowerOffErrorState(this.ParentStateMachine, this.InverterStatus, this.Logger));
            }

            this.InverterStatus.CommonStatusWord.Value = message.UShortPayload;

            if (!this.InverterStatus.CommonStatusWord.IsQuickStopTrue)
            {
                this.ParentStateMachine.ChangeState(new PowerOffDisableOperationState(this.ParentStateMachine, this.InverterStatus, this.Logger));
                returnValue = true;
            }

            return returnValue;
        }

        #endregion
    }
}
