using System;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;
using Ferretto.VW.MAS.FiniteStateMachines.Homing;
using Ferretto.VW.MAS.FiniteStateMachines.MoveDrawer;
using Ferretto.VW.MAS.FiniteStateMachines.Positioning;
using Ferretto.VW.MAS.FiniteStateMachines.ShutterControl;
using Ferretto.VW.MAS.FiniteStateMachines.ShutterPositioning;
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

        private bool EvaluateCondition(ConditionToCheckType condition)
        {
            var result = false;
            switch (condition)
            {
                case ConditionToCheckType.MachineIsInEmergencyState:
                    result = this.machineSensorsStatus.IsMachineInEmergencyStateBay1;
                    break;

                case ConditionToCheckType.DrawerIsCompletelyOnCradle:
                    result = this.machineSensorsStatus.IsDrawerCompletelyOnCradleBay1;
                    break;

                case ConditionToCheckType.DrawerIsPartiallyOnCradle:
                    result = this.machineSensorsStatus.IsDrawerPartiallyOnCradleBay1;
                    break;

                //TEMP Add here other condition getters
                default:
                    break;
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
                MessageStatus.OperationEnd,
                ErrorLevel.NoError);
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
                    this.setupStatus,
                    this.machineSensorsStatus,
                    this.generalInfoDataLayer,
                    this.verticalAxis,
                    this.horizontalAxis,
                    data,
                    this.logger);

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
                this.currentStateMachine = new HomingStateMachine(this.eventAggregator, data, this.logger);

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

        private void ProcessPositioningMessage(CommandMessage message)
        {
            this.logger.LogTrace("1:Method Start");

            if (message.Data is IPositioningMessageData data)
            {
                this.currentStateMachine = new PositioningStateMachine(this.eventAggregator, data, this.logger, this.machineSensorsStatus);

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
        }

        private void ProcessSensorsChangedMessage()
        {
            this.logger.LogTrace("1:Method Start");

            // Send a field message to force the Update of sensors (input lines) to InverterDriver
            var inverterDataMessage = new InverterStatusUpdateFieldMessageData(true, 0, false, 0);
            var inverterMessage = new FieldCommandMessage(
                inverterDataMessage,
                "Update Inverter digital input status",
                FieldMessageActor.InverterDriver,
                FieldMessageActor.FiniteStateMachines,
                FieldMessageType.InverterStatusUpdate);
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

            if (message.Data is IShutterControlMessageData data)
            {
                // TODO Retrieve the type of given shutter based on the information saved in the DataLayer
                data.ShutterType = ShutterType.Shutter2Type;

                this.currentStateMachine = new ShutterControlStateMachine(this.eventAggregator, data, this.logger);

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

        private void ProcessShutterPositioningMessage(CommandMessage message)
        {
            this.logger.LogTrace("1:Method Start");

            if (message.Data is IShutterPositioningMessageData data)
            {
                this.currentStateMachine = new ShutterPositioningStateMachine(this.eventAggregator, data, this.logger, this.machineSensorsStatus);

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

        private void ProcessStopMessage(CommandMessage receivedMessage)
        {
            this.logger.LogTrace($"1:Processing Command {receivedMessage.Type} Source {receivedMessage.Source}");

            this.currentStateMachine?.Stop();
        }

        #endregion
    }
}
