using System;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataLayer;
using Ferretto.VW.MAS.InverterDriver.Contracts;

using Ferretto.VW.MAS.InverterDriver.InverterStatus.Interfaces;
using Ferretto.VW.MAS.Utils.Enumerations;
using Ferretto.VW.MAS.Utils.Messages;
using Ferretto.VW.MAS.Utils.Messages.FieldData;
using Microsoft.Extensions.Logging;

namespace Ferretto.VW.MAS.InverterDriver.StateMachines.PowerOn
{
    internal class PowerOnStartState : InverterStateBase
    {
        #region Fields

        private readonly Axis axisToSwitchOn;

        private bool isAxisChanged;

        private DateTime startTime;

        #endregion

        #region Constructors

        public PowerOnStartState(
            IInverterStateMachine parentStateMachine,
            Axis axisToSwitchOn,
            IInverterStatusBase inverterStatus,
            ILogger logger)
            : base(parentStateMachine, inverterStatus, logger)
        {
            this.Logger.LogTrace("1:Method Start");
            this.axisToSwitchOn = axisToSwitchOn;
        }

        #endregion

        #region Methods

        public override void Start()
        {
            this.Logger.LogDebug($"Power On Start Inverter {this.InverterStatus.SystemIndex}");
            this.startTime = DateTime.UtcNow;
            var oldAxis = this.InverterStatus.CommonControlWord.HorizontalAxis;

            this.InverterStatus.CommonControlWord.HorizontalAxis = this.ParentStateMachine.GetRequiredService<IMachineVolatileDataProvider>().IsOneTonMachine.Value
                ? false
                : this.axisToSwitchOn == Axis.Horizontal;

            //this.InverterStatus.CommonControlWord.EnableVoltage = true;
            //this.InverterStatus.CommonControlWord.QuickStop = true;

            var inverterMessage = new InverterMessage(this.InverterStatus.SystemIndex, (short)InverterParameterId.ControlWord, this.InverterStatus.CommonControlWord.Value);

            if (this.InverterStatus.SystemIndex == 0
                && oldAxis != this.InverterStatus.CommonControlWord.HorizontalAxis
                && this.ParentStateMachine.GetRequiredService<IMachineProvider>().IsAxisChanged()
                )
            {
                this.Logger.LogTrace($"1:inverterMessage={inverterMessage}");

                this.ParentStateMachine.EnqueueCommandMessage(inverterMessage);

                // read AxisChanged parameter
                inverterMessage = new InverterMessage(this.InverterStatus.SystemIndex, InverterParameterId.AxisChanged, InverterDataset.AxisChangeDatasetRead);

                this.Logger.LogTrace($"2:inverterMessage={inverterMessage}");

                this.ParentStateMachine.EnqueueCommandMessage(inverterMessage);
            }
            else
            {
                this.isAxisChanged = true;
                this.InverterStatus.CommonControlWord.EnableVoltage = true;
                this.InverterStatus.CommonControlWord.QuickStop = true;

                inverterMessage = new InverterMessage(this.InverterStatus.SystemIndex, (short)InverterParameterId.ControlWord, this.InverterStatus.CommonControlWord.Value);

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

            return false;
        }

        public override bool ValidateCommandResponse(InverterMessage message)
        {
            if (message.IsError)
            {
                this.Logger.LogError($"1:PowerOnStartState, message={message}");
                this.ParentStateMachine.ChangeState(
                    new PowerOnErrorState(this.ParentStateMachine, this.InverterStatus, this.Logger));
            }
            else
            {
                this.Logger.LogDebug($"2:message={message}:Parameter Id={message.ParameterId}");

                if (message.ParameterId == InverterParameterId.AxisChanged)
                {
                    if (message.UShortPayload == 0)
                    {
                        // read again
                        var inverterMessage = new InverterMessage(this.InverterStatus.SystemIndex, InverterParameterId.AxisChanged, InverterDataset.AxisChangeDatasetRead);

                        this.Logger.LogTrace($"1:inverterMessage={inverterMessage}");

                        this.ParentStateMachine.EnqueueCommandMessage(inverterMessage);
                    }
                    else
                    {
                        // ack: write response
                        var inverterMessage = new InverterMessage(this.InverterStatus.SystemIndex, (short)InverterParameterId.AxisChanged, message.UShortPayload, InverterDataset.AxisChangeDatasetWrite);

                        this.Logger.LogDebug($"1:inverterMessage={inverterMessage}");

                        this.ParentStateMachine.EnqueueCommandMessage(inverterMessage);

                        this.isAxisChanged = true;

                        // power on axis
                        this.InverterStatus.CommonControlWord.EnableVoltage = true;
                        this.InverterStatus.CommonControlWord.QuickStop = true;

                        inverterMessage = new InverterMessage(this.InverterStatus.SystemIndex, (short)InverterParameterId.ControlWord, this.InverterStatus.CommonControlWord.Value);

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
                }
                else if (this.InverterStatus.CommonStatusWord.IsVoltageEnabled &&
                    this.InverterStatus.CommonStatusWord.IsQuickStopTrue &&
                    this.InverterStatus.CommonStatusWord.IsReadyToSwitchOn &&
                    this.isAxisChanged
                    )
                {
                    this.ParentStateMachine.ChangeState(
                        new PowerOnSwitchOnState(this.ParentStateMachine, this.InverterStatus, this.Logger));
                }
                else if (DateTime.UtcNow.Subtract(this.startTime).TotalMilliseconds > 2000)
                {
                    this.Logger.LogError($"2:PowerOnStartState timeout, inverter {this.InverterStatus.SystemIndex}");
                    this.ParentStateMachine.ChangeState(
                        new PowerOnErrorState(this.ParentStateMachine, this.InverterStatus, this.Logger));
                }
            }
            return true;
        }

        #endregion
    }
}
