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
using Ferretto.VW.MAS.FiniteStateMachines.ResetFault;
using Ferretto.VW.MAS.FiniteStateMachines.ResetSecurity;
using Ferretto.VW.MAS.FiniteStateMachines.ShutterPositioning;
using Ferretto.VW.MAS.InverterDriver.Contracts;
using Ferretto.VW.MAS.InverterDriver.InverterStatus;
using Ferretto.VW.MAS.Utils.Enumerations;
using Ferretto.VW.MAS.Utils.Events;
using Ferretto.VW.MAS.Utils.Messages;
using Ferretto.VW.MAS.Utils.Messages.FieldData;
using Microsoft.Extensions.Logging;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS.FiniteStateMachines
{
    internal partial class FiniteStateMachines
    {
        #region Methods

        private void CreatePowerEnableStateMachine(IPowerEnableMessageData data)
        {
            if (this.currentStateMachines.TryGetValue(BayNumber.BayOne, out var currentStateMachine))
            {
                this.logger.LogTrace($"2:Deallocation FSM {currentStateMachine?.GetType()}");
                this.currentStateMachines.Remove(BayNumber.BayOne);
            }

            currentStateMachine = new PowerEnableStateMachine(null,
                this.machineSensorsStatus,
                this.baysProvider,
                this.eventAggregator,
                this.logger,
                this.serviceScopeFactory);
            this.currentStateMachines.Add(BayNumber.BayOne, currentStateMachine);

            this.logger.LogTrace($"3:Starting FSM PowerEnableSTateMachine: Enable {data.Enable}");

            try
            {
                currentStateMachine.Start();
            }
            catch (Exception ex)
            {
                this.logger.LogError($"4:Exception: {ex.Message} during the FSM {currentStateMachine.GetType()} start");

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
                message.RequestingBay,
                message.RequestingBay,
                MessageStatus.OperationEnd);
                this.eventAggregator.GetEvent<NotificationEvent>().Publish(msg);
            }
        }

        private void ProcessDrawerOperation(CommandMessage receivedMessage)
        {
            this.logger.LogTrace($"1:Processing Command {receivedMessage.Type} Source {receivedMessage.Source}");

            if (this.currentStateMachines.TryGetValue(receivedMessage.RequestingBay, out var currentStateMachine))
            {
                this.logger.LogDebug($"2:Deallocation FSM {currentStateMachine?.GetType()}");
                this.SendNotificationMessage(new FsmExceptionMessageData(null, $"Error while starting {currentStateMachine?.GetType()} state machine. Operation already in progress on bay {receivedMessage.RequestingBay}", 1, MessageVerbosity.Error));
            }
            else
            {
                if (receivedMessage.Data is IDrawerOperationMessageData data)
                {
                    this.logger.LogTrace("2: Starting Drawer Operation FSM");

                    currentStateMachine = new MoveDrawerStateMachine(
                        this.machineProvider.IsOneTonMachine(),
                        receivedMessage.RequestingBay,
                        this.setupStatusProvider,
                        this.machineSensorsStatus,
                        data,
                        this.eventAggregator,
                        this.logger,
                        this.serviceScopeFactory);

                    this.currentStateMachines.Add(receivedMessage.RequestingBay, currentStateMachine);

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

            if (this.currentStateMachines.TryGetValue(BayNumber.ElevatorBay, out var currentStateMachine))
            {
                this.logger.LogTrace($"2:Deallocation FSM {currentStateMachine?.GetType()}");
                this.SendNotificationMessage(new FsmExceptionMessageData(null, $"Error while starting {currentStateMachine?.GetType()} state machine. Operation already in progress on ElevatorBay", 1, MessageVerbosity.Error));
            }
            else
            {
                if (receivedMessage.Data is IHomingMessageData data)
                {
                    receivedMessage.TargetBay = BayNumber.ElevatorBay;
                    currentStateMachine = new HomingStateMachine(
                        data.AxisToCalibrate,
                        this.machineProvider.IsOneTonMachine(),
                        receivedMessage.RequestingBay,
                        receivedMessage.TargetBay,
                        this.machineSensorsStatus,
                        this.eventAggregator,
                        this.logger,
                        this.serviceScopeFactory);

                    this.logger.LogTrace($"2:Starting FSM {currentStateMachine.GetType()}");
                    this.currentStateMachines.Add(BayNumber.ElevatorBay, currentStateMachine);

                    try
                    {
                        currentStateMachine.Start();
                    }
                    catch (Exception ex)
                    {
                        this.logger.LogError($"3:Exception: {ex.Message} during the FSM {currentStateMachine.GetType()} start");

                        this.SendNotificationMessage(new FsmExceptionMessageData(ex, string.Empty, 0));
                    }
                }
            }
        }

        private void ProcessInverterFaultResetMessage(CommandMessage receivedMessage)
        {
            this.logger.LogTrace("1:Method Start");

            if (this.currentStateMachines.TryGetValue(receivedMessage.TargetBay, out var currentStateMachine))
            {
                this.logger.LogTrace($"2:Deallocation FSM {currentStateMachine?.GetType()}");
                this.SendNotificationMessage(new FsmExceptionMessageData(null,
                    $"Error while starting {currentStateMachine?.GetType()} state machine. Operation already in progress on {receivedMessage.TargetBay}",
                    1, MessageVerbosity.Error));
            }
            else
            {
                var inverterList = this.digitalDevicesDataProvider.GetAllInvertersByBay(receivedMessage.TargetBay);

                currentStateMachine = new ResetFaultStateMachine(
                    receivedMessage,
                    inverterList,
                    this.eventAggregator,
                    this.logger,
                    this.serviceScopeFactory);

                this.logger.LogTrace($"2:Starting FSM {currentStateMachine.GetType()}");
                this.currentStateMachines.Add(receivedMessage.TargetBay, currentStateMachine);

                try
                {
                    currentStateMachine.Start();
                }
                catch (Exception ex)
                {
                    this.logger.LogError($"3:Exception: {ex.Message} during the FSM {currentStateMachine.GetType()} start");

                    this.SendNotificationMessage(new FsmExceptionMessageData(ex, string.Empty, 0));
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
                if (targetBay == BayNumber.None)
                {
                    targetBay = message.RequestingBay;
                }

                if (this.currentStateMachines.TryGetValue(targetBay, out var currentStateMachine))
                {
                    this.SendNotificationMessage(new FsmExceptionMessageData(null, $"Error while starting {currentStateMachine?.GetType()} state machine. Operation already in progress on ElevatorBay", 1, MessageVerbosity.Error));
                }
                else
                {
                    data.IsOneKMachine = this.machineProvider.IsOneTonMachine();
                    data.IsStartedOnBoard = this.machineSensorsStatus.IsDrawerCompletelyOnCradle;

                    currentStateMachine = new PositioningStateMachine(
                        message.RequestingBay,
                        targetBay,
                        data,
                        this.machineSensorsStatus,
                        this.eventAggregator,
                        this.logger,
                        this.baysProvider,
                        this.serviceScopeFactory);

                    this.logger.LogTrace($"2:Starting FSM {currentStateMachine.GetType()}");
                    this.currentStateMachines.Add(BayNumber.ElevatorBay, currentStateMachine);

                    try
                    {
                        currentStateMachine.Start();
                    }
                    catch (Exception ex)
                    {
                        this.logger.LogError($"3:Exception: {ex.Message} during the FSM {currentStateMachine.GetType()} start");

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
                //TODO verify pre conditions (is this actually an error ?)
                if (this.currentStateMachines.TryGetValue(BayNumber.BayOne, out var currentStateMachine))
                {
                    this.logger.LogTrace($"1:Attempt to Power Off a running State Machine {currentStateMachine.GetType()}");
                    var notificationMessage = new NotificationMessage(
                        null,
                        "Power Enable Info",
                        MessageActor.Any,
                        MessageActor.FiniteStateMachines,
                        MessageType.PowerEnable,
                        message.RequestingBay,
                        BayNumber.BayOne,
                        MessageStatus.OperationError,
                        ErrorLevel.Info);

                    this.eventAggregator.GetEvent<NotificationEvent>().Publish(notificationMessage);

                    return;
                }

                if (this.machineSensorsStatus.IsMachineInRunningState && !data.Enable ||
                    !this.machineSensorsStatus.IsMachineInRunningState && data.Enable)
                {
                    message.TargetBay = BayNumber.BayOne;
                    currentStateMachine = new PowerEnableStateMachine(
                        message,
                        this.machineSensorsStatus,
                        this.baysProvider,
                        this.eventAggregator,
                        this.logger,
                        this.serviceScopeFactory);

                    this.currentStateMachines.Add(BayNumber.BayOne, currentStateMachine);

                    try
                    {
                        currentStateMachine.Start();
                    }
                    catch (Exception ex)
                    {
                        this.logger.LogError($"4:Exception: {ex.Message} during the FSM {currentStateMachine.GetType()} start");

                        this.SendNotificationMessage(new FsmExceptionMessageData(ex, string.Empty, 0));
                    }
                }
                else
                {
                    this.logger.LogDebug($"Machine is already in the requested state [Actual State: {this.machineSensorsStatus.IsMachineInRunningState}] [Requested State: {data.Enable}]");
                    var notificationMessage = new NotificationMessage(
                        null,
                        "Power Enable Completed",
                        MessageActor.Any,
                        MessageActor.FiniteStateMachines,
                        MessageType.PowerEnable,
                        message.RequestingBay,
                        BayNumber.BayOne,
                        MessageStatus.OperationEnd);

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
                    msgData.CurrentPosition = (data.CurrentAxis == Axis.Horizontal)
                        ? this.machineSensorsStatus.AxisXPosition
                        : this.machineSensorsStatus.AxisYPosition;

                    var msg = new NotificationMessage(
                        msgData,
                        "Request Position",
                        MessageActor.Any,
                        MessageActor.FiniteStateMachines,
                        MessageType.Positioning,
                        message.RequestingBay,
                        message.RequestingBay,
                        MessageStatus.OperationExecuting);
                    this.eventAggregator.GetEvent<NotificationEvent>().Publish(msg);
                }
                else if (message.RequestingBay > 0)
                {
                    var notificationMessageData = new ShutterPositioningMessageData();
                    var inverterStatus = new AglInverterStatus(
                        this.baysProvider.GetByNumber(message.RequestingBay).Shutter.Inverter.Index);

                    var sensorStart = (int)(IOMachineSensors.PowerOnOff + inverterStatus.SystemIndex * inverterStatus.Inputs.Length);
                    Array.Copy(this.machineSensorsStatus.DisplayedInputs, sensorStart, inverterStatus.Inputs, 0, inverterStatus.Inputs.Length);
                    notificationMessageData.ShutterPosition = inverterStatus.CurrentShutterPosition;
                    var msg = new NotificationMessage(
                        notificationMessageData,
                        $"Request Position",
                        MessageActor.Any,
                        MessageActor.FiniteStateMachines,
                        MessageType.ShutterPositioning,
                        message.RequestingBay,
                        message.RequestingBay,
                        MessageStatus.OperationExecuting);
                    this.eventAggregator.GetEvent<NotificationEvent>().Publish(msg);
                }
            }
        }

        private void ProcessResetSecurityMessage(CommandMessage message)
        {
            this.logger.LogTrace("1:Method Start");

            if (this.currentStateMachines.TryGetValue(BayNumber.BayOne, out var currentStateMachine))
            {
                this.logger.LogTrace($"1:Attempt to Power Off a running State Machine {currentStateMachine.GetType()}");
                var notificationMessage = new NotificationMessage(
                    null,
                    "Power Enable Critical error",
                    MessageActor.Any,
                    MessageActor.FiniteStateMachines,
                    MessageType.PowerEnable,
                    message.RequestingBay,
                    BayNumber.BayOne,
                    MessageStatus.OperationError,
                    ErrorLevel.Critical);

                this.eventAggregator.GetEvent<NotificationEvent>().Publish(notificationMessage);

                return;
            }

            currentStateMachine = new ResetSecurityStateMachine(
                    message.RequestingBay,
                    BayNumber.BayOne,
                    this.eventAggregator,
                    this.logger,
                    this.serviceScopeFactory);

            this.logger.LogTrace($"2:Starting FSM {currentStateMachine.GetType()}");
            this.currentStateMachines.Add(BayNumber.BayOne, currentStateMachine);

            try
            {
                currentStateMachine.Start();
            }
            catch (Exception ex)
            {
                this.logger.LogError($"3:Exception: {ex.Message} during the FSM {currentStateMachine.GetType()} start");

                this.SendNotificationMessage(new FsmExceptionMessageData(ex, string.Empty, 0));
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
            foreach (var ioDevice in this.ioDevices)
            {
                var ioDataMessage = new SensorsChangedFieldMessageData();
                ioDataMessage.SensorsStatus = true;
                var ioMessage = new FieldCommandMessage(
                    ioDataMessage,
                    "Update IO digital input",
                    FieldMessageActor.IoDriver,
                    FieldMessageActor.FiniteStateMachines,
                    FieldMessageType.SensorsChanged,
                    (byte)ioDevice.Index);

                this.eventAggregator.GetEvent<FieldCommandEvent>().Publish(ioMessage);
            }

            this.forceInverterIoStatusPublish = true;
            this.forceRemoteIoStatusPublish = true;
        }

        private void ProcessShutterPositioningMessage(CommandMessage message)
        {
            this.logger.LogTrace("1:Method Start");

            if (this.currentStateMachines.TryGetValue(message.RequestingBay, out var currentStateMachine))
            {
                this.logger.LogTrace($"2:Deallocation FSM {currentStateMachine?.GetType()}");
                this.SendNotificationMessage(new FsmExceptionMessageData(null,
                    $"Error while starting {currentStateMachine?.GetType()} state machine. Operation already in progress on ElevatorBay",
                    1, MessageVerbosity.Error));
            }
            else
            {
                if (message.Data is IShutterPositioningMessageData data)
                {
                    message.TargetBay = message.RequestingBay;
                    currentStateMachine = new ShutterPositioningStateMachine(data,
                        message.RequestingBay,
                        message.TargetBay,
                        this.baysProvider.GetByNumber(message.RequestingBay).Shutter.Inverter.Index,
                        this.machineSensorsStatus,
                        this.eventAggregator,
                        this.logger,
                        this.serviceScopeFactory);

                    this.logger.LogTrace($"2:Starting FSM {currentStateMachine.GetType()}");
                    this.currentStateMachines.Add(message.TargetBay, currentStateMachine);

                    try
                    {
                        currentStateMachine.Start();
                    }
                    catch (Exception ex)
                    {
                        this.logger.LogError($"3:Exception: {ex.Message} during the FSM {currentStateMachine.GetType()} start");

                        this.SendNotificationMessage(new FsmExceptionMessageData(ex, string.Empty, 0));
                    }
                }
            }
        }

        private void ProcessStopMessage(CommandMessage receivedMessage)
        {
            this.logger.LogTrace($"1:Processing Command {receivedMessage.Type} Source {receivedMessage.Source}");
            if (receivedMessage.TargetBay == BayNumber.None)
            {
                if (this.currentStateMachines.Any(x => x.Key == receivedMessage.RequestingBay))
                {
                    receivedMessage.TargetBay = receivedMessage.RequestingBay;
                }
                else
                {
                    // message received from the UI: let's stop the first active FSM
                    receivedMessage.TargetBay = this.currentStateMachines.Keys.FirstOrDefault();
                }
            }

            if (this.currentStateMachines.TryGetValue(receivedMessage.TargetBay, out var currentStateMachine))
            {
                if (receivedMessage.Data is IStopMessageData data)
                {
                    currentStateMachine.Stop(data.StopReason);
                }
                else
                {
                    currentStateMachine.Stop(StopRequestReason.Stop);
                }
            }
            else
            {
                var errorNotification = new NotificationMessage(
                    receivedMessage.Data,
                    $"Bay {receivedMessage.TargetBay} is already stopped",
                    MessageActor.Any,
                    MessageActor.FiniteStateMachines,
                    receivedMessage.Type,
                    receivedMessage.RequestingBay,
                    receivedMessage.TargetBay,
                    MessageStatus.OperationEnd);

                this.logger.LogTrace($"3:Type={errorNotification.Type}:Destination={errorNotification.Destination}:Status={errorNotification.Status}");

                this.eventAggregator?.GetEvent<NotificationEvent>().Publish(errorNotification);
            }
        }

        #endregion
    }
}
