using System;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS_DataLayer.Enumerations;
using Ferretto.VW.MAS_InverterDriver.Enumerations;
using Ferretto.VW.MAS_InverterDriver.InverterStatus;
using Ferretto.VW.MAS_InverterDriver.InverterStatus.Interfaces;
using Ferretto.VW.MAS_InverterDriver.StateMachines.CalibrateAxis;
using Ferretto.VW.MAS_InverterDriver.StateMachines.Positioning;
using Ferretto.VW.MAS_InverterDriver.StateMachines.PowerOff;
using Ferretto.VW.MAS_InverterDriver.StateMachines.PowerOn;
using Ferretto.VW.MAS_InverterDriver.StateMachines.ShutterPositioning;
using Ferretto.VW.MAS_InverterDriver.StateMachines.Stop;
using Ferretto.VW.MAS_InverterDriver.StateMachines.SwitchOff;
using Ferretto.VW.MAS_InverterDriver.StateMachines.SwitchOn;
using Ferretto.VW.MAS_Utils.Enumerations;
using Ferretto.VW.MAS_Utils.Events;
using Ferretto.VW.MAS_Utils.Exceptions;
using Ferretto.VW.MAS_Utils.Messages;
using Ferretto.VW.MAS_Utils.Messages.FieldData;
using Ferretto.VW.MAS_Utils.Messages.FieldInterfaces;
using Microsoft.Extensions.Logging;
using Prism.Events;

// ReSharper disable ArrangeThisQualifier
// ReSharper disable ParameterHidesMember
namespace Ferretto.VW.MAS_InverterDriver
{
    public partial class HostedInverterDriver
    {
        #region Methods

        private void ConfigureUpdates(IInverterStatusUpdateFieldMessageData updateData)
        {
            if (updateData.SensorStatus)
            {
                if (updateData.SensorUpdateInterval == 0)
                {
                    var readSensorStatusMessage = new InverterMessage(InverterIndex.MainInverter, (short)InverterParameterId.DigitalInputsOutputs);

                    this.logger.LogTrace($"2:ReadSensorStatusMessage={readSensorStatusMessage}");

                    this.inverterCommandQueue.Enqueue(readSensorStatusMessage);

                    this.forceStatusPublish = true;
                }
                else
                {
                    this.logger.LogTrace("3:Change sensor update interval");
                    this.sensorStatusUpdateTimer.Change(updateData.SensorUpdateInterval, updateData.SensorUpdateInterval);
                }
            }
            else
            {
                this.logger.LogTrace("4:Stop sensor update timer");
                this.sensorStatusUpdateTimer.Change(-1, Timeout.Infinite);
            }

            if (updateData.AxisPosition)
            {
                if (updateData.AxisUpdateInterval == 0)
                {
                    var readAxisPositionMessage = new InverterMessage(InverterIndex.MainInverter, (short)InverterParameterId.ActualPositionShaft);

                    this.logger.LogTrace($"5:ReadAxisPositionMessage={readAxisPositionMessage}");

                    this.inverterCommandQueue.Enqueue(readAxisPositionMessage);

                    this.forceStatusPublish = true;
                }
                else
                {
                    this.logger.LogTrace("6:Change axis update interval");
                    this.axisPositionUpdateTimer.Change(updateData.AxisUpdateInterval, updateData.AxisUpdateInterval);
                }
            }
            else
            {
                this.logger.LogTrace("7:Stop axis update timer");
                this.axisPositionUpdateTimer.Change(-1, Timeout.Infinite);
            }
        }

