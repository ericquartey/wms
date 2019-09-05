using System;
using System.Linq;
using Ferretto.VW.CommonUtils.Enumerations;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;
using Ferretto.VW.MAS.FiniteStateMachines.Homing;
using Ferretto.VW.MAS.FiniteStateMachines.MoveDrawer;
using Ferretto.VW.MAS.FiniteStateMachines.Positioning;
using Ferretto.VW.MAS.FiniteStateMachines.PowerEnable;
using Ferretto.VW.MAS.FiniteStateMachines.PowerEnable.Models;
using Ferretto.VW.MAS.FiniteStateMachines.ResetSecurity;
using Ferretto.VW.MAS.FiniteStateMachines.ShutterControl;
using Ferretto.VW.MAS.FiniteStateMachines.ShutterPositioning;
using Ferretto.VW.MAS.FiniteStateMachines.ShutterPositioning.Models;
using Ferretto.VW.MAS.InverterDriver.InverterStatus;
using Ferretto.VW.MAS.Utils.Enumerations;
using Ferretto.VW.MAS.Utils.Events;
using Ferretto.VW.MAS.Utils.Messages;
using Ferretto.VW.MAS.Utils.Messages.FieldData;
using Microsoft.Extensions.Logging;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS.FiniteStateMachines

{
    public partial class FiniteStateMachines
    {


        #region Methods

        private void CreatePowerEnableStateMachine(IPowerEnableMessageData data)
        {
            if (this.currentStateMachines.TryGetValue(BayIndex.ElevatorBay, out var currentStateMachine))
            {
                this.logger.LogDebug($"2:Deallocation FSM {currentStateMachine?.GetType()}");
                this.currentStateMachines.Remove(BayIndex.ElevatorBay);
            }

            var powerEnableData = new PowerEnableData(this.eventAggregator,
                this.vertimagConfiguration.GetInstalledIoList().ToList(),
                this.vertimagConfiguration.GetInstalledInverterList().Keys.ToList(),
                data.Enable,
                this.logger,
                this.serviceScopeFactory);

            currentStateMachine = new PowerEnableStateMachine(powerEnableData);
            this.currentStateMachines.Add(BayIndex.ElevatorBay, currentStateMachine);

            this.logger.LogTrace($"3:Starting FSM PowerEnableSTateMachine: Enable {data.Enable}");

            try
            {
                currentStateMachine.Start();
            }
            catch (Exception ex)
            {
                this.logger.LogDebug($"4:Exception: {ex.Message} during the FSM start");

                this.SendNotificationMessage(new FsmExceptionMessageData(ex, string.Empty, 0));
            }
        }

        private bool EvaluateCondition(ConditionToCheckType condition)
        {
            var result = false;
            switch (condition)
            {
                case ConditionToCheckType.MachineIsInEmergencyState:
                    result = this.machineSensorsStatus.IsMachineInEmergencyStateBay1;
                    break;

                case ConditionToCheckType.DrawerIsCompletelyOnCradle:
                    result = this.machineSensorsStatus.IsDrawerCompletelyOnCradle;
                    break;

                case ConditionToCheckType.DrawerIsPartiallyOnCradle:
                    result = this.machineSensorsStatus.IsDrawerPartiallyOnCradleBay1;
                    break;

                    //TEMP Add here other condition getters
            }
            return result;
        }

        /// <summary>
        /// This routine contains all conditions to be satisfied before to do an homing operation.
        /// </summary>
        //TEMP
        //private bool IsHomingToExecute(out ConditionToCheckType condition)
        //{
        //    //TEMP The following conditions must be checked in order to execute an homing operation

        //    condition = ConditionToCheckType.MachineIsInEmergencyState;
        //    if (this.EvaluateCondition(condition))
        //    {
        //        this.logger.LogTrace("1:MachineIsInEmergencyState");
        //        return false;
        //    }

        //    condition = ConditionToCheckType.DrawerIsPartiallyOnCradle;
        //    if (this.EvaluateCondition(condition))
        //    {
        //        this.logger.LogTrace("1:DrawerIsPartiallyOnCradle");
        //        return false;
        //    }

        //    //TEMP This condition does not satisfied by the Bender machine
        //    //condition = ConditionToCheckType.SensorInZeroOnCradle;
        //    //if (!this.EvaluateCondition(condition))
        //    //{
        //    //    return false;
        //    //}

        //    return true;
        //}

        private void ProcessCheckConditionMessage(CommandMessage message)
        {
            this.logger.LogTrace($"1:Processing Command {message.Type} Source {message.Source}");

            if (message.Data is ICheckConditionMessageData data)
            {
                data.Result = this.EvaluateCondition(data.ConditionToCheck);

                //TEMP Send a notification message
                var msg = new NotificationMessage(
                data,
                $"{data.ConditionToCheck} response: {data.Result}",
                MessageActor.Any,
                MessageActor.FiniteStateMachines,
                MessageType.CheckCondition,
                message.BayIndex,
                MessageStatus.OperationEnd);
                this.eventAggregator.GetEvent<NotificationEvent>().Publish(msg);
            }
        }

        private void ProcessDrawerOperation(CommandMessage receivedMessage)
        {
            this.logger.LogTrace($"1:Processing Command {receivedMessage.Type} Source {receivedMessage.Source}");

            if (this.currentStateMachines.TryGetValue(receivedMessage.BayIndex, out var currentStateMachine))
            {
                this.logger.LogDebug($"2:Deallocation FSM {currentStateMachine?.GetType()}");
                this.SendNotificationMessage(new FsmExceptionMessageData(null, $"Error while starting {currentStateMachine?.GetType()} state machine. Operation already in progress on bay {receivedMessage.BayIndex}", 1, MessageVerbosity.Error));
            }
            else
            {
                if (receivedMessage.Data is IDrawerOperationMessageData data)
                {
                    this.logger.LogTrace("2: Starting Drawer Operation FSM");

                    currentStateMachine = new MoveDrawerStateMachine(
                        this.eventAggregator,
                        this.setupStatusProvider,
                        this.machineSensorsStatus,
                        this.generalInfoDataLayer,
                        this.verticalAxis,
                        this.horizontalAxis,
                        data,
                        this.logger,
                        this.serviceScopeFactory);

                    this.currentStateMachines.Add(receivedMessage.BayIndex, currentStateMachine);

                    try
                    {
                        currentStateMachine.Start();
                    }
                    catch (Exception ex)
                    {
                        this.logger.LogError(
                            $"Exception: {ex.Message} while starting {currentStateMachine.GetType()} state machine");

                        this.SendNotificationMessage(new FsmExceptionMessageData(ex,
                            $"Exception: {ex.Message} while starting {currentStateMachine.GetType()} state machine", 1,
                            MessageVerbosity.Error));
                    }
                }
                else
                {
                    this.logger.LogError(
                        $"Message data type {receivedMessage.Data.GetType()} is invalid for DrawerOperation message type");

                    this.SendNotificationMessage(new FsmExceptionMessageData(null,
                        $"Message data type {receivedMessage.Data.GetType()} is invalid for DrawerOperation message type",
                        2, MessageVerbosity.Error));
                }
            }
        }

        private void ProcessHomingMessage(CommandMessage receivedMessage)
        {
            this.logger.LogTrace("1:Method Start");

            if (this.currentStateMachines.TryGetValue(BayIndex.ElevatorBay, out var currentStateMachine))
            {
                this.logger.LogDebug($"2:Deallocation FSM {currentStateMachine?.GetType()}");
                this.SendNotificationMessage(new FsmExceptionMessageData(null, $"Error while starting {currentStateMachine?.GetType()} state machine. Operation already in progress on ElevatorBay", 1, MessageVerbosity.Error));
            }
            else
            {
                if (receivedMessage.Data is IHomingMessageData data)
                {
                    currentStateMachine = new HomingStateMachine(
                        data.AxisToCalibrate,
                        this.machineConfigurationProvider.IsOneKMachine(),
                        receivedMessage.BayIndex,
                        this.eventAggregator,
                        this.logger,
                        this.serviceScopeFactory);

                    this.logger.LogTrace($"2:Starting FSM {currentStateMachine.GetType()}");
                    this.currentStateMachines.Add(BayIndex.ElevatorBay, currentStateMachine);

                    try
                    {
                        currentStateMachine.Start();
                    }
                    catch (Exception ex)
                    {
                        this.logger.LogDebug($"3:Exception: {ex.Message} during the FSM start");

                        this.SendNotificationMessage(new FsmExceptionMessageData(ex, string.Empty, 0));
                    }
                }
            }
        }

        private void ProcessInverterStopMessage()
        {
            this.logger.LogTrace("1:Method Start");

            var inverterMessage = new FieldCommandMessage(
                null,
                "Stop Inverter",
                FieldMessageActor.InverterDriver,
                FieldMessageActor.FiniteStateMachines,
                FieldMessageType.InverterStop,
                (byte)InverterIndex.All);
            this.eventAggregator.GetEvent<FieldCommandEvent>().Publish(inverterMessage);
        }

        private void ProcessPositioningMessage(CommandMessage message)
        {
            if (message.Data is IPositioningMessageData data)
            {
                var targetBay = this.baysProvider.GetByMovementType(data);
                if (targetBay == BayIndex.None)
                {
                    targetBay = message.BayIndex;
                }

                if (this.currentStateMachines.TryGetValue(targetBay, out var currentStateMachine))
                {
                    this.SendNotificationMessage(new FsmExceptionMessageData(null, $"Error while starting {currentStateMachine?.GetType()} state machine. Operation already in progress on ElevatorBay", 1, MessageVerbosity.Error));
                }
                else
                {
                    currentStateMachine = new PositioningStateMachine(
                        message.BayIndex,
                        targetBay,
                        data,
                        this.machineSensorsStatus,
                        this.eventAggregator,
                        this.logger,
                        this.baysProvider,
                        this.serviceScopeFactory);

                    this.logger.LogTrace($"2:Starting FSM {currentStateMachine.GetType()}");
                    this.currentStateMachines.Add(BayIndex.ElevatorBay, currentStateMachine);

                    try
                    {
                        this.logger.LogDebug("Starting Positioning FSM");
                        currentStateMachine.Start();
                    }
                    catch (Exception ex)
                    {
                        this.logger.LogDebug($"3:Exception: {ex.Message} during the FSM start");

                        this.SendNotificationMessage(new FsmExceptionMessageData(ex, string.Empty, 0));
                    }
                }
            }
            else
            {
                this.SendNotificationMessage(new FsmExceptionMessageData(null, $"Error while starting Positioning state machine. Wrong command message payload type", 1, MessageVerbosity.Error));
            }
        }

        private void ProcessPowerEnableMessage(CommandMessage message)
        {
            this.logger.LogTrace("1:Method Start");

            if (message.Data is IPowerEnableMessageData data)
            {
                if (
                    (this.machineSensorsStatus.IsMachineInRunningState && !data.Enable) ||
                    (!this.machineSensorsStatus.IsMachineInRunningState && data.Enable)
                    )
                {
                    if (this.currentStateMachines.TryGetValue(BayIndex.ElevatorBay, out var currentStateMachine))
                    {
                        this.logger.LogDebug($"2:Deallocation FSM {currentStateMachine?.GetType()}");
                        this.currentStateMachines.Remove(BayIndex.ElevatorBay);
                    }

                    var powerEnableData = new PowerEnableData(this.eventAggregator,
                        this.vertimagConfiguration.GetInstalledIoList().ToList(),
                        this.vertimagConfiguration.GetInstalledInverterList().Keys.ToList(),
                        data.Enable,
                        this.logger,
                        this.serviceScopeFactory);

                    currentStateMachine = new PowerEnableStateMachine(powerEnableData);

                    this.logger.LogTrace($"3:Starting FSM {currentStateMachine.GetType()}: Enable {data.Enable}");
                    this.currentStateMachines.Add(BayIndex.ElevatorBay, currentStateMachine);

                    try
                    {
                        currentStateMachine.Start();
                    }
                    catch (Exception ex)
                    {
                        this.logger.LogDebug($"4:Exception: {ex.Message} during the FSM start");

                        this.SendNotificationMessage(new FsmExceptionMessageData(ex, string.Empty, 0));
                    }
                }
                else
                {
                    this.logger.LogTrace($"5:Machine is already in the requested state: IsNormal {this.machineSensorsStatus.IsMachineInRunningState}: Enable {data.Enable}");
                    var notificationMessage = new NotificationMessage(
                        null,
                        "Power Enable Completed",
                        MessageActor.Any,
                        MessageActor.FiniteStateMachines,
                        MessageType.PowerEnable,
                        message.BayIndex,
                        MessageStatus.OperationEnd);

                    this.logger.LogTrace($"6:Publishing Automation Notification Message {notificationMessage.Type} Destination {notificationMessage.Destination} Status {notificationMessage.Status}");

                    this.eventAggregator.GetEvent<NotificationEvent>().Publish(notificationMessage);
                }
            }
        }

        private void ProcessRequestPositionMessage(CommandMessage message)
        {
            this.logger.LogTrace("1:Method Start");

            if (message.Data is IRequestPositionMessageData data)
            {
                if (data.CurrentAxis == Axis.Horizontal || data.CurrentAxis == Axis.Vertical)
                {
                    var msgData = new PositioningMessageData();
                    msgData.CurrentPosition = (data.CurrentAxis == Axis.Horizontal) ? this.machineSensorsStatus.AxisXPosition : this.machineSensorsStatus.AxisYPosition;
                    var msg = new NotificationMessage(
                        msgData,
                        "Request Position",
                        MessageActor.Any,
                        MessageActor.FiniteStateMachines,
                        MessageType.Positioning,
                        message.BayIndex,
                        MessageStatus.OperationExecuting);
                    this.eventAggregator.GetEvent<NotificationEvent>().Publish(msg);
                }
                else if (data.BayNumber > 0)
                {
                    var notificationMessageData = new ShutterPositioningMessageData();
                    var inverterStatus = new AglInverterStatus((byte)this.baysProvider.GetInverterList(message.BayIndex).ToArray()[this.baysProvider.BayInverterPosition]);
                    int sensorStart = (int)(IOMachineSensors.PowerOnOff + inverterStatus.SystemIndex * inverterStatus.aglInverterInputs.Length);
                    Array.Copy(this.machineSensorsStatus.DisplayedInputs, sensorStart, inverterStatus.aglInverterInputs, 0, inverterStatus.aglInverterInputs.Length);
                    notificationMessageData.ShutterPosition = inverterStatus.CurrentShutterPosition;
                    var msg = new NotificationMessage(
                        notificationMessageData,
                        $"Request Position",
                        MessageActor.Any,
                        MessageActor.FiniteStateMachines,
                        MessageType.ShutterPositioning,
                        message.BayIndex,
                        MessageStatus.OperationExecuting);
                    this.eventAggregator.GetEvent<NotificationEvent>().Publish(msg);
                }
            }
        }

        private void ProcessResetSecurityMessage()
        {
            this.logger.LogTrace("1:Method Start");

            //if (message.Data is IResetSecurityMessageData data)
            {
                if (this.currentStateMachines.TryGetValue(BayIndex.ElevatorBay, out var currentStateMachine))
                {
                    this.logger.LogDebug($"2:Deallocation FSM {currentStateMachine?.GetType()}");
                    this.currentStateMachines.Remove(BayIndex.ElevatorBay);
                }

                currentStateMachine = new ResetSecurityStateMachine(
                    this.eventAggregator,
                    null,
                    this.logger,
                    this.serviceScopeFactory);

                this.logger.LogTrace($"2:Starting FSM {currentStateMachine.GetType()}");
                this.currentStateMachines.Add(BayIndex.ElevatorBay, currentStateMachine);

                try
                {
                    this.logger.LogDebug("Starting Reset Security FSM");
                    currentStateMachine.Start();
                }
                catch (Exception ex)
                {
                    this.logger.LogDebug($"3:Exception: {ex.Message} during the FSM start");

                    this.SendNotificationMessage(new FsmExceptionMessageData(ex, string.Empty, 0));
                }
            }
        }

        private void ProcessSensorsChangedMessage()
        {
            this.logger.LogTrace("1:Method Start");

            // Send a field message to force the Update of sensors (input lines) to InverterDriver
            var inverterDataMessage = new InverterSetTimerFieldMessageData(InverterTimer.SensorStatus, true, 0);
            var inverterMessage = new FieldCommandMessage(
                inverterDataMessage,
                "Update Inverter digital input status",
                FieldMessageActor.InverterDriver,
                FieldMessageActor.FiniteStateMachines,
                FieldMessageType.InverterSetTimer,
                (byte)InverterIndex.MainInverter);
            this.eventAggregator.GetEvent<FieldCommandEvent>().Publish(inverterMessage);

            // Send a field message to force the Update of sensors (input lines) to IoDriver
            foreach (var index in this.ioIndexDeviceList)
            {
                var ioDataMessage = new SensorsChangedFieldMessageData();
                ioDataMessage.SensorsStatus = true;
                var ioMessage = new FieldCommandMessage(
                    ioDataMessage,
                    "Update IO digital input",
                    FieldMessageActor.IoDriver,
                    FieldMessageActor.FiniteStateMachines,
                    FieldMessageType.SensorsChanged,
                    (byte)index);

                this.eventAggregator.GetEvent<FieldCommandEvent>().Publish(ioMessage);
            }

            this.forceInverterIoStatusPublish = true;
            this.forceRemoteIoStatusPublish = true;
        }

        private void ProcessShutterControlMessage(CommandMessage message)
        {
            this.logger.LogTrace("1:Method Start");

            if (this.currentStateMachines.TryGetValue(message.BayIndex, out var currentStateMachine))
            {
                this.logger.LogDebug($"2:Deallocation FSM {currentStateMachine?.GetType()}");
                this.SendNotificationMessage(new FsmExceptionMessageData(null,
                    $"Error while starting {currentStateMachine?.GetType()} state machine. Operation already in progress on ElevatorBay",
                    1, MessageVerbosity.Error));
            }
            else
            {
                if (message.Data is IShutterTestStatusChangedMessageData data)
                {
                    // TODO Retrieve the type of given shutter based on the information saved in the DataLayer
                    data.ShutterType = ShutterType.Shutter2Type;

                    currentStateMachine = new ShutterControlStateMachine(
                        this.eventAggregator,
                        data,
                        this.logger,
                        this.serviceScopeFactory);

                    this.logger.LogTrace($"2:Starting FSM {currentStateMachine.GetType()}");
                    this.currentStateMachines.Add(message.BayIndex, currentStateMachine);

                    try
                    {
                        currentStateMachine.Start();
                    }
                    catch (Exception ex)
                    {
                        this.logger.LogDebug($"3:Exception: {ex.Message} during the FSM start");

                        this.SendNotificationMessage(new FsmExceptionMessageData(ex, string.Empty, 0));
                    }
                }
            }
        }

        private void ProcessShutterPositioningMessage(CommandMessage message)
        {
            this.logger.LogTrace("1:Method Start");

            if (this.currentStateMachines.TryGetValue(message.BayIndex, out var currentStateMachine))
            {
                this.logger.LogDebug($"2:Deallocation FSM {currentStateMachine?.GetType()}");
                this.SendNotificationMessage(new FsmExceptionMessageData(null,
                    $"Error while starting {currentStateMachine?.GetType()} state machine. Operation already in progress on ElevatorBay",
                    1, MessageVerbosity.Error));
            }
            else
            {
                if (message.Data is IShutterPositioningMessageData data)
                {

                    var positioningMachineData = new ShutterPositioningStateMachineData(data,
                        message.BayIndex,
                        this.baysProvider.GetInverterList(message.BayIndex)[this.baysProvider.ShutterInverterPosition],
                        this.eventAggregator,
                        this.machineSensorsStatus,
                        this.logger,
                        this.serviceScopeFactory);

                    currentStateMachine = new ShutterPositioningStateMachine(positioningMachineData);

                    this.logger.LogDebug($"2:Starting FSM {currentStateMachine.GetType()}");
                    this.currentStateMachines.Add(message.BayIndex, currentStateMachine);

                    try
                    {
                        currentStateMachine.Start();
                    }
                    catch (Exception ex)
                    {
                        this.logger.LogDebug($"3:Exception: {ex.Message} during the FSM start");

                        this.SendNotificationMessage(new FsmExceptionMessageData(ex, string.Empty, 0));
                    }
                }
            }
        }

        private void ProcessStopMessage(CommandMessage receivedMessage)
        {
            this.logger.LogTrace($"1:Processing Command {receivedMessage.Type} Source {receivedMessage.Source}");

            if (this.currentStateMachines.TryGetValue(receivedMessage.BayIndex, out var currentStateMachine))
            {
                currentStateMachine.Stop(StopRequestReason.Stop);
            }
        }

        #endregion
    }
}
