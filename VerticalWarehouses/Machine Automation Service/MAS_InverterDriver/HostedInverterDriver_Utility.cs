using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Ferretto.VW.Common_Utils.Messages.Enumerations;
using Ferretto.VW.MAS_DataLayer.Enumerations;
using Ferretto.VW.MAS_InverterDriver.Enumerations;
using Ferretto.VW.MAS_InverterDriver.InverterStatus;
using Ferretto.VW.MAS_InverterDriver.InverterStatus.Interfaces;
using Ferretto.VW.MAS_InverterDriver.StateMachines.CalibrateAxis;
using Ferretto.VW.MAS_InverterDriver.StateMachines.PowerOff;
using Ferretto.VW.MAS_InverterDriver.StateMachines.PowerOn;
using Ferretto.VW.MAS_InverterDriver.StateMachines.Stop;
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
                    this.sensorStatusUpdateTimer.Change(updateData.SensorUpdateInterval, updateData.SensorUpdateInterval);
                }
            }
            else
            {
                this.sensorStatusUpdateTimer.Change(-1, Timeout.Infinite);
            }

            if (updateData.AxisPosition)
            {
                if (updateData.AxisUpdateInterval == 0)
                {
                    var readAxisPositionMessage = new InverterMessage(InverterIndex.MainInverter, (short)InverterParameterId.ActualPositionShaft);

                    this.logger.LogTrace($"3:ReadAxisPositionMessage={readAxisPositionMessage}");

                    this.inverterCommandQueue.Enqueue(readAxisPositionMessage);

                    this.forceStatusPublish = true;
                }
                else
                {
                    this.axisPositionUpdateTimer.Change(updateData.AxisUpdateInterval, updateData.AxisUpdateInterval);
                }
            }
            else
            {
                this.axisPositionUpdateTimer.Change(-1, Timeout.Infinite);
            }

            this.logger.LogDebug("3:Method End");
        }

        private void EvaluateReadMessage(InverterMessage currentMessage, InverterIndex inverterIndex)
        {
            this.logger.LogDebug("1:Method Start");

            this.logger.LogTrace($"2:currentMessage={currentMessage}");

            if (currentMessage.ParameterId == InverterParameterId.StatusWordParam)
            {
                if (!this.currentStateMachine.ValidateCommandResponse(currentMessage))
                {
                    var readStatusWordMessage = new InverterMessage(inverterIndex, (short)InverterParameterId.StatusWordParam);

                    this.logger.LogTrace($"4:readStatusWordMessage={readStatusWordMessage}");

                    this.inverterCommandQueue.Enqueue(readStatusWordMessage);
                }
            }

            if (currentMessage.ParameterId == InverterParameterId.StatusDigitalSignals)
            {
                this.logger.LogTrace($"5:StatusDigitalSignals.UShortPayload={currentMessage.UShortPayload}");

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
                this.logger.LogTrace($"6:ActualPositionShaft.UShortPayload={currentMessage.UShortPayload}");

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

            this.logger.LogDebug("7:Method End");
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
            var inverterList = await this.vertimagConfiguration.GetInstalledInverterListAsync();
            IInverterStatusBase inverterStatus = null;
            foreach (KeyValuePair<InverterIndex, InverterType> inverterType in inverterList)
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

            this.heartBeatTimer?.Dispose();

            try
            {
                this.heartBeatTimer = new Timer(this.SendHeartBeat, null, TimeSpan.Zero, TimeSpan.FromMilliseconds(HEARTBEAT_TIMEOUT));
                this.sensorStatusUpdateTimer?.Change(SENSOR_STATUS_UPDATE_INTERVAL, SENSOR_STATUS_UPDATE_INTERVAL);
            }
            catch (Exception ex)
            {
                this.logger.LogCritical($"5:Exception: {ex.Message} while starting sensor update timer");

                throw new InverterDriverException($"Exception: {ex.Message} while starting sensor update timer", ex);
            }
        }

        private void InitializeMethodSubscriptions()
        {
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
        }

        //TODO CHECK
        private bool IsInverterStarted(InverterIndex inverter)
        {
            if (!this.inverterStatuses.ContainsKey(inverter))
            {
                return false;
            }

            if (!this.inverterStatuses.TryGetValue(inverter, out var inverterStatus))
            {
                return false;
            }

            return inverterStatus.CommonStatusWord.IsReadyToSwitchOn &
                   inverterStatus.CommonStatusWord.IsSwitchedOn &
                   inverterStatus.CommonStatusWord.IsVoltageEnabled &
                   inverterStatus.CommonStatusWord.IsQuickStopActive;
        }

        //TODO CHECK
        private void ProcessCalibrateAxisMessage(FieldCommandMessage receivedMessage)
        {
            if (receivedMessage.Data is ICalibrateAxisFieldMessageData calibrateData)
            {
                this.logger.LogDebug("5:Object creation");

                //TODO define a rule to identify the Inverter to use for the specific axis to calibrate
                InverterIndex currentInverter = InverterIndex.MainInverter;

                if (!this.IsInverterStarted(currentInverter))
                {
                    if (!this.inverterStatuses.TryGetValue(currentInverter, out var inverterStatus))
                    {
                        var errorNotification = new FieldNotificationMessage(new CalibrateAxisFieldMessageData(calibrateData.AxisToCalibrate),
                            "Requested Inverter is not configured",
                            FieldMessageActor.Any,
                            FieldMessageActor.InverterDriver,
                            FieldMessageType.CalibrateAxis,
                            MessageStatus.OperationError,
                            ErrorLevel.Critical);

                        this.eventAggregator?.GetEvent<FieldNotificationEvent>().Publish(errorNotification);
                    }
                    else
                    {
                        lock (this.suspendedCommandMessage)
                        {
                            this.suspendedCommandMessage = receivedMessage;
                        }

                        this.currentStateMachine = new PowerOnStateMachine(
                            inverterStatus, this.inverterCommandQueue,
                            this.eventAggregator,
                            this.logger);
                        this.currentStateMachine?.Start();
                    }
                }
                else
                {
                    this.inverterStatuses.TryGetValue(currentInverter, out var inverterStatus);
                    this.currentAxis = calibrateData.AxisToCalibrate;
                    this.currentStateMachine = new CalibrateAxisStateMachine(this.currentAxis, inverterStatus, this.inverterCommandQueue, this.eventAggregator, this.logger);
                    this.currentStateMachine?.Start();
                }
            }
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

        //TODO CHECK
        private void ProcessInverterStatusUpdateMessage(FieldCommandMessage receivedMessage)
        {
            if (receivedMessage.Data is IInverterStatusUpdateFieldMessageData updateData)
            {
                this.ConfigureUpdates(updateData);
            }
        }

        //TODO CHECK
        private void ProcessPositioningMessage(FieldCommandMessage receivedMessage)
        {
            this.axisPositionUpdateTimer.Change(AXIS_POSITION_UPDATE_INTERVAL, AXIS_POSITION_UPDATE_INTERVAL);
        }

        //TODO CHECK
        private void ProcessPowerOffMessge(FieldCommandMessage receivedMessage)
        {
            if (receivedMessage.Data is IInverterPowerOffFieldMessageData powerOffData)
            {
                if (!this.IsInverterStarted(powerOffData.InverterToPowerOff))
                {
                    var endNotification = new FieldNotificationMessage(new InverterPowerOnFieldMessageData(powerOffData.InverterToPowerOff),
                        "Inverter Started",
                        FieldMessageActor.Any,
                        FieldMessageActor.InverterDriver,
                        FieldMessageType.InverterPowerOn,
                        MessageStatus.OperationEnd);

                    this.eventAggregator?.GetEvent<FieldNotificationEvent>().Publish(endNotification);
                }
                else
                {
                    if (!this.inverterStatuses.TryGetValue(powerOffData.InverterToPowerOff, out var inverterStatus))
                    {
                        var errorNotification = new FieldNotificationMessage(new InverterPowerOnFieldMessageData(powerOffData.InverterToPowerOff),
                            "Requested Inverter is not configured",
                            FieldMessageActor.Any,
                            FieldMessageActor.InverterDriver,
                            FieldMessageType.InverterPowerOn,
                            MessageStatus.OperationError,
                            ErrorLevel.Critical);

                        this.eventAggregator?.GetEvent<FieldNotificationEvent>().Publish(errorNotification);
                    }
                    else
                    {
                        this.currentStateMachine = new PowerOffStateMachine(
                            inverterStatus, this.inverterCommandQueue,
                            this.eventAggregator,
                            this.logger);
                        this.currentStateMachine?.Start();
                    }
                }
            }
        }

        //TODO CHECK
        private void ProcessPowerOnMessage(FieldCommandMessage receivedMessage)
        {
            if (receivedMessage.Data is IInverterPowerOnFieldMessageData startData)
            {
                if (this.IsInverterStarted(startData.InverterToPowerOn))
                {
                    var endNotification = new FieldNotificationMessage(new InverterPowerOnFieldMessageData(startData.InverterToPowerOn),
                        "Inverter Started",
                        FieldMessageActor.Any,
                        FieldMessageActor.InverterDriver,
                        FieldMessageType.InverterPowerOn,
                        MessageStatus.OperationEnd);

                    this.eventAggregator?.GetEvent<FieldNotificationEvent>().Publish(endNotification);
                }
                else
                {
                    if (!this.inverterStatuses.TryGetValue(startData.InverterToPowerOn, out var inverterStatus))
                    {
                        var errorNotification = new FieldNotificationMessage(new InverterPowerOnFieldMessageData(startData.InverterToPowerOn),
                            "Requested Inverter is not configured",
                            FieldMessageActor.Any,
                            FieldMessageActor.InverterDriver,
                            FieldMessageType.InverterPowerOn,
                            MessageStatus.OperationError,
                            ErrorLevel.Critical);

                        this.eventAggregator?.GetEvent<FieldNotificationEvent>().Publish(errorNotification);
                    }
                    else
                    {
                        this.currentStateMachine = new PowerOnStateMachine(
                            inverterStatus, this.inverterCommandQueue,
                            this.eventAggregator,
                            this.logger);
                        this.currentStateMachine?.Start();
                    }
                }
            }
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
                    var errorNotification = new FieldNotificationMessage(null,
                        $"Inverter status not configured for requested inverter {stopData.InverterToStop}",
                        FieldMessageActor.Any,
                        FieldMessageActor.InverterDriver,
                        message.Type,
                        MessageStatus.OperationError,
                        ErrorLevel.Critical);

                    this.logger.LogTrace($"2:Inverter status not configured for requested inverter Type ={errorNotification.Type}:Destination={errorNotification.Destination}:Status={errorNotification.Status}");

                    this.eventAggregator?.GetEvent<FieldNotificationEvent>().Publish(errorNotification);
                }
            }
            else
            {
                var errorNotification = new FieldNotificationMessage(null,
                    "Invalid message data for InverterStop message type",
                    FieldMessageActor.Any,
                    FieldMessageActor.InverterDriver,
                    message.Type,
                    MessageStatus.OperationError,
                    ErrorLevel.Error);

                this.logger.LogTrace($"2:Invalid message data for InverterStop message Type={errorNotification.Type}:Destination={errorNotification.Destination}:Status={errorNotification.Status}");

                this.eventAggregator?.GetEvent<FieldNotificationEvent>().Publish(errorNotification);
            }

            this.logger.LogDebug("3:Method End");
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

                throw new InverterDriverException($"Exception {ex.Message} while Connecting Receiver Socket Transport", ex);
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

                throw new InverterDriverException($"Exception: {ex.Message} while starting service threads", ex);
            }

            this.logger.LogDebug("6:Method End");
        }

        #endregion
    }
}
