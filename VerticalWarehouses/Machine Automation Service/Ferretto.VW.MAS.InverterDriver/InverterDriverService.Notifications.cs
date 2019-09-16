using System;
using System.Threading;
using System.Threading.Tasks;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.InverterDriver.Contracts;
using Ferretto.VW.MAS.InverterDriver.StateMachines.CalibrateAxis;
using Ferretto.VW.MAS.InverterDriver.StateMachines.Positioning;
using Ferretto.VW.MAS.InverterDriver.StateMachines.PowerOff;
using Ferretto.VW.MAS.InverterDriver.StateMachines.PowerOn;
using Ferretto.VW.MAS.InverterDriver.StateMachines.ResetFault;
using Ferretto.VW.MAS.InverterDriver.StateMachines.ShutterPositioning;
using Ferretto.VW.MAS.InverterDriver.StateMachines.Stop;
using Ferretto.VW.MAS.InverterDriver.StateMachines.SwitchOff;
using Ferretto.VW.MAS.InverterDriver.StateMachines.SwitchOn;
using Ferretto.VW.MAS.Utils.Enumerations;
using Ferretto.VW.MAS.Utils.Events;
using Ferretto.VW.MAS.Utils.Messages;
using Ferretto.VW.MAS.Utils.Messages.FieldData;
using Microsoft.Extensions.Logging;

namespace Ferretto.VW.MAS.InverterDriver
{
    partial class InverterDriverService
    {
        #region Methods

