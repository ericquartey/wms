using System;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataLayer;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS.InverterDriver.Contracts;

using Ferretto.VW.MAS.InverterDriver.InverterStatus.Interfaces;
using Ferretto.VW.MAS.Utils.Enumerations;
using Ferretto.VW.MAS.Utils.Messages;
using Ferretto.VW.MAS.Utils.Messages.FieldData;
using Microsoft.Extensions.Logging;

namespace Ferretto.VW.MAS.InverterDriver.StateMachines.PowerOff
{
    internal class PowerOffStartState : InverterStateBase
    {
        #region Fields

        private readonly IErrorsProvider errorProvider;

        private DateTime startTime;

        #endregion

        #region Constructors

        public PowerOffStartState(
            IInverterStateMachine parentStateMachine,
            IInverterStatusBase inverterStatus,
            ILogger logger)
            : base(parentStateMachine, inverterStatus, logger)
        {
            this.errorProvider = this.ParentStateMachine.GetRequiredService<IErrorsProvider>();
        }

        #endregion

        #region Methods

        public override void Start()
        {
            this.startTime = DateTime.UtcNow;
            this.InverterStatus.CommonControlWord.EnableOperation = false;

            var inverterMessage = new InverterMessage(this.InverterStatus.SystemIndex, (short)InverterParameterId.ControlWord, this.InverterStatus.CommonControlWord.Value);

            this.Logger.LogTrace($"1:inverterMessage={inverterMessage}");

            this.ParentStateMachine.EnqueueCommandMessage(inverterMessage);

            var notificationMessageData = new InverterPowerOffFieldMessageData();
            var notificationMessage = new FieldNotificationMessage(
                notificationMessageData,
                $"PowerOff Inverter {this.InverterStatus.SystemIndex}",
                FieldMessageActor.Any,
                FieldMessageActor.InverterDriver,
                FieldMessageType.InverterPowerOff,
                MessageStatus.OperationStart,
                this.InverterStatus.SystemIndex);

            this.Logger.LogTrace($"2:Publishing Field Notification Message {notificationMessage.Type} Destination {notificationMessage.Destination} Status {notificationMessage.Status}");

            this.ParentStateMachine.PublishNotificationEvent(notificationMessage);
        }

        /// <inheritdoc />
        public override void Stop()
        {
            this.Logger.LogDebug("1:Power Off Stop requested");

            this.ParentStateMachine.ChangeState(
                new PowerOffEndState(
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
                this.Logger.LogError($"1:message={message}");
                this.ParentStateMachine.ChangeState(new PowerOffErrorState(this.ParentStateMachine, this.InverterStatus, this.Logger));
            }
            else
            {
                this.Logger.LogDebug($"2:message={message}:Parameter Id={message.ParameterId}");
                if (!this.InverterStatus.CommonStatusWord.IsOperationEnabled)
                {
                    this.ParentStateMachine.ChangeState(new PowerOffEndState(this.ParentStateMachine, this.InverterStatus, this.Logger));
                    returnValue = true;
                }
                else if (DateTime.UtcNow.Subtract(this.startTime).TotalMilliseconds > 2500)
                {
                    this.Logger.LogError($"2:PowerOffStartState timeout, inverter {this.InverterStatus.SystemIndex}");
                    this.errorProvider.RecordNew(MachineErrorCode.InverterCommandTimeout, additionalText: $"Power Off Inverter {this.InverterStatus.SystemIndex}");
                    this.ParentStateMachine.ChangeState(
                        new PowerOffErrorState(this.ParentStateMachine, this.InverterStatus, this.Logger));
                }
            }
            return returnValue;
        }

        #endregion
    }
}