        private void EvaluateReadMessage(InverterMessage currentMessage, InverterIndex inverterIndex)
        {
            this.logger.LogTrace($"1:currentMessage={currentMessage}");

            if (currentMessage.ParameterId == InverterParameterId.StatusWordParam)
            {
                if (!this.currentStateMachine?.ValidateCommandResponse(currentMessage) ?? false)
                {
                    var readStatusWordMessage = new InverterMessage(inverterIndex, (short)InverterParameterId.StatusWordParam);

                    this.logger.LogTrace($"2:readStatusWordMessage={readStatusWordMessage}");

                    this.inverterCommandQueue.Enqueue(readStatusWordMessage);
                }
                else
                {
                    this.logger.LogTrace("3:Validate Command Response True");
                }
            }

            if (currentMessage.ParameterId == InverterParameterId.DigitalInputsOutputs)
            {
                this.logger.LogTrace($"4:StatusDigitalSignals.StringPayload={currentMessage.StringPayload}");

                var ioStatuses = this.RetrieveInverterIOStatus(currentMessage.StringPayload, inverterIndex);

                if (this.inverterStatuses.TryGetValue(inverterIndex, out var inverterStatus))
                {
                    switch (inverterStatus.InverterType)
                    {
                        case InverterType.Ang:
                            if (inverterStatus is AngInverterStatus angInverter)
                            {
                                if (angInverter.UpdateANGInverterInputsStates(ioStatuses) || this.forceStatusPublish)
                                {
                                    var notificationData = new InverterStatusUpdateFieldMessageData(angInverter.Inputs);
                                    var msgNotification = new FieldNotificationMessage(
                                        notificationData,
                                        "Inverter Inputs update",
                                        FieldMessageActor.FiniteStateMachines,
                                        FieldMessageActor.InverterDriver,
                                        FieldMessageType.InverterStatusUpdate,
                                        MessageStatus.OperationExecuting);

                                    this.eventAggregator?.GetEvent<FieldNotificationEvent>().Publish(msgNotification);

                                    this.forceStatusPublish = false;
                                }
                            }
                            break;

                        case InverterType.Acu:
                            if (inverterStatus is AcuInverterStatus acuInverter)
                            {
                                if (acuInverter.UpdateACUInverterInputsStates(ioStatuses) || this.forceStatusPublish)
                                {
                                    var notificationData = new InverterStatusUpdateFieldMessageData(acuInverter.Inputs);
                                    var msgNotification = new FieldNotificationMessage(
                                        notificationData,
                                        "Inverter Inputs update",
                                        FieldMessageActor.FiniteStateMachines,
                                        FieldMessageActor.InverterDriver,
                                        FieldMessageType.InverterStatusUpdate,
                                        MessageStatus.OperationExecuting);

                                    this.eventAggregator?.GetEvent<FieldNotificationEvent>().Publish(msgNotification);

                                    this.forceStatusPublish = false;
                                }
                            }
                            break;

                        case InverterType.Agl:
                            if (inverterStatus is AglInverterStatus aglInverter)
                            {
                                if (aglInverter.UpdateAGLInverterInputsStates(ioStatuses) || this.forceStatusPublish)
                                {
                                    var notificationData = new InverterStatusUpdateFieldMessageData(aglInverter.Inputs);
                                    var msgNotification = new FieldNotificationMessage(
                                        notificationData,
                                        "Inverter Inputs update",
                                        FieldMessageActor.FiniteStateMachines,
                                        FieldMessageActor.InverterDriver,
                                        FieldMessageType.InverterStatusUpdate,
                                        MessageStatus.OperationExecuting);

                                    this.eventAggregator?.GetEvent<FieldNotificationEvent>().Publish(msgNotification);

                                    this.forceStatusPublish = false;
                                }
                            }
                            break;
                    }
                }

                //TODO retrieve current inverter Status and Update its I/O Status, removing general InverterIoStatus from hosted Inverter Driver.
                //TODO e.g. MainInverter.UpdateANGInverterInputsStates(ioStatuses);

                ////if (this.inverterIoStatus.UpdateInputStates(ioStatuses) || this.forceStatusPublish)
                ////{
                ////    var notificationData = new InverterStatusUpdateFieldMessageData(this.inverterIoStatus.Inputs);
                ////    var msgNotification = new FieldNotificationMessage(notificationData,
                ////        "Inverter Inputs update",
                ////        FieldMessageActor.FiniteStateMachines,
                ////        FieldMessageActor.InverterDriver,
                ////        FieldMessageType.InverterStatusUpdate,
                ////        MessageStatus.OperationExecuting);

                ////    this.eventAggregator?.GetEvent<FieldNotificationEvent>().Publish(msgNotification);

                ////    this.forceStatusPublish = false;
                ////}
            }

            if (currentMessage.ParameterId == InverterParameterId.ActualPositionShaft)
            {
                this.logger.LogTrace($"5:ActualPositionShaft.UIntPayload={currentMessage.IntPayload}");

                if (this.inverterStatuses.TryGetValue(inverterIndex, out var inverterStatus))
                {
                    if (inverterStatus.InverterType == InverterType.Ang && inverterStatus is AngInverterStatus angInverter)
                    {
                        if (angInverter.UpdateANGInverterCurrentPosition(this.currentAxis, currentMessage.IntPayload) || this.forceStatusPublish)
                        {
                            if (this.shaftPositionUpdateNumberOfTimes == 10 || this.forceStatusPublish)
                            {
                                var notificationData = new InverterStatusUpdateFieldMessageData(this.currentAxis, angInverter.Inputs, currentMessage.IntPayload);
                                var msgNotification = new FieldNotificationMessage(
                                  notificationData,
                                  "Inverter encoder value update",
                                  FieldMessageActor.FiniteStateMachines,
                                  FieldMessageActor.InverterDriver,
                                  FieldMessageType.InverterStatusUpdate,
                                  MessageStatus.OperationExecuting);

                                this.eventAggregator?.GetEvent<FieldNotificationEvent>().Publish(msgNotification);

                                this.forceStatusPublish = false;
                            }
                            else
                            {
                                this.shaftPositionUpdateNumberOfTimes++;
                            }
                        }
                    }
                }

                //if (this.inverterIoStatus.UpdateInputStates(currentMessage.UShortPayload) || this.forceStatusPublish)
                //{
                //    var notificationData = new InverterStatusUpdateFieldMessageData(this.currentAxis, currentMessage.UShortPayload);
                //    var msgNotification = new FieldNotificationMessage(notificationData,
                //        "Inverter encoder value update",
                //        FieldMessageActor.FiniteStateMachines,
                //        FieldMessageActor.InverterDriver,
                //        FieldMessageType.InverterStatusUpdate,
                //        MessageStatus.OperationExecuting);

                //    this.eventAggregator?.GetEvent<FieldNotificationEvent>().Publish(msgNotification);

                //    this.forceStatusPublish = false;
                //}
            }
        }

