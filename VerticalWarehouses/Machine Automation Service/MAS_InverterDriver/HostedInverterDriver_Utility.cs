using System;
using System.Threading;
using System.Threading.Tasks;
using Ferretto.VW.Common_Utils.Messages.Data;
using Ferretto.VW.Common_Utils.Messages.Enumerations;
using Ferretto.VW.MAS_DataLayer.Enumerations;
using Ferretto.VW.MAS_InverterDriver.Enumerations;
using Ferretto.VW.MAS_InverterDriver.InverterStatus;
using Ferretto.VW.MAS_InverterDriver.InverterStatus.Interfaces;
using Ferretto.VW.MAS_InverterDriver.StateMachines.CalibrateAxis;
using Ferretto.VW.MAS_InverterDriver.StateMachines.PowerOff;
using Ferretto.VW.MAS_InverterDriver.StateMachines.PowerOn;
using Ferretto.VW.MAS_InverterDriver.StateMachines.Stop;
using Ferretto.VW.MAS_InverterDriver.StateMachines.SwitchOff;
using Ferretto.VW.MAS_InverterDriver.StateMachines.SwitchOn;
using Ferretto.VW.MAS_InverterDriver.StateMachines.VerticalPositioning;
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
            this.logger.LogDebug("1:Method Start");

            if (updateData.SensorStatus)
            {
                if (updateData.SensorUpdateInterval == 0)
                {
                    var readSensorStatusMessage = new InverterMessage(InverterIndex.MainInverter, (short)InverterParameterId.StatusDigitalSignals);

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

            this.logger.LogDebug("8:Method End");
        }

        private void EvaluateReadMessage(InverterMessage currentMessage, InverterIndex inverterIndex)
        {
            this.logger.LogDebug("1:Method Start");

            this.logger.LogTrace($"2:currentMessage={currentMessage}");

            if (currentMessage.ParameterId == InverterParameterId.StatusWordParam)
            {
                if (!this.currentStateMachine?.ValidateCommandResponse(currentMessage) ?? false)
                {
                    var readStatusWordMessage = new InverterMessage(inverterIndex, (short)InverterParameterId.StatusWordParam);

                    this.logger.LogTrace($"3:readStatusWordMessage={readStatusWordMessage}");

                    this.inverterCommandQueue.Enqueue(readStatusWordMessage);
                }
            }

            if (currentMessage.ParameterId == InverterParameterId.StatusDigitalSignals)
            {
                this.logger.LogTrace($"4:StatusDigitalSignals.UShortPayload={currentMessage.UShortPayload}");

                if (this.inverterIoStatus.UpdateInputStates(currentMessage.UShortPayload) || this.forceStatusPublish)
                {
                    var notificationData = new InverterStatusUpdateFieldMessageData(this.inverterIoStatus.Inputs);
                    var errorNotification = new FieldNotificationMessage(notificationData,
                        "Inverter Inputs update",
                        FieldMessageActor.FiniteStateMachines,
                        FieldMessageActor.InverterDriver,
                        FieldMessageType.InverterStatusUpdate,
                        MessageStatus.OperationExecuting);

                    this.eventAggregator?.GetEvent<FieldNotificationEvent>().Publish(errorNotification);

                    this.forceStatusPublish = false;
                }
            }

            if (currentMessage.ParameterId == InverterParameterId.ActualPositionShaft)
            {
                this.logger.LogTrace($"5:ActualPositionShaft.UShortPayload={currentMessage.UShortPayload}");

                if (this.inverterIoStatus.UpdateInputStates(currentMessage.UShortPayload) || this.forceStatusPublish)
                {
                    var notificationData = new InverterStatusUpdateFieldMessageData(this.currentAxis, currentMessage.UShortPayload);
                    var errorNotification = new FieldNotificationMessage(notificationData,
                        "Inverter encoder value update",
                        FieldMessageActor.FiniteStateMachines,
                        FieldMessageActor.InverterDriver,
                        FieldMessageType.InverterStatusUpdate,
                        MessageStatus.OperationExecuting);

                    this.eventAggregator?.GetEvent<FieldNotificationEvent>().Publish(errorNotification);

                    this.forceStatusPublish = false;
                }
            }

            this.logger.LogDebug("6:Method End");
        }

        private void EvaluateWriteMessage(InverterMessage currentMessage, InverterIndex inverterIndex)
        {
            this.logger.LogDebug("1:Method Start");

            this.logger.LogTrace($"2:currentMessage={currentMessage}");

            if (this.currentStateMachine?.ValidateCommandMessage(currentMessage) ?? false)
            {
                var readStatusWordMessage = new InverterMessage(inverterIndex, (short)InverterParameterId.StatusWordParam);
                this.inverterCommandQueue.Enqueue(readStatusWordMessage);

                this.logger.LogTrace($"3:readStatusWordMessage={readStatusWordMessage}");
            }

            this.logger.LogDebug("4:Method End");
        }

        private async Task InitializeInverterStatus()
        {
            this.logger.LogDebug("1:Method Start");

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
                        throw new NotImplementedException();

                    case InverterType.Agl:
                        throw new NotImplementedException();
                }

                this.inverterStatuses.Add(inverterType.Key, inverterStatus);
            }

            this.logger.LogTrace("2:Start Heart beat timer");

            this.heartBeatTimer?.Dispose();

            try
            {
                this.heartBeatTimer = new Timer(this.SendHeartBeat, null, TimeSpan.Zero, TimeSpan.FromMilliseconds(HEARTBEAT_TIMEOUT));
                this.sensorStatusUpdateTimer?.Change(SENSOR_STATUS_UPDATE_INTERVAL, SENSOR_STATUS_UPDATE_INTERVAL);
            }
            catch (Exception ex)
            {
                this.logger.LogCritical($"3:Exception: {ex.Message} while starting sensor update timer");

                //TEMP throw new InverterDriverException($"Exception: {ex.Message} while starting sensor update timer", ex);

                this.SendMessage(new ExceptionMessageData(ex, "", 0));
            }

            this.logger.LogDebug("4:Method End");
        }

        private void InitializeMethodSubscriptions()
        {
            this.logger.LogDebug("1:Method Start");

            var commandEvent = this.eventAggregator.GetEvent<FieldCommandEvent>();
            commandEvent.Subscribe(message =>
            {
                this.commandQueue.Enqueue(message);
            },
                ThreadOption.PublisherThread,
                false,
                message => message.Destination == FieldMessageActor.InverterDriver || message.Destination == FieldMessageActor.Any);

            var notificationEvent = this.eventAggregator.GetEvent<FieldNotificationEvent>();
            notificationEvent.Subscribe(message =>
            {
                this.notificationQueue.Enqueue(message);
            },
                ThreadOption.PublisherThread,
                false,
                message => message.Destination == FieldMessageActor.InverterDriver || message.Destination == FieldMessageActor.Any);

            this.logger.LogDebug("2:Method End");
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
            this.logger.LogDebug("1:Method Start");

            if (receivedMessage.Data is ICalibrateAxisFieldMessageData calibrateData)
            {
                this.logger.LogTrace("2:Parse Message Data");

                //TODO define a rule to identify the Inverter to use for the specific axis to calibrate (Backlog Item 2649)
                var currentInverter = InverterIndex.MainInverter;

                if (!this.inverterStatuses.TryGetValue(currentInverter, out var inverterStatus))
                {
                    this.logger.LogTrace("3:Required Inverter Status not configured");

                    var errorNotification = new FieldNotificationMessage(calibrateData,
                        "Requested Inverter is not configured",
                        FieldMessageActor.Any,
                        FieldMessageActor.InverterDriver,
                        FieldMessageType.CalibrateAxis,
                        MessageStatus.OperationError,
                        ErrorLevel.Critical);

                    this.eventAggregator?.GetEvent<FieldNotificationEvent>().Publish(errorNotification);
                    return;
                }

                if (this.IsInverterStarted(inverterStatus))
                {
                    this.logger.LogTrace("4:Starting Calibrate Axis FSM");
                    this.currentAxis = calibrateData.AxisToCalibrate;
                    this.currentStateMachine = new CalibrateAxisStateMachine(this.currentAxis, inverterStatus, this.inverterCommandQueue, this.eventAggregator, this.logger);
                    this.currentStateMachine?.Start();
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
                var errorNotification = new FieldNotificationMessage(receivedMessage.Data,
                    "Wrong message Data data type",
                    FieldMessageActor.Any,
                    FieldMessageActor.InverterDriver,
                    FieldMessageType.CalibrateAxis,
                    MessageStatus.OperationError,
                    ErrorLevel.Critical);

                this.eventAggregator?.GetEvent<FieldNotificationEvent>().Publish(errorNotification);
            }

            this.logger.LogDebug("7:Method End");
        }

        private async Task ProcessHeartbeat()
        {
            this.logger.LogDebug("1:Method Start");

            while (this.heartbeatQueue.Dequeue(out var message))
            {
                this.logger.LogTrace($"2:message={message}");

                await this.socketTransport.WriteAsync(message.GetHeartbeatMessage(message.HeartbeatValue), this.stoppingToken);
            }

            this.logger.LogDebug("3:Method End");
        }

        private async Task ProcessInverterCommand()
        {
            this.logger.LogDebug("1:Method Start");

            while (this.inverterCommandQueue.Dequeue(out var message))
            {
                this.logger.LogTrace($"2:ParameterId={message.ParameterId}:IsWriteMessage={message.IsWriteMessage}:SendDelay{message.SendDelay}");

                var inverterMessagePacket = message.IsWriteMessage ? message.GetWriteMessage() : message.GetReadMessage();
                if (message.SendDelay > 0)
                {
                    await this.socketTransport.WriteAsync(inverterMessagePacket, message.SendDelay, this.stoppingToken);
                }
                else
                {
                    await this.socketTransport.WriteAsync(inverterMessagePacket, this.stoppingToken);
                }
            }

            this.logger.LogDebug("3:Method End");
        }

        private void ProcessInverterStatusUpdateMessage(FieldCommandMessage receivedMessage)
        {
            this.logger.LogDebug("1:Method Start");
            if (receivedMessage.Data is IInverterStatusUpdateFieldMessageData updateData)
            {
                this.ConfigureUpdates(updateData);
            }
            else
            {
                this.logger.LogTrace("2:Wrong message Data data type");
                var errorNotification = new FieldNotificationMessage(receivedMessage.Data,
                    "Wrong message Data data type",
                    FieldMessageActor.Any,
                    FieldMessageActor.InverterDriver,
                    FieldMessageType.InverterStatusUpdate,
                    MessageStatus.OperationError,
                    ErrorLevel.Critical);

                this.eventAggregator?.GetEvent<FieldNotificationEvent>().Publish(errorNotification);
            }

            this.logger.LogDebug("3:Method End");
        }

        private void ProcessInverterSwitchOffMessage(FieldCommandMessage message)
        {
            this.logger.LogDebug("1:Method Start");

            if (message.Data is IInverterSwitchOffFieldMessageData switchOffData)
            {
                if (this.inverterStatuses.TryGetValue(switchOffData.SystemIndex, out var inverterStatus))
                {
                    this.currentStateMachine = new SwitchOffStateMachine(inverterStatus, this.inverterCommandQueue, this.eventAggregator, this.logger);
                    this.currentStateMachine?.Start();
                }
                else
                {
                    var errorNotification = new FieldNotificationMessage(message.Data,
                        $"Inverter status not configured for requested inverter {switchOffData.SystemIndex}",
                        FieldMessageActor.Any,
                        FieldMessageActor.InverterDriver,
                        FieldMessageType.InverterSwitchOff,
                        MessageStatus.OperationError,
                        ErrorLevel.Critical);

                    this.logger.LogTrace($"2:Inverter status not configured for requested inverter Type ={errorNotification.Type}:Destination={errorNotification.Destination}:Status={errorNotification.Status}");

                    this.eventAggregator?.GetEvent<FieldNotificationEvent>().Publish(errorNotification);
                }
            }
            else
            {
                var errorNotification = new FieldNotificationMessage(message.Data,
                    "Invalid message data for InverterStop message type",
                    FieldMessageActor.Any,
                    FieldMessageActor.InverterDriver,
                    FieldMessageType.InverterSwitchOff,
                    MessageStatus.OperationError,
                    ErrorLevel.Error);

                this.logger.LogTrace($"3:Invalid message data for InverterStop message Type={errorNotification.Type}:Destination={errorNotification.Destination}:Status={errorNotification.Status}");

                this.eventAggregator?.GetEvent<FieldNotificationEvent>().Publish(errorNotification);
            }

            this.logger.LogDebug("4:Method End");
        }

        private void ProcessInverterSwitchOnMessage(FieldCommandMessage message)
        {
            this.logger.LogDebug("1:Method Start");

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
                                var notificationMessage = new FieldNotificationMessage(notificationMessageData,
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
                                this.currentStateMachine = new SwitchOnStateMachine(switchOnData.AxisToSwitchOn, inverterStatus, this.inverterCommandQueue, this.eventAggregator, this.logger);
                                this.currentStateMachine.Start();
                            }
                        }
                        else
                        {
                            inverterStatus.CommonControlWord.HorizontalAxis = switchOnData.AxisToSwitchOn == Axis.Horizontal;

                            this.currentStateMachine = new SwitchOffStateMachine(inverterStatus, this.inverterCommandQueue, this.eventAggregator, this.logger, message);
                            this.currentStateMachine?.Start();
                        }
                    }
                    else
                    {
                        inverterStatus.CommonControlWord.HorizontalAxis = switchOnData.AxisToSwitchOn == Axis.Horizontal;

                        this.currentStateMachine = new PowerOnStateMachine(inverterStatus, this.inverterCommandQueue, this.eventAggregator, this.logger, message);
                        this.currentStateMachine.Start();
                    }
                }
                else
                {
                    var errorNotification = new FieldNotificationMessage(message.Data,
                        $"Inverter status not configured for requested inverter {switchOnData.SystemIndex}",
                        FieldMessageActor.Any,
                        FieldMessageActor.InverterDriver,
                        FieldMessageType.InverterSwitchOn,
                        MessageStatus.OperationError,
                        ErrorLevel.Critical);

                    this.logger.LogTrace($"2:Inverter status not configured for requested inverter Type ={errorNotification.Type}:Destination={errorNotification.Destination}:Status={errorNotification.Status}");

                    this.eventAggregator?.GetEvent<FieldNotificationEvent>().Publish(errorNotification);
                }
            }
            else
            {
                var errorNotification = new FieldNotificationMessage(message.Data,
                    "Invalid message data for InverterStop message type",
                    FieldMessageActor.Any,
                    FieldMessageActor.InverterDriver,
                    FieldMessageType.InverterSwitchOn,
                    MessageStatus.OperationError,
                    ErrorLevel.Error);

                this.logger.LogTrace($"3:Invalid message data for InverterStop message Type={errorNotification.Type}:Destination={errorNotification.Destination}:Status={errorNotification.Status}");

                this.eventAggregator?.GetEvent<FieldNotificationEvent>().Publish(errorNotification);
            }

            this.logger.LogDebug("4:Method End");
        }

        private void ProcessPositioningMessage(FieldCommandMessage receivedMessage)
        {
            this.logger.LogDebug("1:Method Start");

            if (receivedMessage.Data is IPositioningFieldMessageData positioningData)
            {
                this.logger.LogTrace("2:Parse Message Data");

                //TODO define a rule to identify the Inverter to use for the specific axis to calibrate (Backlog Item 2651)
                var currentInverter = InverterIndex.MainInverter;

                if (!this.inverterStatuses.TryGetValue(currentInverter, out var inverterStatus))
                {
                    this.logger.LogTrace("3:Required Inverter Status not configured");

                    var errorNotification = new FieldNotificationMessage(positioningData,
                        "Requested Inverter is not configured",
                        FieldMessageActor.Any,
                        FieldMessageActor.InverterDriver,
                        FieldMessageType.Positioning,
                        MessageStatus.OperationError,
                        ErrorLevel.Critical);

                    this.eventAggregator?.GetEvent<FieldNotificationEvent>().Publish(errorNotification);
                }

                if (this.IsInverterStarted(inverterStatus))
                {
                    this.logger.LogTrace("4:Starting Positioning FSM");
                    var verticalPositioningData = new VerticalPositioningFieldMessageData(positioningData, 1, 1, 1, 1);

                    this.currentStateMachine = new VerticalPositioningStateMachine(verticalPositioningData, inverterStatus, this.inverterCommandQueue, this.eventAggregator, this.logger);
                    this.currentStateMachine?.Start();
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
                var errorNotification = new FieldNotificationMessage(receivedMessage.Data,
                    "Wrong message Data data type",
                    FieldMessageActor.Any,
                    FieldMessageActor.InverterDriver,
                    FieldMessageType.Positioning,
                    MessageStatus.OperationError,
                    ErrorLevel.Critical);

                this.eventAggregator?.GetEvent<FieldNotificationEvent>().Publish(errorNotification);
            }

            this.logger.LogDebug("7:Method End");
        }

        private void ProcessPowerOffMessage(FieldCommandMessage receivedMessage)
        {
            this.logger.LogDebug("1:Method Start");

            if (receivedMessage.Data is IInverterPowerOffFieldMessageData powerOffData)
            {
                this.logger.LogTrace("2:Parse Message Data");

                var currentInverter = ((InverterPowerOffFieldMessageData)receivedMessage.Data).InverterToPowerOff;
                if (!this.inverterStatuses.TryGetValue(currentInverter, out var inverterStatus))
                {
                    this.logger.LogTrace("3:Required Inverter Status not configured");

                    var errorNotification = new FieldNotificationMessage(powerOffData,
                        "Requested Inverter is not configured",
                        FieldMessageActor.Any,
                        FieldMessageActor.InverterDriver,
                        FieldMessageType.InverterPowerOff,
                        MessageStatus.OperationError,
                        ErrorLevel.Critical);

                    this.eventAggregator?.GetEvent<FieldNotificationEvent>().Publish(errorNotification);
                }

                if (this.IsInverterStarted(inverterStatus))
                {
                    this.logger.LogTrace("4:Starting Power Off FSM");
                    this.currentStateMachine = new PowerOffStateMachine(inverterStatus, this.inverterCommandQueue,
                        this.eventAggregator, this.logger);
                    this.currentStateMachine?.Start();
                }
                else
                {
                    this.logger.LogTrace("5:Inverter already powered off. Just notify operation completed");
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
                this.logger.LogTrace("6:Wrong message Data data type");
                var errorNotification = new FieldNotificationMessage(receivedMessage.Data,
                    "Wrong message Data data type",
                    FieldMessageActor.Any,
                    FieldMessageActor.InverterDriver,
                    FieldMessageType.InverterPowerOff,
                    MessageStatus.OperationError,
                    ErrorLevel.Critical);

                this.eventAggregator?.GetEvent<FieldNotificationEvent>().Publish(errorNotification);
            }

            this.logger.LogDebug("7:Method End");
        }

        private void ProcessPowerOnMessage(FieldCommandMessage receivedMessage)
        {
            this.logger.LogDebug("1:Method Start");

            if (receivedMessage.Data is IInverterPowerOnFieldMessageData powerOnData)
            {
                this.logger.LogTrace("2:Parse Message Data");

                var currentInverter = ((InverterPowerOnFieldMessageData)receivedMessage.Data).InverterToPowerOn;
                if (!this.inverterStatuses.TryGetValue(currentInverter, out var inverterStatus))
                {
                    this.logger.LogTrace("3:Required Inverter Status not configured");

                    var errorNotification = new FieldNotificationMessage(powerOnData,
                        "Requested Inverter is not configured",
                        FieldMessageActor.Any,
                        FieldMessageActor.InverterDriver,
                        FieldMessageType.InverterPowerOn,
                        MessageStatus.OperationError,
                        ErrorLevel.Critical);

                    this.eventAggregator?.GetEvent<FieldNotificationEvent>().Publish(errorNotification);
                }

                if (this.IsInverterStarted(inverterStatus))
                {
                    this.logger.LogTrace("4:Inverter already powered on. Just notify operation completed");
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
                    this.logger.LogTrace("5:Starting Power On FSM");
                    this.currentStateMachine = new PowerOnStateMachine(inverterStatus, this.inverterCommandQueue, this.eventAggregator, this.logger);
                    this.currentStateMachine?.Start();
                }
            }
            else
            {
                this.logger.LogTrace("6:Wrong message Data data type");
                var errorNotification = new FieldNotificationMessage(receivedMessage.Data,
                    "Wrong message Data data type",
                    FieldMessageActor.Any,
                    FieldMessageActor.InverterDriver,
                    FieldMessageType.InverterPowerOn,
                    MessageStatus.OperationError,
                    ErrorLevel.Critical);

                this.eventAggregator?.GetEvent<FieldNotificationEvent>().Publish(errorNotification);
            }

            this.logger.LogDebug("7:Method End");
        }

        private void ProcessStopMessage(FieldCommandMessage message)
        {
            this.logger.LogDebug("1:Method Start");

            if (message.Data is IInverterStopFieldMessageData stopData)
            {
                if (this.inverterStatuses.TryGetValue(stopData.InverterToStop, out var inverterStatus))
                {
                    this.currentStateMachine = new StopStateMachine(inverterStatus, this.inverterCommandQueue, this.eventAggregator, this.logger);
                    this.currentStateMachine?.Start();
                }
                else
                {
                    var errorNotification = new FieldNotificationMessage(message.Data,
                        $"Inverter status not configured for requested inverter {stopData.InverterToStop}",
                        FieldMessageActor.Any,
                        FieldMessageActor.InverterDriver,
                        FieldMessageType.InverterStop,
                        MessageStatus.OperationError,
                        ErrorLevel.Critical);

                    this.logger.LogTrace($"2:Inverter status not configured for requested inverter Type ={errorNotification.Type}:Destination={errorNotification.Destination}:Status={errorNotification.Status}");

                    this.eventAggregator?.GetEvent<FieldNotificationEvent>().Publish(errorNotification);
                }
            }
            else
            {
                var errorNotification = new FieldNotificationMessage(message.Data,
                    "Invalid message data for InverterStop message type",
                    FieldMessageActor.Any,
                    FieldMessageActor.InverterDriver,
                    FieldMessageType.InverterStop,
                    MessageStatus.OperationError,
                    ErrorLevel.Error);

                this.logger.LogTrace($"3:Invalid message data for InverterStop message Type={errorNotification.Type}:Destination={errorNotification.Destination}:Status={errorNotification.Status}");

                this.eventAggregator?.GetEvent<FieldNotificationEvent>().Publish(errorNotification);
            }

            this.logger.LogDebug("4:Method End");
        }

        private void RequestAxisPositionUpdate(object state)
        {
            this.logger.LogDebug("1:Method Start");

            var readAxisPositionMessage = new InverterMessage(InverterIndex.MainInverter, (short)InverterParameterId.ActualPositionShaft);

            this.logger.LogTrace($"2:ReadAxisPositionMessage={readAxisPositionMessage}");

            this.inverterCommandQueue.Enqueue(readAxisPositionMessage);

            this.logger.LogDebug("3:Method End");
        }

        private void RequestSensorStatusUpdate(object state)
        {
            this.logger.LogDebug("1:Method Start");

            var readSensorStatusMessage = new InverterMessage(InverterIndex.MainInverter, (short)InverterParameterId.StatusDigitalSignals);

            this.logger.LogTrace($"2:ReadSensorStatusMessage={readSensorStatusMessage}");

            this.inverterCommandQueue.Enqueue(readSensorStatusMessage);

            this.logger.LogDebug("3:Method End");
        }

        private void SendHeartBeat(object state)
        {
            if (!this.inverterStatuses.TryGetValue(InverterIndex.MainInverter, out var inverterStatus))
            {
                var errorNotification = new FieldNotificationMessage(null,
                    "Inverter status not configured for Main Inverter",
                    FieldMessageActor.Any,
                    FieldMessageActor.InverterDriver,
                    FieldMessageType.InverterException,
                    MessageStatus.OperationError,
                    ErrorLevel.Critical);

                this.logger.LogTrace($"2:Inverter status not configured for Main Inverter");

                this.eventAggregator?.GetEvent<FieldNotificationEvent>().Publish(errorNotification);
                return;
            }

            inverterStatus.CommonControlWord.HeartBeat = !inverterStatus.CommonControlWord.HeartBeat;

            this.heartbeatQueue.Enqueue(new InverterMessage(InverterIndex.MainInverter, (short)InverterParameterId.ControlWordParam, inverterStatus.CommonControlWord.Value));
        }

        private async Task StartHardwareCommunications()
        {
            this.logger.LogDebug("1:Method Start");

            var inverterAddress = await this.dataLayerConfigurationValueManagement.GetIPAddressConfigurationValueAsync((long)SetupNetwork.Inverter1, (long)ConfigurationCategory.SetupNetwork);
            var inverterPort = await this.dataLayerConfigurationValueManagement.GetIntegerConfigurationValueAsync((long)SetupNetwork.Inverter1Port, (long)ConfigurationCategory.SetupNetwork);

            this.socketTransport.Configure(inverterAddress, inverterPort);

            try
            {
                await this.socketTransport.ConnectAsync();
            }
            catch (Exception ex)
            {
                this.logger.LogCritical($"2:Exception {ex.Message} while Connecting Receiver Socket Transport");

                //TEMP throw new InverterDriverException($"Exception {ex.Message} while Connecting Receiver Socket Transport", ex);

                this.SendMessage(new ExceptionMessageData(ex, "", 0));
            }

            if (!this.socketTransport.IsConnected)
            {
                this.logger.LogCritical("3:Socket Transport failed to connect");

                throw new InverterDriverException("Socket Transport failed to connect");
            }

            try
            {
                this.inverterReceiveTask.Start();
                this.inverterSendTask.Start();
            }
            catch (Exception ex)
            {
                this.logger.LogCritical($"4:Exception: {ex.Message} while starting service threads");

                //TEMP throw new InverterDriverException($"Exception: {ex.Message} while starting service threads", ex);

                this.SendMessage(new ExceptionMessageData(ex, "", 0));
            }

            this.logger.LogDebug("6:Method End");
        }

        #endregion
    }
}
