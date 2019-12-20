using System;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.InverterDriver.Contracts;

using Ferretto.VW.MAS.InverterDriver.InverterStatus.Interfaces;
using Ferretto.VW.MAS.Utils.Enumerations;
using Ferretto.VW.MAS.Utils.Messages;
using Ferretto.VW.MAS.Utils.Messages.FieldData;
using Microsoft.Extensions.Logging;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS.InverterDriver.StateMachines.PowerOn
{
    internal class PowerOnStartState : InverterStateBase
    {
        #region Fields

        private readonly DateTime startTime;

        #endregion

        #region Constructors

        public PowerOnStartState(
            IInverterStateMachine parentStateMachine,
            IInverterStatusBase inverterStatus,
            ILogger logger)
            : base(parentStateMachine, inverterStatus, logger)
        {
            this.Logger.LogTrace("1:Method Start");
            this.startTime = DateTime.UtcNow;
        }

        #endregion

        #region Methods

        public override void Start()
        {
            this.Logger.LogDebug($"Power On Start Inverter {this.InverterStatus.SystemIndex}");
            this.InverterStatus.CommonControlWord.EnableVoltage = true;
            this.InverterStatus.CommonControlWord.QuickStop = true;

            var inverterMessage = new InverterMessage(this.InverterStatus.SystemIndex, (short)InverterParameterId.ControlWord, this.InverterStatus.CommonControlWord.Value);

            this.Logger.LogTrace($"1:inverterMessage={inverterMessage}");

            this.ParentStateMachine.EnqueueCommandMessage(inverterMessage);

            var notificationMessageData = new InverterPowerOnFieldMessageData();
            var notificationMessage = new FieldNotificationMessage(
                notificationMessageData,
                $"Power On Inverter {this.InverterStatus.SystemIndex}",
                FieldMessageActor.Any,
                FieldMessageActor.InverterDriver,
                FieldMessageType.InverterPowerOn,
                MessageStatus.OperationStart,
                this.InverterStatus.SystemIndex);

            this.Logger.LogTrace($"2:Publishing Field Notification Message {notificationMessage.Type} Destination {notificationMessage.Destination} Status {notificationMessage.Status}");

            this.ParentStateMachine.PublishNotificationEvent(notificationMessage);
        }

        /// <inheritdoc />
        public override void Stop()
        {
            this.Logger.LogDebug("1:Power On Stop requested");

            this.ParentStateMachine.ChangeState(
                new PowerOnEndState(
                    this.ParentStateMachine,
                    this.InverterStatus,
                    this.Logger));
        }

        public override bool ValidateCommandMessage(InverterMessage message)
        {
            this.Logger.LogTrace($"1:message={message}:Is Error={message.IsError}");

            return true;
        }

        public override bool ValidateCommandResponse(InverterMessage message)
        {
            var responseReceived = false;

            if (message.IsError)
            {
                this.Logger.LogError($"1:PowerOnStartState, message={message}");
                this.ParentStateMachine.ChangeState(
                    new PowerOnErrorState(this.ParentStateMachine, this.InverterStatus, this.Logger));
            }
            else
            {
                this.Logger.LogTrace($"2:message={message}:Parameter Id={message.ParameterId}");
                if (this.InverterStatus.CommonStatusWord.IsVoltageEnabled &
                    this.InverterStatus.CommonStatusWord.IsQuickStopTrue &
                    this.InverterStatus.CommonStatusWord.IsReadyToSwitchOn
                    )
                {
                    this.ParentStateMachine.ChangeState(
                        new PowerOnSwitchOnState(this.ParentStateMachine, this.InverterStatus, this.Logger));
                    responseReceived = true;
                }
                else if (DateTime.UtcNow.Subtract(this.startTime).TotalMilliseconds > 2500)
                {
                    this.Logger.LogError($"2:PowerOnStartState timeout, inverter {this.InverterStatus.SystemIndex}");
                    this.ParentStateMachine.ChangeState(
                        new PowerOnErrorState(this.ParentStateMachine, this.InverterStatus, this.Logger));
                }
            }
            return responseReceived;
        }

        #endregion
    }
}