        private void EvaluateWriteMessage(InverterMessage currentMessage, InverterIndex inverterIndex)
        {
            this.logger.LogTrace($"1:currentMessage={currentMessage}");

            if (currentMessage.ParameterId == InverterParameterId.ControlWordParam)
            {
                this.logger.LogTrace("2:Evaluate Control word");

                if (!this.inverterStatuses.TryGetValue(InverterIndex.MainInverter, out var inverterStatus))
                {
                    this.logger.LogTrace("3:Required Inverter Status not configured");

                    var ex = new Exception();
                    this.SendOperationErrorMessage(new InverterExceptionFieldMessageData(ex, "Requested Inverter is not configured", 0), FieldMessageType.InverterError);

                    return;
                }

                if (!(inverterStatus is AngInverterStatus mainInverterStatus))
                {
                    this.logger.LogTrace("4:Wrong inverter status");
                    return;
                }

                if (mainInverterStatus.WaitingHeartbeatAck)
                {
                    mainInverterStatus.WaitingHeartbeatAck = false;
                    this.logger.LogTrace("5:Reset Heartbeat flag");
                    return;
                }
            }
            if (this.currentStateMachine?.ValidateCommandMessage(currentMessage) ?? false)
            {
                this.logger.LogTrace("6:Request Status word");
                var readStatusWordMessage = new InverterMessage(inverterIndex, (short)InverterParameterId.StatusWordParam);
                this.inverterCommandQueue.Enqueue(readStatusWordMessage);
            }
        }

        private async Task InitializeInverterStatus()
        {
            var inverterList = await this.vertimagConfiguration.GetInstalledInverterListAsync();
            IInverterStatusBase inverterStatus = null;
            foreach (var inverterType in inverterList)
            {
                switch (inverterType.Value)
                {
                    case InverterType.Ang:
                        inverterStatus = new AngInverterStatus((byte)inverterType.Key);
                        break;

                    case InverterType.Acu:
                        inverterStatus = new AcuInverterStatus((byte)inverterType.Key);
                        break;

                    case InverterType.Agl:
                        inverterStatus = new AglInverterStatus((byte)inverterType.Key);
                        break;
                }

                this.inverterStatuses.Add(inverterType.Key, inverterStatus);
            }

            this.logger.LogTrace("1:Start Heart beat timer");

            this.heartBeatTimer?.Dispose();

            try
            {
                this.heartBeatTimer = new Timer(this.SendHeartBeat, null, TimeSpan.Zero, TimeSpan.FromMilliseconds(HEARTBEAT_TIMEOUT));
                this.sensorStatusUpdateTimer?.Change(SENSOR_STATUS_UPDATE_INTERVAL, SENSOR_STATUS_UPDATE_INTERVAL);
            }
            catch (Exception ex)
            {
                this.logger.LogCritical($"3:Exception: {ex.Message} while starting heartBeat update timer");

                this.SendOperationErrorMessage(new InverterExceptionFieldMessageData(ex, "Exception while starting heartBeat update timer", 0), FieldMessageType.InverterException);

                //TODO: try to re-create the timer and start once again
            }
        }

        private void InitializeMethodSubscriptions()
        {
            this.logger.LogTrace("1:Method Start");

            var commandEvent = this.eventAggregator.GetEvent<FieldCommandEvent>();
            commandEvent.Subscribe(
                message =>
            {
                this.commandQueue.Enqueue(message);
            },
                ThreadOption.PublisherThread,
                false,
                message => message.Destination == FieldMessageActor.InverterDriver || message.Destination == FieldMessageActor.Any);

            var notificationEvent = this.eventAggregator.GetEvent<FieldNotificationEvent>();
            notificationEvent.Subscribe(
                message =>
            {
                this.notificationQueue.Enqueue(message);
            },
                ThreadOption.PublisherThread,
                false,
                message => message.Destination == FieldMessageActor.InverterDriver || message.Destination == FieldMessageActor.Any);
        }

        private bool IsInverterPoweredOn(IInverterStatusBase inverterStatus)
        {
            return inverterStatus.CommonStatusWord.IsVoltageEnabled &
                   inverterStatus.CommonStatusWord.IsSwitchedOn;   //TODO: check this
        }

