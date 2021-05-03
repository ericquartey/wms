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

namespace Ferretto.VW.MAS.InverterDriver.StateMachines.SwitchOn
{
    internal class SwitchOnStartState : InverterStateBase
    {
        #region Fields

        private readonly Axis axisToSwitchOn;

        private readonly IErrorsProvider errorProvider;

        private bool isAxisChanged;

        private int minTimeout = 500;

        private DateTime startTime = DateTime.UtcNow;

        private bool waitAck;

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
            this.errorProvider = this.ParentStateMachine.GetRequiredService<IErrorsProvider>();
        }

        #endregion

        #region Methods

        public override void Start()
        {
            this.Logger.LogDebug($"Switch On Start Inverter {this.InverterStatus.SystemIndex}");
            this.startTime = DateTime.UtcNow;
            var oldAxis = this.InverterStatus.CommonControlWord.HorizontalAxis;
            this.InverterStatus.CommonControlWord.HorizontalAxis =
                !this.ParentStateMachine.GetRequiredService<IMachineVolatileDataProvider>().IsOneTonMachine.Value
                && this.axisToSwitchOn == Axis.Horizontal;

            var inverterMessage = new InverterMessage(this.InverterStatus.SystemIndex, (short)InverterParameterId.ControlWord, this.InverterStatus.CommonControlWord.Value);
            this.Logger.LogTrace($"1:inverterMessage={inverterMessage}");

            this.ParentStateMachine.EnqueueCommandMessage(inverterMessage);

            if (this.InverterStatus.SystemIndex == 0
                && !this.ParentStateMachine.GetRequiredService<IMachineVolatileDataProvider>().IsOneTonMachine.Value
                && oldAxis != this.InverterStatus.CommonControlWord.HorizontalAxis
                && this.ParentStateMachine.GetRequiredService<IMachineProvider>().IsAxisChanged()
                )
            {
                // read AxisChanged parameter
                inverterMessage = new InverterMessage(this.InverterStatus.SystemIndex, InverterParameterId.AxisChanged, InverterDataset.AxisChangeDatasetRead);

                this.Logger.LogDebug($"2:inverterMessage={inverterMessage}");

                this.ParentStateMachine.EnqueueCommandMessage(inverterMessage);

                this.minTimeout = 0;
            }
            else
            {
                this.isAxisChanged = true;
                if (this.InverterStatus.SystemIndex > 0)
                {
                    this.minTimeout = 300;
                }

                // separate set HorizontalAxis and switch on axis
                this.InverterStatus.CommonControlWord.SwitchOn = true;
                inverterMessage = new InverterMessage(this.InverterStatus.SystemIndex, (short)InverterParameterId.ControlWord, this.InverterStatus.CommonControlWord.Value);

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
            if (message.IsError)
            {
                this.Logger.LogError($"1:SwitchOnStartState message={message}");
                this.errorProvider.RecordNew(MachineErrorCode.InverterErrorBaseCode);
                this.ParentStateMachine.ChangeState(new SwitchOnErrorState(this.ParentStateMachine, this.axisToSwitchOn, this.InverterStatus, this.Logger));
            }
            else
            {
                this.Logger.LogTrace($"1:message={message}:Is Error={message.IsError}");
            }

            return true;
        }

        public override bool ValidateCommandResponse(InverterMessage message)
        {
            var returnValue = false;

            if (message.IsError)
            {
                this.Logger.LogError($"1:SwitchOnStartState message={message}");
                this.errorProvider.RecordNew(MachineErrorCode.InverterErrorBaseCode);
                this.ParentStateMachine.ChangeState(new SwitchOnErrorState(this.ParentStateMachine, this.axisToSwitchOn, this.InverterStatus, this.Logger));
            }
            else
            {
                this.Logger.LogTrace($"2:message={message}:Parameter Id={message.ParameterId} waitAck {this.waitAck}");

                if (message.ParameterId == InverterParameterId.AxisChanged)
                {
                    if (message.UShortPayload == 0)
                    {
                        if (this.waitAck)
                        {
                            // switch on axis
                            this.InverterStatus.CommonControlWord.SwitchOn = true;
                            var inverterMessage = new InverterMessage(this.InverterStatus.SystemIndex, (short)InverterParameterId.ControlWord, this.InverterStatus.CommonControlWord.Value);

                            this.Logger.LogDebug($"1:inverterMessage={inverterMessage}");

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
                            this.waitAck = false;

                            this.startTime = DateTime.UtcNow;
                        }
                        else
                        {
                            if (DateTime.UtcNow.Subtract(this.startTime).TotalMilliseconds > 800)
                            {
                                // try to restart inverter
                                var inverterMessage = new InverterMessage(this.InverterStatus.SystemIndex, (short)InverterParameterId.AxisChanged, message.UShortPayload, InverterDataset.AxisChangeDatasetWrite);

                                this.Logger.LogDebug($"1:inverterMessage={inverterMessage}");

                                this.ParentStateMachine.EnqueueCommandMessage(inverterMessage);

                                this.isAxisChanged = true;
                                this.waitAck = true;
                            }
                            else
                            {
                                // read again
                                var inverterMessage = new InverterMessage(this.InverterStatus.SystemIndex, InverterParameterId.AxisChanged, InverterDataset.AxisChangeDatasetRead);

                                this.Logger.LogTrace($"1:inverterMessage={inverterMessage}");

                                this.ParentStateMachine.EnqueueCommandMessage(inverterMessage);
                            }
                        }
                    }
                    else
                    {
                        // ack: write response
                        var inverterMessage = new InverterMessage(this.InverterStatus.SystemIndex, (short)InverterParameterId.AxisChanged, message.UShortPayload, InverterDataset.AxisChangeDatasetWrite);

                        this.Logger.LogDebug($"1:inverterMessage={inverterMessage}");

                        this.ParentStateMachine.EnqueueCommandMessage(inverterMessage);

                        this.isAxisChanged = true;
                        this.waitAck = true;

                        //// and read again
                        //inverterMessage = new InverterMessage(this.InverterStatus.SystemIndex, InverterParameterId.AxisChanged, InverterDataset.AxisChangeDatasetRead);

                        //this.Logger.LogDebug($"1:inverterMessage={inverterMessage}");

                        //this.ParentStateMachine.EnqueueCommandMessage(inverterMessage);
                    }
                }
                else if (this.InverterStatus.CommonStatusWord.IsSwitchedOn
                    && this.isAxisChanged
                    && DateTime.UtcNow.Subtract(this.startTime).TotalMilliseconds > this.minTimeout
                    && !this.waitAck
                    )
                {
                    this.ParentStateMachine.ChangeState(new SwitchOnEndState(this.ParentStateMachine, this.axisToSwitchOn, this.InverterStatus, this.Logger));
                    returnValue = true;
                }
                else if (DateTime.UtcNow.Subtract(this.startTime).TotalMilliseconds > 2000)
                {
                    this.Logger.LogError($"2:SwitchOnStartState timeout, inverter {this.InverterStatus.SystemIndex}, waitAck {this.waitAck}");
                    this.errorProvider.RecordNew(MachineErrorCode.InverterCommandTimeout, additionalText: $"Switch On Inverter {this.InverterStatus.SystemIndex}");
                    this.ParentStateMachine.ChangeState(new SwitchOnErrorState(this.ParentStateMachine, this.axisToSwitchOn, this.InverterStatus, this.Logger));
                }
                else if (this.isAxisChanged
                    && this.waitAck)
                {
                    // read again
                    var inverterMessage = new InverterMessage(this.InverterStatus.SystemIndex, InverterParameterId.AxisChanged, InverterDataset.AxisChangeDatasetRead);

                    this.Logger.LogDebug($"3:inverterMessage={inverterMessage}");

                    this.ParentStateMachine.EnqueueCommandMessage(inverterMessage);
                }
            }
            return returnValue;
        }

        #endregion
    }
}
