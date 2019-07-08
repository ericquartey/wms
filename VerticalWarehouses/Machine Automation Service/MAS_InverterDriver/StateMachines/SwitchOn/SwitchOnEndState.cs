using System;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS_InverterDriver.Interface.StateMachines;
using Ferretto.VW.MAS_InverterDriver.InverterStatus.Interfaces;
using Ferretto.VW.MAS_Utils.Enumerations;
using Ferretto.VW.MAS_Utils.Messages;
using Ferretto.VW.MAS_Utils.Messages.FieldData;
using Microsoft.Extensions.Logging;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS_InverterDriver.StateMachines.SwitchOn
{
    public class SwitchOnEndState : InverterStateBase
    {
        #region Fields

        private readonly Axis axisToSwitchOn;

        #endregion

        #region Constructors

        public SwitchOnEndState(
            IInverterStateMachine parentStateMachine,
            Axis axisToSwitchOn,
            IInverterStatusBase inverterStatus,
            ILogger logger)
            : base(parentStateMachine, inverterStatus, logger)
        {
            this.axisToSwitchOn = axisToSwitchOn;
        }

        #endregion

        #region Destructors

        ~SwitchOnEndState()
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
            Enum.TryParse(this.InverterStatus.SystemIndex.ToString(), out InverterIndex inverterIndex);

            var notificationMessageData = new InverterSwitchOnFieldMessageData(this.axisToSwitchOn, inverterIndex);
            var notificationMessage = new FieldNotificationMessage(
                notificationMessageData,
                $"Inverter Switch On on axis {this.axisToSwitchOn} End",
                FieldMessageActor.Any,
                FieldMessageActor.InverterDriver,
                FieldMessageType.InverterSwitchOn,
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

        public override bool ValidateCommandResponse(InverterMessage message)
        {
            this.Logger.LogTrace($"1:message={message}:Is Error={message.IsError}");

            return true;
        }

        #endregion
    }
}