        private async Task OnFieldNotificationReceived(FieldNotificationMessage receivedMessage)
        {
            var messageDeviceIndex = Enum.Parse<InverterIndex>(receivedMessage.DeviceIndex.ToString());
            this.currentStateMachines.TryGetValue(messageDeviceIndex, out var messageCurrentStateMachine);

            switch (receivedMessage.Type)
            {
                case FieldMessageType.DataLayerReady:

                    await this.StartHardwareCommunications();
                    this.InitializeInverterStatus();

                    break;

                case FieldMessageType.Positioning:
                    {
                        if (receivedMessage.Status == MessageStatus.OperationEnd ||
                            receivedMessage.Status == MessageStatus.OperationError)
                        {
                            this.logger.LogDebug($"Positioning Deallocating {messageCurrentStateMachine?.GetType()} state machine");
                            this.logger.LogTrace($"4:Deallocation SM {messageCurrentStateMachine?.GetType()}");

                            if (messageCurrentStateMachine is PositioningStateMachine)
                            {
                                this.currentStateMachines.Remove(messageDeviceIndex);
                            }
                            else if (messageCurrentStateMachine is PositioningTableStateMachine)
                            {
                                this.currentStateMachines.Remove(messageDeviceIndex);
                            }
                            else
                            {
                                this.logger.LogDebug($"Try to deallocate {messageCurrentStateMachine?.GetType()} Handling {receivedMessage.Type}");
                            }

                            this.logger.LogTrace("4: Stop the timer for update shaft position");
                            this.axisPositionUpdateTimer[(int)messageDeviceIndex].Change(Timeout.Infinite, Timeout.Infinite);
                        }

                        if (receivedMessage.Status == MessageStatus.OperationStop)
                        {
                            this.logger.LogTrace($"5:Deallocation SM {messageCurrentStateMachine?.GetType()}");

                            if (messageCurrentStateMachine is PositioningStateMachine)
                            {
                                this.currentStateMachines.Remove(messageDeviceIndex);
                            }
                            else if (messageCurrentStateMachine is PositioningTableStateMachine)
                            {
                                this.currentStateMachines.Remove(messageDeviceIndex);
                            }
                            else
                            {
                                this.logger.LogDebug($"Try to deallocate {messageCurrentStateMachine?.GetType()} Handling {receivedMessage.Type}");
                            }

                            this.logger.LogTrace("4: Stop the timer for update shaft position");
                            this.axisPositionUpdateTimer[(int)messageDeviceIndex].Change(Timeout.Infinite, Timeout.Infinite);

                            // Enqueue a message to execute the Stop states machine
                            var stopMessage = new FieldCommandMessage(
                                null,
                                "Stop inverter",
                                FieldMessageActor.InverterDriver,
                                FieldMessageActor.InverterDriver,
                                FieldMessageType.InverterStop,
                                receivedMessage.DeviceIndex);
                            this.commandQueue.Enqueue(stopMessage);
                        }

                        break;
                    }
                case FieldMessageType.CalibrateAxis:

                    if (receivedMessage.Status == MessageStatus.OperationEnd ||
                        receivedMessage.Status == MessageStatus.OperationError)
                    {
                        this.logger.LogDebug($"CalibrateAxis Deallocating {messageCurrentStateMachine?.GetType()} state machine");

                        if (messageCurrentStateMachine is CalibrateAxisStateMachine)
                        {
                            this.currentStateMachines.Remove(messageDeviceIndex);
                        }
                        else
                        {
                            this.logger.LogDebug($"Try to deallocate {messageCurrentStateMachine?.GetType()} Handling {receivedMessage.Type}");
                        }
                    }
                    if (receivedMessage.Status == MessageStatus.OperationStop)
                    {
                        if (messageCurrentStateMachine is CalibrateAxisStateMachine)
                        {
                            this.currentStateMachines.Remove(messageDeviceIndex);
                        }
                        else
                        {
                            this.logger.LogDebug($"Try to deallocate {messageCurrentStateMachine?.GetType()} Handling {receivedMessage.Type}");
                        }

                        // Enqueue a message to execute the Stop states machine
                        var stopMessage = new FieldCommandMessage(
                            null,
                            "Stop inverter",
                            FieldMessageActor.InverterDriver,
                            FieldMessageActor.InverterDriver,
                            FieldMessageType.InverterStop,
                            receivedMessage.DeviceIndex);
                        this.commandQueue.Enqueue(stopMessage);
                    }

                    break;

                case FieldMessageType.ShutterPositioning:

                    this.logger.LogDebug($"ShutterPositioning Deallocating {messageCurrentStateMachine?.GetType()} state machine");
                    if (receivedMessage.Status == MessageStatus.OperationEnd ||
                        receivedMessage.Status == MessageStatus.OperationError)
                    {
                        if (messageCurrentStateMachine is ShutterPositioningStateMachine)
                        {
                            this.currentStateMachines.Remove(messageDeviceIndex);
                        }
                        else
                        {
                            this.logger.LogDebug($"Try to deallocate {messageCurrentStateMachine?.GetType()} Handling {receivedMessage.Type}");
                        }
                    }
                    if (receivedMessage.Status == MessageStatus.OperationStop)
                    {
                        if (messageCurrentStateMachine is ShutterPositioningStateMachine)
                        {
                            this.currentStateMachines.Remove(messageDeviceIndex);
                        }
                        else
                        {
                            this.logger.LogDebug($"Try to deallocate {messageCurrentStateMachine?.GetType()} Handling {receivedMessage.Type}");
                        }

                        // Enqueue a message to execute the Stop states machine
                        var stopMessage = new FieldCommandMessage(
                            null,
                            "Stop inverter",
                            FieldMessageActor.InverterDriver,
                            FieldMessageActor.InverterDriver,
                            FieldMessageType.InverterStop,
                            receivedMessage.DeviceIndex);
                        this.commandQueue.Enqueue(stopMessage);
                    }

                    break;

                case FieldMessageType.InverterSwitchOn:
                case FieldMessageType.InverterStop:

                    this.logger.LogDebug($"Deallocating {messageCurrentStateMachine?.GetType()} state machine ({receivedMessage.Type})");
                    if (receivedMessage.Status == MessageStatus.OperationEnd ||
                        receivedMessage.Status == MessageStatus.OperationError)
                    {
                        if (messageCurrentStateMachine is null)
                        {
                            this.logger.LogDebug($"State machine is null !!");
                        }

                        if (messageCurrentStateMachine is SwitchOnStateMachine ||
                            messageCurrentStateMachine is StopStateMachine)
                        {
                            this.currentStateMachines.Remove(messageDeviceIndex);
                        }
                        else
                        {
                            this.logger.LogDebug($"Try to deallocate {messageCurrentStateMachine?.GetType()} Handling {receivedMessage.Type}");
                        }
                    }

                    break;

                case FieldMessageType.InverterSwitchOff:
                    if (receivedMessage.Status == MessageStatus.OperationEnd ||
                        receivedMessage.Status == MessageStatus.OperationError)
                    {
                        this.logger.LogDebug($"InverterSwitchOff Deallocating {messageCurrentStateMachine?.GetType()} state machine");

                        if (messageCurrentStateMachine is SwitchOffStateMachine)
                        {
                            this.currentStateMachines.Remove(messageDeviceIndex);
                        }
                        else
                        {
                            this.logger.LogDebug($"Try to deallocate {messageCurrentStateMachine?.GetType()} Handling {receivedMessage.Type}");
                        }

                        var nextMessage = ((InverterSwitchOffFieldMessageData)receivedMessage.Data).NextCommandMessage;
                        if (nextMessage != null)
                        {
                            this.commandQueue.Enqueue(nextMessage);
                        }
                    }

                    break;

                case FieldMessageType.InverterPowerOn:

                    if (receivedMessage.Status == MessageStatus.OperationEnd ||
                        receivedMessage.Status == MessageStatus.OperationError)
                    {
                        this.logger.LogDebug($"Deallocating {messageCurrentStateMachine?.GetType()} state machine");

                        if (messageCurrentStateMachine is PowerOnStateMachine)
                        {
                            this.currentStateMachines.Remove(messageDeviceIndex);
                        }
                        else
                        {
                            this.logger.LogDebug($"Try to deallocate {messageCurrentStateMachine?.GetType()} Handling {receivedMessage.Type}");
                        }

                        var nextMessage = ((InverterPowerOnFieldMessageData)receivedMessage.Data).NextCommandMessage;
                        if (nextMessage != null)
                        {
                            this.commandQueue.Enqueue(nextMessage);
                        }
                    }

                    break;

                case FieldMessageType.InverterPowerOff:

                    if (receivedMessage.Status == MessageStatus.OperationEnd ||
                        receivedMessage.Status == MessageStatus.OperationError)
                    {
                        this.logger.LogDebug($"Deallocating {messageCurrentStateMachine?.GetType()} state machine");

                        if (messageCurrentStateMachine is PowerOffStateMachine)
                        {
                            this.currentStateMachines.Remove(messageDeviceIndex);
                        }
                        else
                        {
                            this.logger.LogDebug($"Try to deallocate {messageCurrentStateMachine?.GetType()} Handling {receivedMessage.Type}");
                        }

                        var nextMessage = ((InverterPowerOffFieldMessageData)receivedMessage.Data).NextCommandMessage;
                        if (nextMessage != null)
                        {
                            this.commandQueue.Enqueue(nextMessage);
                        }
                    }

                    break;

                case FieldMessageType.InverterFaultReset:

                    if (receivedMessage.Status == MessageStatus.OperationEnd ||
                        receivedMessage.Status == MessageStatus.OperationError)
                    {
                        this.logger.LogDebug($"InverterFaultReset Deallocating {messageCurrentStateMachine?.GetType()} state machine");

                        if (messageCurrentStateMachine is ResetFaultStateMachine)
                        {
                            this.currentStateMachines.Remove(messageDeviceIndex);
                        }
                        else
                        {
                            this.logger.LogDebug($"Try to deallocate {messageCurrentStateMachine?.GetType()} Handling {receivedMessage.Type}");
                        }
                    }

                    break;
            }

            if (receivedMessage.Source == FieldMessageActor.InverterDriver)
            {
                if (receivedMessage.Status == MessageStatus.OperationEnd ||
                    receivedMessage.Status == MessageStatus.OperationStop)
                {
                    var notificationMessageToFsm = receivedMessage;
                    //TEMP Set the destination of message to FSM
                    notificationMessageToFsm.Destination = FieldMessageActor.FiniteStateMachines;

                    this.eventAggregator?.GetEvent<FieldNotificationEvent>().Publish(notificationMessageToFsm);
                }
            }
        }

        #endregion
    }
}
