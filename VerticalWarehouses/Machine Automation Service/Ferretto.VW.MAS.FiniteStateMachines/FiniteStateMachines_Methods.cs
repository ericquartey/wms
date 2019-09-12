using System;
using System.Linq;
using System.Threading;
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
using Ferretto.VW.MAS.FiniteStateMachines.ShutterPositioning;
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
            if (this.currentStateMachine != null)
            {
                this.logger.LogDebug($"2:Deallocation FSM {this.currentStateMachine?.GetType()}");
                this.currentStateMachine = null;
            }

            var powerEnableData = new PowerEnableData(this.eventAggregator,
                this.vertimagConfiguration.GetInstalledIoList().ToList(),
                this.vertimagConfiguration.GetInstalledInverterList().Keys.ToList(),
                data.Enable,
                this.logger,
                this.serviceScopeFactory);

            this.currentStateMachine = new PowerEnableStateMachine(powerEnableData);

            this.logger.LogTrace($"3:Starting FSM {this.currentStateMachine.GetType()}: Enable {data.Enable}");

            try
            {
                this.currentStateMachine.Start();
            }
            catch (Exception ex)
            {
                this.logger.LogDebug($"4:Exception: {ex.Message} during the FSM start");

                this.SendMessage(new FsmExceptionMessageData(ex, string.Empty, 0));
            }
        }

        private void DelayTimerMethod(object state)
        {
            // stop timer
            this.delayTimer.Change(Timeout.Infinite, Timeout.Infinite);

            // send a notification to wake up the state machine waiting for the delay
            var notificationMessage = new NotificationMessage(
                null,
                "Delay Timer Expired",
                MessageActor.FiniteStateMachines,
                MessageActor.FiniteStateMachines,
                MessageType.CheckCondition,
                MessageStatus.OperationExecuting);

            this.logger.LogTrace($"1:Publishing Automation Notification Message {notificationMessage.Type} Destination {notificationMessage.Destination} Status {notificationMessage.Status}");

            this.eventAggregator.GetEvent<NotificationEvent>().Publish(notificationMessage);
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

        private InverterIndex InverterFromBayNumber(int BayNumber)
        {
            InverterIndex inverterIndex;
            switch (BayNumber)
            {
                case 1:
                    inverterIndex = InverterIndex.Slave2;
                    break;

                case 2:
                    inverterIndex = InverterIndex.Slave4;
                    break;

                case 3:
                    inverterIndex = InverterIndex.Slave6;
                    break;

                default:
                    throw new ArgumentException($"Bay number not valid {BayNumber}");
            }
            return inverterIndex;
        }

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
                MessageStatus.OperationEnd);
                this.eventAggregator.GetEvent<NotificationEvent>().Publish(msg);
            }
        }

        private void ProcessDrawerOperation(CommandMessage receivedMessage)
        {
            this.logger.LogTrace($"1:Processing Command {receivedMessage.Type} Source {receivedMessage.Source}");

            if (receivedMessage.Data is IDrawerOperationMessageData data)
            {
                this.logger.LogTrace("2: Starting Drawer Operation FSM");

                this.currentStateMachine = new MoveDrawerStateMachine(
                    this.eventAggregator,
                    this.setupStatusProvider,
                    this.machineSensorsStatus,
                    this.generalInfoDataLayer,
                    this.verticalAxis,
                    this.horizontalAxis,
                    data,
                    this.logger,
                    this.serviceScopeFactory);

                try
                {
                    this.currentStateMachine.Start();
                }
                catch (Exception ex)
                {
                    this.logger.LogError($"Exception: {ex.Message} while starting {this.currentStateMachine.GetType()} state machine");

                    this.SendMessage(new FsmExceptionMessageData(ex, $"Exception: {ex.Message} while starting {this.currentStateMachine.GetType()} state machine", 1, MessageVerbosity.Error));
                }
            }
            else
            {
                this.logger.LogError($"Message data type {receivedMessage.Data.GetType()} is invalid for DrawerOperation message type");

                this.SendMessage(new FsmExceptionMessageData(null, $"Message data type {receivedMessage.Data.GetType()} is invalid for DrawerOperation message type", 2, MessageVerbosity.Error));
            }
        }

        private void ProcessHomingMessage(CommandMessage message)
        {
            this.logger.LogTrace("1:Method Start");

            if (message.Data is IHomingMessageData data)
            {
                //TEMP Check the conditions before start an homing procedure
                //if (!this.IsHomingToExecute(out var condition))
                //{
                //    var notificationData = new HomingMessageData(data.AxisToCalibrate);

                //    var msg = new NotificationMessage(
                //        notificationData,
                //        $"Condition: {condition}",
                //        MessageActor.Any,
                //        MessageActor.FiniteStateMachines,
                //        MessageType.Homing,
                //        MessageStatus.OperationError,
                //        ErrorLevel.Error);
                //    this.eventAggregator.GetEvent<NotificationEvent>().Publish(msg);

                //    return;
                //}

                //TEMP Instantiate the homing states machine
                this.currentStateMachine = new HomingStateMachine(
                    this.machineSensorsStatus,
                    this.eventAggregator,
                    data,
                    this.machineConfigurationProvider.IsOneKMachine(),
                    this.logger,
                    this.serviceScopeFactory);

                this.logger.LogTrace($"2:Starting FSM {this.currentStateMachine.GetType()}");

                try
                {
                    this.currentStateMachine.Start();
                }
                catch (Exception ex)
                {
                    this.logger.LogDebug($"3:Exception: {ex.Message} during the FSM start");

                    this.SendMessage(new FsmExceptionMessageData(ex, string.Empty, 0));
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

        private void ProcessPositioningMessage(IPositioningMessageData data)
        {
            if (data is null)
            {
                return;
            }

            this.logger.LogTrace("1:Method Start");

            data.IsOneKMachine = this.machineConfigurationProvider.IsOneKMachine();

            this.currentStateMachine = new PositioningStateMachine(
                this.machineSensorsStatus,
                this.eventAggregator,
                data,
                this.logger,
                this.serviceScopeFactory);

            this.logger.LogTrace($"2:Starting FSM {this.currentStateMachine.GetType()}");

            try
            {
                this.logger.LogDebug("Starting Positioning FSM");
                this.currentStateMachine.Start();
            }
            catch (Exception ex)
            {
                this.logger.LogDebug($"3:Exception: {ex.Message} during the FSM start");

                this.SendMessage(new FsmExceptionMessageData(ex, string.Empty, 0));
            }
        }

        private void ProcessPowerEnableMessage(CommandMessage message)
        {
            this.logger.LogTrace("1:Method Start");

            if (message.Data is IPowerEnableMessageData data)
            {
                if (
                    (this.machineSensorsStatus.IsMachineInNormalState && !data.Enable) ||
                    (!this.machineSensorsStatus.IsMachineInNormalState && data.Enable)
                    )
                {
                    if (this.currentStateMachine != null)
                    {
                        this.logger.LogDebug($"2:Deallocation FSM {this.currentStateMachine?.GetType()}");
                        this.currentStateMachine = null;
                    }

                    var powerEnableData = new PowerEnableData(this.eventAggregator,
                        this.vertimagConfiguration.GetInstalledIoList().ToList(),
                        this.vertimagConfiguration.GetInstalledInverterList().Keys.ToList(),
                        data.Enable,
                        this.logger,
                        this.serviceScopeFactory);

                    this.currentStateMachine = new PowerEnableStateMachine(powerEnableData);

                    this.logger.LogTrace($"3:Starting FSM {this.currentStateMachine.GetType()}: Enable {data.Enable}");

                    try
                    {
                        this.currentStateMachine.Start();
                    }
                    catch (Exception ex)
                    {
                        this.logger.LogDebug($"4:Exception: {ex.Message} during the FSM start");

                        this.SendMessage(new FsmExceptionMessageData(ex, string.Empty, 0));
                    }
                }
                else
                {
                    this.logger.LogTrace($"5:Machine is already in the requested state: IsNormal {this.machineSensorsStatus.IsMachineInNormalState}: Enable {data.Enable}");
                    var notificationMessage = new NotificationMessage(
                        null,
                        "Power Enable Completed",
                        MessageActor.Any,
                        MessageActor.FiniteStateMachines,
                        MessageType.PowerEnable,
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
                        MessageStatus.OperationExecuting);
                    this.eventAggregator.GetEvent<NotificationEvent>().Publish(msg);
                }
                else if (data.BayNumber > 0)
                {
                    var notificationMessageData = new ShutterPositioningMessageData();
                    var inverterStatus = new AglInverterStatus((byte)this.InverterFromBayNumber(data.BayNumber));
                    int sensorStart = (int)(IOMachineSensors.PowerOnOff + inverterStatus.SystemIndex * inverterStatus.aglInverterInputs.Length);
                    Array.Copy(this.machineSensorsStatus.DisplayedInputs, sensorStart, inverterStatus.aglInverterInputs, 0, inverterStatus.aglInverterInputs.Length);
                    notificationMessageData.ShutterPosition = inverterStatus.CurrentShutterPosition;
                    var msg = new NotificationMessage(
                        notificationMessageData,
                        $"Request Position",
                        MessageActor.Any,
                        MessageActor.FiniteStateMachines,
                        MessageType.ShutterPositioning,
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
                this.currentStateMachine = new ResetSecurityStateMachine(
                    this.eventAggregator,
                    null,
                    this.logger,
                    this.serviceScopeFactory);

                this.logger.LogTrace($"2:Starting FSM {this.currentStateMachine.GetType()}");

                try
                {
                    this.logger.LogDebug("Starting Reset Security FSM");
                    this.currentStateMachine.Start();
                }
                catch (Exception ex)
                {
                    this.logger.LogDebug($"3:Exception: {ex.Message} during the FSM start");

                    this.SendMessage(new FsmExceptionMessageData(ex, string.Empty, 0));
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

        private void ProcessShutterPositioningMessage(CommandMessage message)
        {
            this.logger.LogTrace("1:Method Start");

            if (message.Data is IShutterPositioningMessageData data)
            {
                try
                {
                    this.currentStateMachine = new ShutterPositioningStateMachine(
                        this.eventAggregator,
                        data,
                        this.InverterFromBayNumber(data.BayNumber),
                        this.logger,
                        this.serviceScopeFactory,
                        this.machineSensorsStatus,
                        this.delayTimer);

                    this.logger.LogDebug($"2:Starting FSM {this.currentStateMachine.GetType()}");

                    this.currentStateMachine.Start();
                }
                catch (Exception ex)
                {
                    this.logger.LogDebug($"3:Exception: {ex.Message} during the FSM start");

                    this.SendMessage(new FsmExceptionMessageData(ex, string.Empty, 0));
                }
            }
        }

        private void ProcessStopMessage(CommandMessage receivedMessage)
        {
            this.logger.LogTrace($"1:Processing Command {receivedMessage.Type} Source {receivedMessage.Source}");

            this.currentStateMachine?.Stop();
        }

        #endregion
    }
}