        private bool IsInverterStarted(IInverterStatusBase inverterStatus)
        {
            return inverterStatus.CommonStatusWord.IsReadyToSwitchOn &
                   inverterStatus.CommonStatusWord.IsSwitchedOn &
                   inverterStatus.CommonStatusWord.IsVoltageEnabled &
                   inverterStatus.CommonStatusWord.IsQuickStopTrue;
        }

        private void ProcessCalibrateAxisMessage(FieldCommandMessage receivedMessage)
        {
            if (receivedMessage.Data is ICalibrateAxisFieldMessageData calibrateData)
            {
                this.logger.LogTrace("1:Parse Message Data");

                //TODO define a rule to identify the Inverter to use for the specific axis to calibrate (Backlog Item 2649)
                var currentInverter = InverterIndex.MainInverter;

                if (!this.inverterStatuses.TryGetValue(currentInverter, out var inverterStatus))
                {
                    this.logger.LogTrace("2:Required Inverter Status not configured");

                    var ex = new Exception();
                    this.SendOperationErrorMessage(new InverterExceptionFieldMessageData(ex, "Requested Inverter is not configured", 0), FieldMessageType.CalibrateAxis);

                    return;
                }

                if (this.IsInverterStarted(inverterStatus))
                {
                    this.logger.LogTrace("3:Starting Calibrate Axis FSM");
                    this.currentAxis = calibrateData.AxisToCalibrate;
                    this.currentStateMachine = new CalibrateAxisStateMachine(this.currentAxis, inverterStatus, this.inverterCommandQueue, this.eventAggregator, this.logger);
                    this.currentStateMachine?.Start();
                }
                else
                {
                    this.logger.LogTrace("4:Inverter is not ready. Powering up the inverter");

                    this.currentStateMachine = new PowerOnStateMachine(inverterStatus, this.inverterCommandQueue, this.eventAggregator, this.logger, receivedMessage);
                    this.currentStateMachine?.Start();
                }
            }
            else
            {
                this.logger.LogTrace("5:Wrong message Data data type");

                var ex = new Exception();
                this.SendOperationErrorMessage(new InverterExceptionFieldMessageData(ex, "Wrong message Data data type", 0), FieldMessageType.CalibrateAxis);
            }
        }

        private async Task ProcessHeartbeat()
        {
            this.heartbeatQueue.Dequeue(out var message);

            try
            {
                this.inverterStatuses.TryGetValue(InverterIndex.MainInverter, out var inverterStatus);
                var newMessage = new InverterMessage(InverterIndex.MainInverter, (short)InverterParameterId.ControlWordParam, inverterStatus.CommonControlWord.Value);

                this.logger.LogTrace($"1:heartbeat inverterMessage={newMessage}");

                this.roundTripStopwatch.Reset();
                this.roundTripStopwatch.Start();
                await this.socketTransport.WriteAsync(newMessage.GetHeartbeatMessage(newMessage.HeartbeatValue), this.stoppingToken);
            }
            catch (InverterDriverException ex)
            {
                this.logger.LogCritical($"Exception {ex.Message}, InverterExceptionCode={ex.InverterDriverExceptionCode}");
            }
        }

        private async Task ProcessInverterCommand()
        {
            this.inverterCommandQueue.Dequeue(out var message);

            this.logger.LogTrace($"1:ParameterId={message.ParameterId}:IsWriteMessage={message.IsWriteMessage}:SendDelay{message.SendDelay}");

            var inverterMessagePacket = message.IsWriteMessage ? message.GetWriteMessage() : message.GetReadMessage();
            if (message.SendDelay > 0)
            {
                try
                {
                    this.roundTripStopwatch.Reset();
                    this.roundTripStopwatch.Start();
                    await this.socketTransport.WriteAsync(inverterMessagePacket, message.SendDelay, this.stoppingToken);
                }
                catch (InverterDriverException ex)
                {
                    this.logger.LogCritical($"Exception {ex.Message}, InverterExceptionCode={ex.InverterDriverExceptionCode}");
                }
            }
            else
            {
                try
                {
                    this.roundTripStopwatch.Reset();
                    this.roundTripStopwatch.Start();
                    await this.socketTransport.WriteAsync(inverterMessagePacket, this.stoppingToken);
                }
                catch (InverterDriverException ex)
                {
                    this.logger.LogCritical($"Exception {ex.Message}, InverterExceptionCode={ex.InverterDriverExceptionCode}");
                }
            }
        }

        private void ProcessInverterStatusUpdateMessage(FieldCommandMessage receivedMessage)
        {
            this.logger.LogTrace("1:Method Start");
            if (receivedMessage.Data is IInverterStatusUpdateFieldMessageData updateData)
            {
                this.ConfigureUpdates(updateData);
            }
            else
            {
                this.logger.LogTrace("2:Wrong message Data data type");

                var ex = new Exception();
                this.SendOperationErrorMessage(new InverterExceptionFieldMessageData(ex, "Wrong message Data data type", 0), FieldMessageType.InverterStatusUpdate);
            }
        }

