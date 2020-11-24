using System;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.InverterDriver.Contracts;
using Ferretto.VW.MAS.InverterDriver.InverterStatus.Interfaces;
using Ferretto.VW.MAS.Utils.Enumerations;
using Ferretto.VW.MAS.Utils.Messages;
using Ferretto.VW.MAS.Utils.Messages.FieldData;
using Microsoft.Extensions.Logging;


namespace Ferretto.VW.MAS.InverterDriver.StateMachines.SwitchOn
{
    internal class SwitchOnEndState : InverterStateBase
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

        #region Methods

        public override void Start()
        {
            var inverterIndex = this.InverterStatus.SystemIndex;
            this.Logger.LogDebug($"Switch On End Inverter {this.InverterStatus.SystemIndex}");

            var notificationMessageData = new InverterSwitchOnFieldMessageData(this.axisToSwitchOn);
            var notificationMessage = new FieldNotificationMessage(
                notificationMessageData,
                $"Inverter Switch On on axis {this.axisToSwitchOn} End",
                FieldMessageActor.InverterDriver,
                FieldMessageActor.InverterDriver,
                FieldMessageType.InverterSwitchOn,
                MessageStatus.OperationEnd,
                inverterIndex);

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

        public override bool ValidateCommandResponse(InverterMessage message)
        {
            this.Logger.LogTrace($"1:message={message}:Is Error={message.IsError}");

            return true;
        }

        #endregion
    }
}
