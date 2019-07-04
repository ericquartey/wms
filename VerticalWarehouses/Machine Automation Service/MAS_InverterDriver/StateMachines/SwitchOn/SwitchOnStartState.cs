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
namespace Ferretto.VW.MAS_InverterDriver.StateMachines.SwitchOn
{
    public class SwitchOnStartState : InverterStateBase
    {
        #region Fields

        private readonly Axis axisToSwitchOn;

        #endregion

        #region Constructors

        public SwitchOnStartState(
            IInverterStateMachine parentStateMachine,
            Axis axisToSwitchOn,
            IInverterStatusBase inverterStatus,
            ILogger logger)
            : base(parentStateMachine, inverterStatus, logger)
        {
            this.axisToSwitchOn = axisToSwitchOn;

            logger.LogTrace("1:Method Start");
        }

        #endregion

        #region Destructors

        ~SwitchOnStartState()
        {
            this.Dispose(false);
        }

        #endregion

        #region Methods

        public override void Start()
        {
            this.InverterStatus.CommonControlWord.SwitchOn = true;
            this.InverterStatus.CommonControlWord.HorizontalAxis = this.axisToSwitchOn == Axis.Horizontal;

            var inverterMessage = new InverterMessage(this.InverterStatus.SystemIndex, (short)InverterParameterId.ControlWordParam, this.InverterStatus.CommonControlWord.Value);

            this.Logger.LogTrace($"1:inverterMessage={inverterMessage}");

            this.ParentStateMachine.EnqueueMessage(inverterMessage);

            Enum.TryParse(this.InverterStatus.SystemIndex.ToString(), out InverterIndex inverterIndex);

            var notificationMessageData = new InverterSwitchOnFieldMessageData(this.axisToSwitchOn, inverterIndex);
            var notificationMessage = new FieldNotificationMessage(
                notificationMessageData,
                $"Switch On Inverter for axis {this.axisToSwitchOn}",
                FieldMessageActor.FiniteStateMachines,
                FieldMessageActor.InverterDriver,
                FieldMessageType.InverterSwitchOn,
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
                this.ParentStateMachine.ChangeState(new SwitchOnErrorState(this.ParentStateMachine, this.axisToSwitchOn, this.InverterStatus, this.Logger));
            }

            this.InverterStatus.CommonStatusWord.Value = message.UShortPayload;

            if (this.InverterStatus.CommonStatusWord.IsSwitchedOn)
            {
                this.ParentStateMachine.ChangeState(new SwitchOnEndState(this.ParentStateMachine, this.axisToSwitchOn, this.InverterStatus, this.Logger));
                returnValue = true;
            }

            return returnValue;
        }

        #endregion
    }
}