        private void ProcessInverterSwitchOffMessage(FieldCommandMessage message)
        {
            this.logger.LogTrace("1:Method Start");

            if (message.Data is IInverterSwitchOffFieldMessageData switchOffData)
            {
                if (this.inverterStatuses.TryGetValue(switchOffData.SystemIndex, out var inverterStatus))
                {
                    this.currentStateMachine = new SwitchOffStateMachine(inverterStatus, this.inverterCommandQueue, this.eventAggregator, this.logger);
                    this.currentStateMachine?.Start();
                }
                else
                {
                    this.logger.LogTrace("2:Inverter status not configured for requested inverter Type");

                    var ex = new Exception();
                    this.SendOperationErrorMessage(new InverterExceptionFieldMessageData(ex, "Inverter status not configured for requested inverter Type", 0), FieldMessageType.InverterSwitchOff);
                }
            }
            else
            {
                this.logger.LogTrace($"3:Invalid message data for InverterStop message Type");

                var ex = new Exception();
                this.SendOperationErrorMessage(new InverterExceptionFieldMessageData(ex, "Invalid message data for InverterStop message Type", 0), FieldMessageType.InverterSwitchOff);
            }
        }

        private void ProcessInverterSwitchOnMessage(FieldCommandMessage message)
        {
            this.logger.LogTrace("1:Method Start");

            if (message.Data is IInverterSwitchOnFieldMessageData switchOnData)
            {
                if (this.inverterStatuses.TryGetValue(switchOnData.SystemIndex, out var inverterStatus))
                {
                    if (inverterStatus.CommonStatusWord.IsReadyToSwitchOn &
                        inverterStatus.CommonStatusWord.IsVoltageEnabled &
                        inverterStatus.CommonStatusWord.IsQuickStopTrue)
                    {
                        if (inverterStatus.CommonControlWord.HorizontalAxis == (switchOnData.AxisToSwitchOn == Axis.Horizontal))
                        {
                            if (inverterStatus.CommonStatusWord.IsSwitchedOn)
                            {
                                var notificationMessageData = new InverterSwitchOnFieldMessageData(switchOnData.AxisToSwitchOn, switchOnData.SystemIndex);
                                var notificationMessage = new FieldNotificationMessage(
                                    notificationMessageData,
                                    $"Inverter Switch On on axis {switchOnData.AxisToSwitchOn} End",
                                    FieldMessageActor.Any,
                                    FieldMessageActor.InverterDriver,
                                    FieldMessageType.InverterSwitchOn,
                                    MessageStatus.OperationEnd);

                                this.logger.LogTrace($"2:Type={notificationMessage.Type}:Destination={notificationMessage.Destination}:Status={notificationMessage.Status}");

                                this.eventAggregator?.GetEvent<FieldNotificationEvent>().Publish(notificationMessage);
                            }
                            else
                            {
                                this.logger.LogDebug("3: Switch On the inverter state machine");

                                this.currentStateMachine = new SwitchOnStateMachine(switchOnData.AxisToSwitchOn, inverterStatus, this.inverterCommandQueue, this.eventAggregator, this.logger);
                                this.currentStateMachine.Start();
                            }
                        }
                        else
                        {
                            this.logger.LogDebug("4: Switch Off the inverter state machine");

                            inverterStatus.CommonControlWord.HorizontalAxis = switchOnData.AxisToSwitchOn == Axis.Horizontal;

                            this.currentStateMachine = new SwitchOffStateMachine(inverterStatus, this.inverterCommandQueue, this.eventAggregator, this.logger, message);
                            this.currentStateMachine?.Start();
                        }
                    }
                    else
                    {
                        this.logger.LogDebug("5: Power On the inverter state machine");

                        inverterStatus.CommonControlWord.HorizontalAxis = switchOnData.AxisToSwitchOn == Axis.Horizontal;

                        this.currentStateMachine = new PowerOnStateMachine(inverterStatus, this.inverterCommandQueue, this.eventAggregator, this.logger, message);
                        this.currentStateMachine.Start();
                    }
                }
                else
                {
                    this.logger.LogTrace("2:Inverter status not configured for requested inverter Type");

                    var ex = new Exception();
                    this.SendOperationErrorMessage(new InverterExceptionFieldMessageData(ex, "Inverter status not configured for requested inverter Type", 0), FieldMessageType.InverterSwitchOn);
                }
            }
            else
            {
                this.logger.LogTrace("3:Invalid message data for InverterStop message Type");

                var ex = new Exception();
                this.SendOperationErrorMessage(new InverterExceptionFieldMessageData(ex, "Invalid message data for InverterStop message Type", 0), FieldMessageType.InverterSwitchOn);
            }
        }

