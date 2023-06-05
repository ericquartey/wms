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

namespace Ferretto.VW.MAS.InverterDriver.StateMachines.ResetFault
{
    internal class ResetFaultStartState : InverterStateBase
    {
        #region Fields

        private readonly IErrorsProvider errorProvider;

        private readonly InverterIndex inverterIndex;

        private readonly int inverterResponseTimeout;

        private readonly IMachineProvider machineProvider;

        private DateTime startTime;

        #endregion

        #region Constructors

        public ResetFaultStartState(
            IInverterStateMachine parentStateMachine,
            IInverterStatusBase inverterStatus,
            InverterIndex inverterIndex,
            ILogger logger)
            : base(parentStateMachine, inverterStatus, logger)
        {
            this.inverterIndex = inverterIndex;
            this.errorProvider = this.ParentStateMachine.GetRequiredService<IErrorsProvider>();
            this.machineProvider = this.ParentStateMachine.GetRequiredService<IMachineProvider>();

            this.inverterResponseTimeout = this.machineProvider.GetInverterResponseTimeout();
        }

        #endregion

        #region Methods

        public override void Start()
        {
            this.Logger.LogDebug($"Reset Fault Start Inverter {this.inverterIndex}");
            this.startTime = DateTime.UtcNow;
            this.InverterStatus.CommonControlWord.Value = 0;
            this.InverterStatus.CommonControlWord.FaultReset = true;

            var inverterMessage = new InverterMessage(
                this.InverterStatus.SystemIndex,
                (short)InverterParameterId.ControlWord,
                this.InverterStatus.CommonControlWord.Value);

            this.Logger.LogTrace($"1:inverterMessage={inverterMessage}");

            this.ParentStateMachine.EnqueueCommandMessage(inverterMessage);

            var notificationMessageData = new InverterFaultFieldMessageData();
            var notificationMessage = new FieldNotificationMessage(
                notificationMessageData,
                $"Fault Reset Inverter {this.inverterIndex}",
                FieldMessageActor.Any,
                FieldMessageActor.InverterDriver,
                FieldMessageType.InverterFaultReset,
                MessageStatus.OperationStart,
                this.InverterStatus.SystemIndex);

            this.Logger.LogTrace($"2:Publishing Field Notification Message {notificationMessage.Type} Destination {notificationMessage.Destination} Status {notificationMessage.Status}");

            this.ParentStateMachine.PublishNotificationEvent(notificationMessage);
        }

        /// <inheritdoc />
        public override void Stop()
        {
            this.Logger.LogDebug("1:ResetFault Stop requested");

            this.ParentStateMachine.ChangeState(
                new ResetFaultEndState(
                    this.ParentStateMachine,
                    this.InverterStatus,
                    this.inverterIndex,
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
                this.Logger.LogError($"1:ResetFaultStartState message={message}");
                this.ParentStateMachine.ChangeState(new ResetFaultErrorState(this.ParentStateMachine, this.InverterStatus, this.inverterIndex, this.Logger));
            }
            else
            {
                this.Logger.LogTrace($"2:message={message}:Parameter Id={message.ParameterId}");
                if (this.InverterStatus == null)
                {
                    this.ParentStateMachine.ChangeState(new ResetFaultEndState(this.ParentStateMachine, this.InverterStatus, this.inverterIndex, this.Logger));
                }
                else if (!this.InverterStatus.CommonStatusWord.IsFault
                    || DateTime.UtcNow.Subtract(this.startTime).TotalMilliseconds > this.inverterResponseTimeout)
                {
                    if (this.InverterStatus.CommonStatusWord.IsFault)
                    {
                        this.errorProvider.RecordNew(MachineErrorCode.InverterCommandTimeout, additionalText: $"Reset Fault Inverter {this.inverterIndex}");
                        this.Logger.LogError($"2:ResetFaultStartState timeout, inverter {this.inverterIndex}");
                    }
                    // reset command FaultReset bit before exiting the state machine

                    this.InverterStatus.CommonControlWord.FaultReset = false;

                    var inverterMessage = new InverterMessage(this.InverterStatus.SystemIndex, (short)InverterParameterId.ControlWord, this.InverterStatus.CommonControlWord.Value);

                    this.Logger.LogTrace($"2:inverterMessage={inverterMessage}");

                    this.ParentStateMachine.EnqueueCommandMessage(inverterMessage);

                    this.ParentStateMachine.ChangeState(new ResetFaultEndState(this.ParentStateMachine, this.InverterStatus, this.inverterIndex, this.Logger));

                    returnValue = true;
                }
            }
            return returnValue;
        }

        #endregion
    }
}
