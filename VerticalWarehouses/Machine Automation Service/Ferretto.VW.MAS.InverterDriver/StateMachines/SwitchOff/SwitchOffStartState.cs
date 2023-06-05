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

namespace Ferretto.VW.MAS.InverterDriver.StateMachines.SwitchOff
{
    internal class SwitchOffStartState : InverterStateBase
    {
        #region Fields

        private readonly IErrorsProvider errorProvider;

        private readonly int inverterResponseTimeout;

        private readonly IMachineProvider machineProvider;

        private DateTime startTime;

        #endregion

        #region Constructors

        public SwitchOffStartState(
                    IInverterStateMachine parentStateMachine,
            IInverterStatusBase inverterStatus,
            ILogger logger)
            : base(parentStateMachine, inverterStatus, logger)
        {
            this.errorProvider = this.ParentStateMachine.GetRequiredService<IErrorsProvider>();
            this.machineProvider = this.ParentStateMachine.GetRequiredService<IMachineProvider>();

            this.inverterResponseTimeout = this.machineProvider.GetInverterResponseTimeout();
        }

        #endregion

        #region Methods

        public override void Start()
        {
            this.Logger.LogDebug("SwitchOffStartState");

            this.InverterStatus.CommonControlWord.SwitchOn = false;
            this.startTime = DateTime.UtcNow;

            var inverterMessage = new InverterMessage(this.InverterStatus.SystemIndex, (short)InverterParameterId.ControlWord, this.InverterStatus.CommonControlWord.Value);

            this.Logger.LogTrace($"1:inverterMessage={inverterMessage}");

            this.ParentStateMachine.EnqueueCommandMessage(inverterMessage);

            var inverterIndex = this.InverterStatus.SystemIndex;

            var notificationMessageData = new InverterSwitchOffFieldMessageData();
            var notificationMessage = new FieldNotificationMessage(
                notificationMessageData,
                $"SwitchOff Inverter {inverterIndex}",
                FieldMessageActor.Any,
                FieldMessageActor.InverterDriver,
                FieldMessageType.InverterSwitchOff,
                MessageStatus.OperationStart,
                inverterIndex);

            this.Logger.LogTrace($"2:Publishing Field Notification Message {notificationMessage.Type} Destination {notificationMessage.Destination} Status {notificationMessage.Status}");

            this.ParentStateMachine.PublishNotificationEvent(notificationMessage);
        }

        /// <inheritdoc />
        public override void Stop()
        {
            this.Logger.LogDebug("1:Switch Off Stop requested");

            this.ParentStateMachine.ChangeState(
                new SwitchOffEndState(
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
                this.Logger.LogError($"1:SwitchOffStartState message={message}");
                this.errorProvider.RecordNew(MachineErrorCode.InverterErrorBaseCode);
                this.ParentStateMachine.ChangeState(new SwitchOffErrorState(this.ParentStateMachine, this.InverterStatus, this.Logger));
            }
            else
            {
                this.Logger.LogTrace($"2:message={message}:Parameter Id={message.ParameterId}");
                if (!this.InverterStatus.CommonStatusWord.IsSwitchedOn)
                {
                    this.ParentStateMachine.ChangeState(new SwitchOffWaitState(this.ParentStateMachine, this.InverterStatus, this.Logger));
                    returnValue = true;
                }
                else if (DateTime.UtcNow.Subtract(this.startTime).TotalMilliseconds > this.inverterResponseTimeout)
                {
                    this.Logger.LogError($"2:SwitchOffStartState timeout, inverter {this.InverterStatus.SystemIndex}");
                    this.errorProvider.RecordNew(MachineErrorCode.InverterCommandTimeout, additionalText: $"Switch Off Inverter {this.InverterStatus.SystemIndex}");
                    this.ParentStateMachine.ChangeState(new SwitchOffErrorState(this.ParentStateMachine, this.InverterStatus, this.Logger));
                }
            }
            return returnValue;
        }

        #endregion
    }
}