        private void ProcessPositioningMessage(FieldCommandMessage receivedMessage)
        {
            if (receivedMessage.Data is IPositioningFieldMessageData positioningData)
            {
                this.logger.LogTrace("1:Parse Message Data");

                //TODO define a rule to identify the Inverter to use for the specific axis to calibrate (Backlog Item 2651)
                var currentInverter = InverterIndex.MainInverter;

                if (!this.inverterStatuses.TryGetValue(currentInverter, out var inverterStatus))
                {
                    this.logger.LogTrace("2:Required Inverter Status not configured");

                    var ex = new Exception();
                    this.SendOperationErrorMessage(new InverterExceptionFieldMessageData(ex, "Required Inverter Status not configured", 0), FieldMessageType.Positioning);
                }

                if (this.IsInverterStarted(inverterStatus))
                {
                    this.axisPositionUpdateTimer?.Change(AXIS_POSITION_UPDATE_INTERVAL, AXIS_POSITION_UPDATE_INTERVAL);
                    this.currentAxis = positioningData.AxisMovement;

                    this.shaftPositionUpdateNumberOfTimes = 0;

                    this.logger.LogTrace("4:Starting Positioning FSM");

                    var verticalPositioningData = new InverterPositioningFieldMessageData(positioningData);

                    if (inverterStatus is AngInverterStatus currentStatus)
                    {
                        this.logger.LogTrace($"1:CurrentPositionAxisVertical = {currentStatus.CurrentPositionAxisVertical}");
                        this.logger.LogTrace($"2:data.TargetPosition = {verticalPositioningData.TargetPosition}");

                        if (currentStatus.CurrentPositionAxisVertical == verticalPositioningData.TargetPosition)
                        {
                            var msgNotification = new FieldNotificationMessage(
                                null,
                                "Axis already in position",
                                FieldMessageActor.FiniteStateMachines,
                                FieldMessageActor.InverterDriver,
                                FieldMessageType.Positioning,
                                MessageStatus.OperationEnd);

                            this.eventAggregator?.GetEvent<FieldNotificationEvent>().Publish(msgNotification);
                        }
                        else
                        {
                            this.currentStateMachine = new PositioningStateMachine(verticalPositioningData, inverterStatus, this.inverterCommandQueue, this.eventAggregator, this.logger);
                            this.currentStateMachine?.Start();
                        }
                    }
                }
                else
                {
                    this.logger.LogTrace("5:Inverter is not ready. Powering up the inverter");

                    this.currentStateMachine = new PowerOnStateMachine(inverterStatus, this.inverterCommandQueue, this.eventAggregator, this.logger, receivedMessage);
                    this.currentStateMachine?.Start();
                }
            }
            else
            {
                this.logger.LogTrace("6:Wrong message Data data type");

                var ex = new Exception();
                this.SendOperationErrorMessage(new InverterExceptionFieldMessageData(ex, "Wrong message Data data type", 0), FieldMessageType.Positioning);
            }
        }

        private void ProcessPowerOffMessage(FieldCommandMessage receivedMessage)
        {
            if (receivedMessage.Data is IInverterPowerOffFieldMessageData powerOffData)
            {
                this.logger.LogTrace("1:Parse Message Data");

                var currentInverter = ((InverterPowerOffFieldMessageData)receivedMessage.Data).InverterToPowerOff;
                if (!this.inverterStatuses.TryGetValue(currentInverter, out var inverterStatus))
                {
                    this.logger.LogTrace("2:Required Inverter Status not configured");

                    var ex = new Exception();
                    this.SendOperationErrorMessage(new InverterExceptionFieldMessageData(ex, "Required Inverter Status not configured", 0), FieldMessageType.InverterPowerOff);
                }

                if (this.IsInverterStarted(inverterStatus))
                {
                    this.logger.LogTrace("3:Starting Power Off FSM");
                    this.currentStateMachine = new PowerOffStateMachine(
                        inverterStatus,
                        this.inverterCommandQueue,
                        this.eventAggregator,
                        this.logger);
                    this.currentStateMachine?.Start();
                }
                else
                {
                    this.logger.LogTrace("4:Inverter already powered off. Just notify operation completed");
                    var endNotification = new FieldNotificationMessage(
                        new InverterPowerOnFieldMessageData(powerOffData.InverterToPowerOff),
                        "Inverter Started",
                        FieldMessageActor.Any,
                        FieldMessageActor.InverterDriver,
                        FieldMessageType.InverterPowerOff,
                        MessageStatus.OperationEnd);

                    this.eventAggregator?.GetEvent<FieldNotificationEvent>().Publish(endNotification);
                }
            }
            else
            {
                this.logger.LogTrace("5:Wrong message Data data type");

                var ex = new Exception();
                this.SendOperationErrorMessage(new InverterExceptionFieldMessageData(ex, "Wrong message Data data type", 0), FieldMessageType.InverterPowerOff);
            }
        }

