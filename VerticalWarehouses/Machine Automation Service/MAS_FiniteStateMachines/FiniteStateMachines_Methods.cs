using System;
using Ferretto.VW.Common_Utils.Messages;
using Ferretto.VW.Common_Utils.Messages.Data;
using Ferretto.VW.Common_Utils.Messages.Enumerations;
using Ferretto.VW.Common_Utils.Messages.Interfaces;
using Ferretto.VW.MAS_FiniteStateMachines.Homing;
using Ferretto.VW.MAS_FiniteStateMachines.Positioning;
using Ferretto.VW.MAS_FiniteStateMachines.ShutterControl;
using Ferretto.VW.MAS_FiniteStateMachines.ShutterPositioning;
using Ferretto.VW.MAS_Utils.Enumerations;
using Ferretto.VW.MAS_Utils.Events;
using Ferretto.VW.MAS_Utils.Messages;
using Ferretto.VW.MAS_Utils.Messages.FieldData;
using Microsoft.Extensions.Logging;

namespace Ferretto.VW.MAS_FiniteStateMachines
{
    public partial class FiniteStateMachines
    {
        #region Methods

        private bool CheckConditionToExecuteHoming(out ConditionToCheckType condition)
        {
            condition = ConditionToCheckType.MachineIsInEmergencyState;
            if (this.machineSensorsStatus.MachineIsInEmergencyState)
            {
                return false;
            }

            condition = ConditionToCheckType.DrawerIsPartiallyOnCradle;
            if (this.machineSensorsStatus.DrawerIsPartiallyOnCradle)
            {
                return false;
            }

            //TEMP This condition does not satisfied by the Bender machine
            //condition = ConditionToCheckType.SensorInZeroOnCradle;
            //if (!this.machineSensorsStatus.SensorInZeroOnCradle)
            //{
            //    return false;
            //}

            return true;
        }

        private bool EvaluateCondition(ConditionToCheckType condition)
        {
            var result = false;
            switch (condition)
            {
                case ConditionToCheckType.MachineIsInEmergencyState:
                    result = this.machineSensorsStatus.MachineIsInEmergencyState;
                    break;

                case ConditionToCheckType.DrawerIsCompletelyOnCradle:
                    result = this.machineSensorsStatus.DrawerIsCompletelyOnCradle;
                    break;

                case ConditionToCheckType.DrawerIsPartiallyOnCradle:
                    result = this.machineSensorsStatus.DrawerIsPartiallyOnCradle;
                    break;

                //TEMP Add here other condition getters

                default:
                    break;
            }
            return result;
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
                MessageStatus.OperationEnd,
                ErrorLevel.NoError);
                this.eventAggregator.GetEvent<NotificationEvent>().Publish(msg);
            }
        }

        private void ProcessHomingMessage(CommandMessage message)
        {
            this.logger.LogTrace("1:Method Start");

            if (message.Data is IHomingMessageData data)
            {
                //TEMP Check the conditions before start an homing procedure
                if (!this.CheckConditionToExecuteHoming(out var condition))
                {
                    var notificationData = new HomingMessageData(data.AxisToCalibrate);

                    var msg = new NotificationMessage(
                        notificationData,
                        $"Condition: {condition}",
                        MessageActor.Any,
                        MessageActor.FiniteStateMachines,
                        MessageType.Homing,
                        MessageStatus.OperationError,
                        ErrorLevel.Error);
                    this.eventAggregator.GetEvent<NotificationEvent>().Publish(msg);

                    return;
                }

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

                    this.SendMessage(new FSMExceptionMessageData(ex, "", 0));
                }
            }
        }

        private void ProcessPositioningMessage(CommandMessage message)
        {
            this.logger.LogTrace("1:Method Start");

            if (message.Data is IPositioningMessageData data)
            {
                this.currentStateMachine = new PositioningStateMachine(this.eventAggregator, data, this.logger);

                this.logger.LogTrace($"2:Starting FSM {this.currentStateMachine.GetType()}");

                try
                {
                    this.currentStateMachine.Start();
                }
                catch (Exception ex)
                {
                    this.logger.LogDebug($"3:Exception: {ex.Message} during the FSM start");

                    this.SendMessage(new FSMExceptionMessageData(ex, "", 0));
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
            var IoDataMessage = new SensorsChangedFieldMessageData();
            IoDataMessage.SensorsStatus = true;
            var IoMessage = new FieldCommandMessage(
                IoDataMessage,
                "Update IO digital input",
                FieldMessageActor.IoDriver,
                FieldMessageActor.FiniteStateMachines,
                FieldMessageType.SensorsChanged);

            this.eventAggregator.GetEvent<FieldCommandEvent>().Publish(IoMessage);

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

                    this.SendMessage(new FSMExceptionMessageData(ex, "", 0));
                }
            }
        }

        private void ProcessShutterPositioningMessage(CommandMessage message)
        {
            this.logger.LogTrace("1:Method Start");

            if (message.Data is IShutterPositioningMessageData data)
            {
                this.currentStateMachine = new ShutterPositioningStateMachine(this.eventAggregator, data, this.logger);

                this.logger.LogTrace($"2:Starting FSM {this.currentStateMachine.GetType()}");

                try
                {
                    this.currentStateMachine.Start();
                }
                catch (Exception ex)
                {
                    this.logger.LogDebug($"3:Exception: {ex.Message} during the FSM start");

                    this.SendMessage(new FSMExceptionMessageData(ex, "", 0));
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
