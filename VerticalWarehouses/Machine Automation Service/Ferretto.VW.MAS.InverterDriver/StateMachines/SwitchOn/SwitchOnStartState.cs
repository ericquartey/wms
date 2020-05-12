﻿using System;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataLayer;
using Ferretto.VW.MAS.InverterDriver.Contracts;
using Ferretto.VW.MAS.InverterDriver.InverterStatus.Interfaces;
using Ferretto.VW.MAS.Utils.Enumerations;
using Ferretto.VW.MAS.Utils.Messages;
using Ferretto.VW.MAS.Utils.Messages.FieldData;
using Microsoft.Extensions.Logging;

namespace Ferretto.VW.MAS.InverterDriver.StateMachines.SwitchOn
{
    internal class SwitchOnStartState : InverterStateBase
    {
        #region Fields

        private readonly Axis axisToSwitchOn;

        private DateTime startTime;

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
        }

        #endregion

        #region Methods

        public override void Start()
        {
            this.Logger.LogDebug($"Switch On Start Inverter {this.InverterStatus.SystemIndex}");
            this.startTime = DateTime.UtcNow;
            this.InverterStatus.CommonControlWord.SwitchOn = true;
            this.InverterStatus.CommonControlWord.HorizontalAxis =
                this.ParentStateMachine.GetRequiredService<IMachineVolatileDataProvider>().IsOneTonMachine.Value
                ? false
                : this.axisToSwitchOn == Axis.Horizontal;

            var inverterMessage = new InverterMessage(this.InverterStatus.SystemIndex, (short)InverterParameterId.ControlWord, this.InverterStatus.CommonControlWord.Value);

            this.Logger.LogTrace($"1:inverterMessage={inverterMessage}");

            this.ParentStateMachine.EnqueueCommandMessage(inverterMessage);

            var inverterIndex = this.InverterStatus.SystemIndex;

            var notificationMessageData = new InverterSwitchOnFieldMessageData(this.axisToSwitchOn);
            var notificationMessage = new FieldNotificationMessage(
                notificationMessageData,
                $"Switch On Inverter for axis {this.axisToSwitchOn}",
                FieldMessageActor.DeviceManager,
                FieldMessageActor.InverterDriver,
                FieldMessageType.InverterSwitchOn,
                MessageStatus.OperationStart,
                inverterIndex);

            this.Logger.LogTrace($"2:Publishing Field Notification Message {notificationMessage.Type} Destination {notificationMessage.Destination} Status {notificationMessage.Status}");

            this.ParentStateMachine.PublishNotificationEvent(notificationMessage);
        }

        /// <inheritdoc />
        public override void Stop()
        {
            this.Logger.LogDebug("1:Switch On Stop requested");

            this.ParentStateMachine.ChangeState(
                new SwitchOnEndState(
                    this.ParentStateMachine,
                    this.axisToSwitchOn,
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
                this.Logger.LogError($"1:SwitchOnStartState message={message}");
                this.ParentStateMachine.ChangeState(new SwitchOnErrorState(this.ParentStateMachine, this.axisToSwitchOn, this.InverterStatus, this.Logger));
            }
            else
            {
                this.Logger.LogTrace($"2:message={message}:Parameter Id={message.ParameterId}");
                if (this.InverterStatus.CommonStatusWord.IsSwitchedOn
                    && DateTime.UtcNow.Subtract(this.startTime).TotalMilliseconds > 500)
                {
                    this.ParentStateMachine.ChangeState(new SwitchOnEndState(this.ParentStateMachine, this.axisToSwitchOn, this.InverterStatus, this.Logger));
                    returnValue = true;
                }
                else if (DateTime.UtcNow.Subtract(this.startTime).TotalMilliseconds > 2000)
                {
                    this.Logger.LogError($"2:SwitchOnStartState timeout, inverter {this.InverterStatus.SystemIndex}");
                    this.ParentStateMachine.ChangeState(new SwitchOnErrorState(this.ParentStateMachine, this.axisToSwitchOn, this.InverterStatus, this.Logger));
                }
            }
            return returnValue;
        }

        #endregion
    }
}