        private void ProcessPowerOnMessage(FieldCommandMessage receivedMessage)
        {
            if (receivedMessage.Data is IInverterPowerOnFieldMessageData powerOnData)
            {
                this.logger.LogTrace("1:Parse Message Data");

                var currentInverter = ((InverterPowerOnFieldMessageData)receivedMessage.Data).InverterToPowerOn;
                if (!this.inverterStatuses.TryGetValue(currentInverter, out var inverterStatus))
                {
                    this.logger.LogTrace("2:Required Inverter Status not configured");

                    var ex = new Exception();
                    this.SendOperationErrorMessage(new InverterExceptionFieldMessageData(ex, "Required Inverter Status not configured", 0), FieldMessageType.InverterPowerOn);
                }

                if (this.IsInverterStarted(inverterStatus))
                {
                    this.logger.LogTrace("3:Inverter already powered on. Just notify operation completed");
                    var endNotification = new FieldNotificationMessage(
                        new InverterPowerOnFieldMessageData(powerOnData.InverterToPowerOn),
                        "Inverter Started",
                        FieldMessageActor.Any,
                        FieldMessageActor.InverterDriver,
                        FieldMessageType.InverterPowerOn,
                        MessageStatus.OperationEnd);

                    this.eventAggregator?.GetEvent<FieldNotificationEvent>().Publish(endNotification);
                }
                else
                {
                    this.logger.LogTrace("4:Starting Power On FSM");
                    this.currentStateMachine = new PowerOnStateMachine(inverterStatus, this.inverterCommandQueue, this.eventAggregator, this.logger);
                    this.currentStateMachine?.Start();
                }
            }
            else
            {
                this.logger.LogTrace("5:Wrong message Data data type");

                var ex = new Exception();
                this.SendOperationErrorMessage(new InverterExceptionFieldMessageData(ex, "Wrong message Data data type", 0), FieldMessageType.InverterPowerOn);
            }
        }

        private void ProcessShutterPositioningMessage(FieldCommandMessage receivedMessage)
        {
            if (receivedMessage.Data is IShutterPositioningFieldMessageData shutterPositioningData)
            {
                this.logger.LogTrace("1:Parse Message Data");

                var currentInverter = InverterIndex.Slave2;

                if (!this.inverterStatuses.TryGetValue(currentInverter, out var inverterStatus))
                {
                    this.logger.LogTrace("2:Required Inverter Status not configured");

                    var ex = new Exception();
                    this.SendOperationErrorMessage(new InverterExceptionFieldMessageData(ex, "Required Inverter Status not configured", 0), FieldMessageType.ShutterPositioning);

                    return;
                }

                if (this.IsInverterPoweredOn(inverterStatus))
                {
                    this.logger.LogTrace("3:Inverter start powering off");

                    this.currentStateMachine = new PowerOffStateMachine(inverterStatus, this.inverterCommandQueue, this.eventAggregator, this.logger, receivedMessage);
                }
                else
                {
                    this.logger.LogTrace("4:Starting ShutterPositioning FSM");

                    var convertedShutterPositioningData = new InverterShutterPositioningFieldMessageData(shutterPositioningData);
                    this.currentStateMachine = new ShutterPositioningStateMachine(convertedShutterPositioningData, this.inverterCommandQueue, inverterStatus, this.eventAggregator, this.logger);
                    this.currentStateMachine?.Start();
                }
            }
            else
            {
                this.logger.LogTrace("5:Wrong message Data data type");

                var ex = new Exception();
                this.SendOperationErrorMessage(new InverterExceptionFieldMessageData(ex, "Wrong message Data data type", 0), FieldMessageType.ShutterPositioning);
            }
        }

        private void ProcessStopMessage(FieldCommandMessage message)
        {
            this.logger.LogTrace("1:Method Start");

            if (message.Data is IInverterStopFieldMessageData stopData)
            {
                if (this.inverterStatuses.TryGetValue(stopData.InverterToStop, out var inverterStatus))
                {
                    this.currentStateMachine = new StopStateMachine(inverterStatus, this.inverterCommandQueue, this.eventAggregator, this.logger);
                    this.currentStateMachine?.Start();
                }
                else
                {
                    this.logger.LogTrace("2:Inverter status not configured for requested inverter Type");

                    var ex = new Exception();
                    this.SendOperationErrorMessage(new InverterExceptionFieldMessageData(ex, $"Inverter status not configured for requested inverter {stopData.InverterToStop}", 0), FieldMessageType.InverterStop);
                }
            }
            else
            {
                this.logger.LogTrace("3:Invalid message data for InverterStop message Type");

                var ex = new Exception();
                this.SendOperationErrorMessage(new InverterExceptionFieldMessageData(ex, "Invalid message data for InverterStop message type", 0), FieldMessageType.InverterStop);
            }
        }

