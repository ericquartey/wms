using System;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.InverterDriver.Contracts;

using Ferretto.VW.MAS.InverterDriver.InverterStatus.Interfaces;
using Ferretto.VW.MAS.Utils.Enumerations;
using Ferretto.VW.MAS.Utils.Messages;
using Microsoft.Extensions.Logging;

namespace Ferretto.VW.MAS.InverterDriver.StateMachines.Stop
{
    internal class StopStartState : InverterStateBase
    {
        #region Fields

        private DateTime startTime;

        #endregion

        #region Constructors

        public StopStartState(
            IInverterStateMachine parentStateMachine,
            IInverterStatusBase inverterStatus,
            ILogger logger)
            : base(parentStateMachine, inverterStatus, logger)
        {
        }

        #endregion

        #region Methods

        public override void Start()
        {
            this.Logger.LogDebug($"Stop Inverter Start state {this.InverterStatus.SystemIndex}");
            this.startTime = DateTime.UtcNow;
            this.InverterStatus.CommonControlWord.SwitchOn = false;

            var inverterMessage = new InverterMessage(this.InverterStatus.SystemIndex, (short)InverterParameterId.ControlWord, this.InverterStatus.CommonControlWord.Value);

            this.Logger.LogTrace($"1:inverterMessage={inverterMessage}");

            this.ParentStateMachine.EnqueueCommandMessage(inverterMessage);

            var inverterIndex = this.InverterStatus.SystemIndex;

            var notificationMessage = new FieldNotificationMessage(
                null,
                $"Stop Inverter {inverterIndex}",
                FieldMessageActor.Any,
                FieldMessageActor.InverterDriver,
                FieldMessageType.InverterStop,
                MessageStatus.OperationStart,
                inverterIndex);

            this.Logger.LogTrace($"2:Publishing Field Notification Message {notificationMessage.Type} Destination {notificationMessage.Destination} Status {notificationMessage.Status}");

            this.ParentStateMachine.PublishNotificationEvent(notificationMessage);
        }

        /// <inheritdoc />
        public override void Stop()
        {
            this.Logger.LogDebug("1:Stop Inverter Stop requested");

            this.ParentStateMachine.ChangeState(
                new StopEndState(
                    this.ParentStateMachine,
                    this.InverterStatus,
                    this.Logger));
        }

        /// <inheritdoc />
        public override bool ValidateCommandMessage(InverterMessage message)
        {
            this.Logger.LogTrace($"1:message={message}:Is Error={message.IsError}");

            return true;
        }

        public override bool ValidateCommandResponse(InverterMessage message)
        {
            var returnValue = false;

            if (message.IsError)
            {
                this.Logger.LogError($"1:StopStartState message={message}");
                this.ParentStateMachine.ChangeState(new StopErrorState(this.ParentStateMachine, this.InverterStatus, this.Logger));
            }
            else
            {
                this.Logger.LogTrace($"2:message={message}:Parameter Id={message.ParameterId}");
                if (!this.InverterStatus.CommonStatusWord.IsSwitchedOn)
                {
                    this.ParentStateMachine.ChangeState(new StopDisableOperationState(this.ParentStateMachine, this.InverterStatus, this.Logger));
                    returnValue = true;
                }
                else if (DateTime.UtcNow.Subtract(this.startTime).TotalMilliseconds > 2000)
                {
                    this.Logger.LogError($"2:StopStartState timeout, inverter {this.InverterStatus.SystemIndex}");
                    this.ParentStateMachine.ChangeState(new StopErrorState(this.ParentStateMachine, this.InverterStatus, this.Logger));
                }
            }
            return returnValue;
        }

        #endregion
    }
}