        private void RequestAxisPositionUpdate(object state)
        {
            var readAxisPositionMessage = new InverterMessage(InverterIndex.MainInverter, (short)InverterParameterId.ActualPositionShaft);

            this.logger.LogTrace($"1:ReadAxisPositionMessage={readAxisPositionMessage}");

            this.inverterCommandQueue.Enqueue(readAxisPositionMessage);
        }

        private void RequestSensorStatusUpdate(object state)
        {
            var readSensorStatusMessage = new InverterMessage(InverterIndex.MainInverter, (short)InverterParameterId.DigitalInputsOutputs);

            this.logger.LogTrace($"1:ReadSensorStatusMessage={readSensorStatusMessage}");

            this.inverterCommandQueue.Enqueue(readSensorStatusMessage);
        }

        private bool[] RetrieveInverterIOStatus(string currentMessageStringPayload, InverterIndex inverterIndex)
        {
            //TEMP NOTE ==>
            // int i = Array.IndexOf(this.inverterStatuses.Keys.ToArray(), (ushort)inverterIndex);  // retrieve the first occurrence in the dictionary
            // and use i instead the parameter inverterIndex
            var returnValue = new bool[8];

            if (!string.IsNullOrEmpty(currentMessageStringPayload))
            {
                var regex = new Regex("[ ]{2,}", RegexOptions.None);
                var cleanString = regex.Replace(currentMessageStringPayload, " ").Trim();
                var encodedValues = cleanString.Split(" ");

                var encodedWord = encodedValues[(ushort)inverterIndex / 2];

                if (!encodedWord.Equals("\0"))
                {
                    var values = ushort.Parse(encodedWord);

                    var dataByte = (ushort)inverterIndex % 2;

                    for (var index = 8 * dataByte; index < 8 + (8 * dataByte); index++)
                    {
                        returnValue[index - (8 * dataByte)] = (values & 0x0001 << index) > 0;
                    }
                }
            }

            return returnValue;
        }

        private void SendHeartBeat(object state)
        {
            if (!this.inverterStatuses.TryGetValue(InverterIndex.MainInverter, out var inverterStatus))
            {
                this.logger.LogTrace("1:Inverter status not configured for Main Inverter");

                var ex = new Exception();
                this.SendOperationErrorMessage(new InverterExceptionFieldMessageData(ex, "Inverter status not configured for Main Inverter", 0), FieldMessageType.InverterError);

                return;
            }

            inverterStatus.CommonControlWord.HeartBeat = !inverterStatus.CommonControlWord.HeartBeat;
            if (inverterStatus is AngInverterStatus mainInverterStatus)
            {
                mainInverterStatus.WaitingHeartbeatAck = true;
            }
            var message = new InverterMessage(InverterIndex.MainInverter, (short)InverterParameterId.ControlWordParam, inverterStatus.CommonControlWord.Value);
            this.heartbeatQueue.Enqueue(message);
        }

        private async Task StartHardwareCommunications()
        {
            this.logger.LogTrace("1:Method Start");

            var inverterAddress = await this.dataLayerConfigurationValueManagement.GetIPAddressConfigurationValueAsync((long)SetupNetwork.Inverter1, (long)ConfigurationCategory.SetupNetwork);
            var inverterPort = await this.dataLayerConfigurationValueManagement.GetIntegerConfigurationValueAsync((long)SetupNetwork.Inverter1Port, (long)ConfigurationCategory.SetupNetwork);

            this.socketTransport.Configure(inverterAddress, inverterPort);

            try
            {
                await this.socketTransport.ConnectAsync();
            }
            catch (InverterDriverException ex)
            {
                this.logger.LogCritical($"1A: Exception {ex.Message}; Exception code={ex.InverterDriverExceptionCode}");
            }
            catch (Exception ex)
            {
                this.logger.LogCritical($"2:Exception {ex.Message} while Connecting Receiver Socket Transport");

                this.SendOperationErrorMessage(new InverterExceptionFieldMessageData(ex, "while Connecting Receiver Socket Transport", 0), FieldMessageType.InverterException);
            }

            if (!this.socketTransport.IsConnected)
            {
                this.logger.LogCritical("3:Socket Transport failed to connect");

                var ex = new Exception();
                this.SendOperationErrorMessage(new InverterExceptionFieldMessageData(ex, "Socket Transport failed to connect", 0), FieldMessageType.InverterError);
            }

            try
            {
                this.inverterReceiveTask.Start();
                this.inverterSendTask.Start();
            }
            catch (Exception ex)
            {
                this.logger.LogCritical($"4:Exception: {ex.Message} while starting service threads");

                this.SendOperationErrorMessage(new InverterExceptionFieldMessageData(ex, "while starting service threads", 0), FieldMessageType.InverterException);
            }
        }

        #endregion
    }
}
