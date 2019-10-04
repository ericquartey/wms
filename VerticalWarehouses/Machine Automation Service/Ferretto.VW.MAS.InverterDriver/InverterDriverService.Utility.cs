﻿using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS.DataModels.Enumerations;
using Ferretto.VW.MAS.InverterDriver.Contracts;
using Ferretto.VW.MAS.InverterDriver.InverterStatus;
using Ferretto.VW.MAS.InverterDriver.InverterStatus.Interfaces;
using Ferretto.VW.MAS.InverterDriver.StateMachines;
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
using Ferretto.VW.MAS.Utils.Messages.FieldInterfaces;
using Microsoft.Extensions.Logging;
using Prism.Events;

// ReSharper disable ArrangeThisQualifier
// ReSharper disable ParameterHidesMember
namespace Ferretto.VW.MAS.InverterDriver
{
    public partial class InverterDriverService
    {
        #region Fields

        private readonly bool refreshTargetTable = false;

        private readonly object syncAxisTimer = new object();

        private readonly object syncSensorTimer = new object();

        private readonly object syncStatusTimer = new object();

        private IInverterPositioningFieldMessageData dataOld;

        private IPAddress inverterAddress;

        private int inverterPort;

        #endregion

        #region Methods

        private void ConfigureTimer(IInverterSetTimerFieldMessageData updateData)
        {
            var inverterIndex = updateData.InverterIndex;
            switch (updateData.InverterTimer)
            {
                case InverterTimer.AxisPosition:
                    if (updateData.Enable)
                    {
                        if (updateData.UpdateInterval == 0)
                        {
                            var readAxisPositionMessage = new InverterMessage(inverterIndex, InverterParameterId.ActualPositionShaft);

                            this.logger.LogTrace($"1:ReadAxisPositionMessage={readAxisPositionMessage}");

                            this.inverterCommandQueue.Enqueue(readAxisPositionMessage);
                        }
                        else
                        {
                            this.logger.LogTrace("2:Change axis update interval");
                            this.axisPositionUpdateTimer[(int)inverterIndex].Change(updateData.UpdateInterval, updateData.UpdateInterval);
                        }
                        this.forceStatusPublish = true;
                    }
                    else
                    {
                        this.logger.LogTrace("3:Stop axis update timer");
                        this.axisPositionUpdateTimer[(int)inverterIndex].Change(Timeout.Infinite, Timeout.Infinite);
                    }
                    break;

                case InverterTimer.SensorStatus:
                    if (updateData.Enable)
                    {
                        if (updateData.UpdateInterval == 0)
                        {
                            var readSensorStatusMessage = new InverterMessage(InverterIndex.MainInverter, InverterParameterId.DigitalInputsOutputs);

                            this.logger.LogTrace($"1:ReadSensorStatusMessage={readSensorStatusMessage}");

                            this.inverterCommandQueue.Enqueue(readSensorStatusMessage);
                        }
                        else
                        {
                            this.logger.LogTrace("2:Change sensor update interval");
                            this.sensorStatusUpdateTimer.Change(updateData.UpdateInterval, updateData.UpdateInterval);
                        }
                        this.forceStatusPublish = true;
                    }
                    else
                    {
                        this.logger.LogTrace("3:Stop sensor update timer");
                        this.sensorStatusUpdateTimer.Change(-1, Timeout.Infinite);
                    }
                    break;

                case InverterTimer.StatusWord:
                    if (updateData.Enable)
                    {
                        if (updateData.UpdateInterval == 0)
                        {
                            var readStatusWordMessage = new InverterMessage(inverterIndex, InverterParameterId.StatusWordParam);

                            this.logger.LogTrace($"1:ReadStatusWordMessage={readStatusWordMessage}");

                            this.inverterCommandQueue.Enqueue(readStatusWordMessage);
                        }
                        else
                        {
                            this.logger.LogTrace("2:Change status word interval");
                            this.statusWordUpdateTimer[(int)inverterIndex].Change(updateData.UpdateInterval, updateData.UpdateInterval);
                        }
                        this.forceStatusPublish = true;
                    }
                    else
                    {
                        this.logger.LogTrace("3:Stop status word timer");
                        this.statusWordUpdateTimer[(int)inverterIndex].Change(-1, Timeout.Infinite);
                    }
                    break;

                default:
                    if (Debugger.IsAttached)
                    {
                        Debugger.Break();
                    }
                    break;
            }
        }

        private void EvaluateReadMessage(InverterMessage currentMessage, InverterIndex inverterIndex, IInverterStateMachine currentStateMachine)
        {
            this.logger.LogTrace($"1:currentMessage={currentMessage}");

            if (currentMessage.ParameterId == InverterParameterId.StatusWordParam)
            {
                var inverter = this.invertersProvider.GetByIndex(inverterIndex);

                if (inverter.CommonStatusWord.Value != currentMessage.UShortPayload)
                {
                    var msgNotification = new FieldNotificationMessage(
                        new InverterStatusWordFieldMessageData(currentMessage.UShortPayload),
                        "Inverter Status Word update",
                        FieldMessageActor.FiniteStateMachines,
                        FieldMessageActor.InverterDriver,
                        FieldMessageType.InverterStatusWord,
                        MessageStatus.OperationExecuting,
                        (byte)inverterIndex);

                    this.eventAggregator.GetEvent<FieldNotificationEvent>().Publish(msgNotification);
                }

                if (inverter != null)
                {
                    inverter.CommonStatusWord.Value = currentMessage.UShortPayload;
                }

                if (!currentStateMachine?.ValidateCommandResponse(currentMessage) ?? false)
                {
                    var readStatusWordMessage = new InverterMessage(inverterIndex, InverterParameterId.StatusWordParam);

                    this.logger.LogTrace($"2:readStatusWordMessage={readStatusWordMessage}");

                    this.inverterCommandQueue.Enqueue(readStatusWordMessage);
                }
                else
                {
                    this.logger.LogTrace("3:Validate Command Response True");
                }
            }
            else if (currentMessage.ParameterId == InverterParameterId.DigitalInputsOutputs)
            {
                this.sensorStopwatch.Stop();
                this.SensorTimeData.AddValue(this.sensorStopwatch.ElapsedTicks);

                this.logger.LogTrace($"4:StatusDigitalSignals.StringPayload={currentMessage.StringPayload}");

                foreach (var inverter in this.invertersProvider.GetAll())
                {
                    var ioStatuses = this.RetrieveInverterIOStatus(currentMessage.StringPayload, (int)inverter.SystemIndex);

                    switch (inverter)
                    {
                        case AngInverterStatus angInverter:

                            // INFO The Overrun elevator must be inverted (WORKAROUND)
                            ioStatuses[6] = !ioStatuses[6];

                            if (angInverter.UpdateInputsStates(ioStatuses) || this.forceStatusPublish)
                            {
                                this.logger.LogTrace("Sensor Update");

                                var msgNotification = new FieldNotificationMessage(
                                    new InverterStatusUpdateFieldMessageData(angInverter.Inputs),
                                    "Inverter Inputs update",
                                    FieldMessageActor.FiniteStateMachines,
                                    FieldMessageActor.InverterDriver,
                                    FieldMessageType.InverterStatusUpdate,
                                    MessageStatus.OperationExecuting,
                                    angInverter.SystemIndex);

                                this.eventAggregator.GetEvent<FieldNotificationEvent>().Publish(msgNotification);
                            }

                            break;

                        case AcuInverterStatus acuInverter:

                            if (acuInverter.UpdateInputsStates(ioStatuses) || this.forceStatusPublish)
                            {
                                var msgNotification = new FieldNotificationMessage(
                                    new InverterStatusUpdateFieldMessageData(acuInverter.Inputs),
                                    "Inverter Inputs update",
                                    FieldMessageActor.FiniteStateMachines,
                                    FieldMessageActor.InverterDriver,
                                    FieldMessageType.InverterStatusUpdate,
                                    MessageStatus.OperationExecuting,
                                    acuInverter.SystemIndex);

                                this.eventAggregator.GetEvent<FieldNotificationEvent>().Publish(msgNotification);
                            }

                            break;

                        case AglInverterStatus aglInverter:

                            if (aglInverter.UpdateInputsStates(ioStatuses) || this.forceStatusPublish)
                            {
                                var msgNotification = new FieldNotificationMessage(
                                    new InverterStatusUpdateFieldMessageData(aglInverter.Inputs),
                                    "Inverter Inputs update",
                                    FieldMessageActor.FiniteStateMachines,
                                    FieldMessageActor.InverterDriver,
                                    FieldMessageType.InverterStatusUpdate,
                                    MessageStatus.OperationExecuting,
                                    aglInverter.SystemIndex);

                                this.eventAggregator.GetEvent<FieldNotificationEvent>().Publish(msgNotification);
                            }

                            break;
                    }
                }
                this.forceStatusPublish = false;
            }
            else if (currentMessage.ParameterId == InverterParameterId.ActualPositionShaft)
            {
                this.axisStopwatch.Stop();
                this.AxisTimeData.AddValue(this.axisStopwatch.ElapsedTicks);

                this.logger.LogTrace($"5:ActualPositionShaft.UIntPayload={currentMessage.IntPayload}");

                var inverter = this.invertersProvider.GetByIndex(inverterIndex);

                if ((inverter is AngInverterStatus || inverter is AcuInverterStatus)
                    && inverter is IPositioningInverterStatus positioningInverter)
                {
                    var axis = (inverterIndex == InverterIndex.MainInverter && !inverter.CommonControlWord.HorizontalAxis)
                        ? Axis.Vertical
                        : Axis.Horizontal;

                    if ((axis == this.currentAxis || currentStateMachine == null) &&
                        (positioningInverter.UpdateInverterCurrentPosition(axis, currentMessage.IntPayload) || this.forceStatusPublish))
                    {
                        var axisOrientation = axis == Axis.Horizontal ? Orientation.Horizontal : Orientation.Vertical;

                        double currentAxisPosition = 0;
                        if (currentMessage.IntPayload != 0)
                        {
                            currentAxisPosition = this.elevatorDataProvider.ConvertPulsesToMillimeters(currentMessage.IntPayload, axisOrientation);
                        }

                        var offset = axis == Axis.Horizontal
                            ? this.elevatorDataProvider.GetHorizontalAxis().Offset
                            : this.elevatorDataProvider.GetVerticalAxis().Offset;
                        currentAxisPosition += offset;

                        var notificationData = new InverterStatusUpdateFieldMessageData(axis, inverter.Inputs, (int)currentAxisPosition /*currentMessage.IntPayload*/);
                        var msgNotification = new FieldNotificationMessage(
                            notificationData,
                            "Inverter encoder value update",
                            FieldMessageActor.FiniteStateMachines,
                            FieldMessageActor.InverterDriver,
                            FieldMessageType.InverterStatusUpdate,
                            MessageStatus.OperationExecuting,
                            (byte)inverterIndex);

                        this.eventAggregator.GetEvent<FieldNotificationEvent>().Publish(msgNotification);

                        this.forceStatusPublish = false;
                    }
                }
            }

            if (currentMessage.ParameterId == InverterParameterId.TorqueCurrent)
            {
                currentStateMachine?.ValidateCommandResponse(currentMessage);
            }
        }

        private void EvaluateWriteMessage(InverterMessage currentMessage, InverterIndex inverterIndex, IInverterStateMachine currentStateMachine)
        {
            this.logger.LogTrace($"1:currentMessage={currentMessage}");

            if (currentMessage.ParameterId == InverterParameterId.ControlWordParam
                &&
                inverterIndex == InverterIndex.MainInverter)
            {
                this.logger.LogTrace("2:Evaluate Control word");

                var mainInverter = this.invertersProvider.GetMainInverter();

                if (mainInverter.WaitingHeartbeatAck)
                {
                    mainInverter.WaitingHeartbeatAck = false;
                    this.logger.LogTrace("5:Reset Heartbeat flag");
                    return;
                }
            }

            if (currentStateMachine?.ValidateCommandMessage(currentMessage) ?? false)
            {
                this.logger.LogTrace("6:Request Status word");
                var readStatusWordMessage = new InverterMessage(inverterIndex, InverterParameterId.StatusWordParam);
                this.inverterCommandQueue.Enqueue(readStatusWordMessage);
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

        private void InitializeTimers()
        {
            try
            {
                //this.heartBeatTimer = new Timer(this.SendHeartBeat, null, TimeSpan.Zero, TimeSpan.FromMilliseconds(HEARTBEAT_TIMEOUT));
                this.sensorStatusUpdateTimer?.Change(SENSOR_STATUS_UPDATE_INTERVAL, SENSOR_STATUS_UPDATE_INTERVAL);
                //this.statusWordUpdateTimer?.Change(STATUS_WORD_UPDATE_INTERVAL, STATUS_WORD_UPDATE_INTERVAL);
            }
            catch (Exception ex)
            {
                this.logger.LogCritical($"3:Exception: {ex.Message} while starting heartBeat update timer");

                this.SendOperationErrorMessage(InverterIndex.None, new InverterExceptionFieldMessageData(ex, "Exception while starting heartBeat update timer", 0), FieldMessageType.InverterException);

                //TODO: try to re-create the timer and start once again
            }
        }

        private void ProcessCalibrateAxisMessage(FieldCommandMessage receivedMessage, IInverterStatusBase inverter)
        {
            if (receivedMessage.Data is ICalibrateAxisFieldMessageData calibrateData)
            {
                this.logger.LogTrace("1:Parse Message Data");

                if (inverter.IsStarted)
                {
                    this.logger.LogTrace("3:Starting Calibrate Axis FSM");

                    this.logger.LogDebug($"Starting Calibrate Axis {calibrateData.AxisToCalibrate}");

                    this.currentAxis = calibrateData.AxisToCalibrate;
                    var currentStateMachine = new CalibrateAxisStateMachine(
                        this.currentAxis,
                        calibrateData.CalibrationType,
                        inverter,
                        this.logger,
                        this.eventAggregator,
                        this.inverterCommandQueue,
                        this.serviceScopeFactory);

                    this.currentStateMachines.Add(inverter.SystemIndex, currentStateMachine);
                    currentStateMachine.Start();
                }
                else
                {
                    this.logger.LogDebug("4:Inverter is not ready. Powering up the inverter");

                    var currentStateMachine = new PowerOnStateMachine(
                        inverter,
                        this.logger,
                        this.eventAggregator,
                        this.inverterCommandQueue,
                        this.serviceScopeFactory,
                        receivedMessage);

                    this.currentStateMachines.Add(inverter.SystemIndex, currentStateMachine);
                    currentStateMachine.Start();
                }
            }
            else
            {
                this.logger.LogError("5:Wrong message Data data type");

                var ex = new Exception();
                this.SendOperationErrorMessage(inverter.SystemIndex, new InverterExceptionFieldMessageData(ex, "Wrong message Data data type", 0), FieldMessageType.CalibrateAxis);
            }
        }

        private void ProcessDisableMessage(IInverterStatusBase inverter)
        {
            try
            {
                var currentStateMachine = new SwitchOffStateMachine(
                   inverter,
                   this.logger,
                   this.eventAggregator,
                   this.inverterCommandQueue,
                   this.serviceScopeFactory);

                this.currentStateMachines.Add(inverter.SystemIndex, currentStateMachine);
                currentStateMachine.Start();
            }
            catch (Exception ex)
            {
                this.SendOperationErrorMessage(inverter.SystemIndex, new InverterExceptionFieldMessageData(ex, "Inverter status not configured for requested inverter Type", 0), FieldMessageType.InverterDisable);
            }
        }

        private void ProcessFaultResetMessage(IInverterStatusBase inverter)
        {
            this.logger.LogTrace("1:Method Start");

            var currentStateMachine = new ResetFaultStateMachine(
                inverter,
                inverter.SystemIndex,
                this.logger,
                this.eventAggregator,
                this.inverterCommandQueue,
                this.serviceScopeFactory);

            this.currentStateMachines.Add(inverter.SystemIndex, currentStateMachine);
            currentStateMachine.Start();
        }

        private async Task<bool> ProcessHeartbeat()
        {
            if (this.heartbeatQueue.Dequeue(out _))
            {
                try
                {
                    var inverter = this.invertersProvider.GetMainInverter();

                    var newMessage = new InverterMessage(
                        InverterIndex.MainInverter,
                        (short)InverterParameterId.ControlWordParam,
                        inverter.CommonControlWord.Value);

                    this.logger.LogTrace($"1:heartbeat inverterMessage={newMessage}");

                    this.roundTripStopwatch.Reset();
                    this.roundTripStopwatch.Start();

                    var heartbeat = newMessage.GetHeartbeatMessage(newMessage.HeartbeatValue);
                    return await this.socketTransport.WriteAsync(heartbeat, this.stoppingToken) == heartbeat.Length;
                }
                catch (InverterDriverException ex)
                {
                    this.logger.LogError(ex, $"Exception {ex.Message}, InverterExceptionCode={ex.InverterDriverExceptionCode}");
                }
                catch (Exception ex)
                {
                    this.logger.LogError(ex, "An exception was thrown.");
                }
            }
            else if (Debugger.IsAttached)
            {
                Debugger.Break();
            }

            return false;
        }

        private async Task<bool> ProcessInverterCommand()
        {
            var result = false;

            if (this.inverterCommandQueue.Peek(out var message))
            {
                this.logger.LogTrace($"1:ParameterId={message.ParameterId}:SendDelay{message.SendDelay}:Queue{this.inverterCommandQueue.Count}:inverterMessage={message}");

                var inverterMessagePacket = message.IsWriteMessage ? message.ToBytes() : message.GetReadMessage();

                this.roundTripStopwatch.Reset();
                this.roundTripStopwatch.Start();

                try
                {
                    result = await this.socketTransport.WriteAsync(inverterMessagePacket, message.SendDelay, this.stoppingToken) == inverterMessagePacket.Length;
                }
                catch (InverterDriverException ex)
                {
                    this.logger.LogError(ex, $"Exception {ex.Message}, InverterExceptionCode={ex.InverterDriverExceptionCode}");
                }

                if (result)
                {
                    this.inverterCommandQueue.Dequeue(out _);
                }
                else if (Debugger.IsAttached)
                {
                    Debugger.Break();
                }
            }
            else if (Debugger.IsAttached)
            {
                Debugger.Break();
            }

            return result;
        }

        private void ProcessInverterSetTimerMessage(FieldCommandMessage receivedMessage, IInverterStatusBase inverter)
        {
            this.logger.LogTrace("1:Method Start");
            if (receivedMessage.Data is IInverterSetTimerFieldMessageData updateData)
            {
                updateData.InverterIndex = inverter.SystemIndex;
                this.ConfigureTimer(updateData);
            }
            else
            {
                this.logger.LogError("2:Wrong message Data data type");

                var ex = new Exception();
                this.SendOperationErrorMessage(inverter.SystemIndex, new InverterExceptionFieldMessageData(ex, "Wrong message Data data type", 0), FieldMessageType.InverterStatusUpdate);
            }
        }

        private void ProcessInverterSwitchOffMessage(IInverterStatusBase inverter)
        {
            this.logger.LogTrace("1:Method Start");

            var currentStateMachine = new SwitchOffStateMachine(
                inverter,
                this.logger,
                this.eventAggregator,
                this.inverterCommandQueue,
                this.serviceScopeFactory);

            this.currentStateMachines.Add(inverter.SystemIndex, currentStateMachine);

            currentStateMachine.Start();
        }

        private void ProcessInverterSwitchOnMessage(FieldCommandMessage receivedMessage, IInverterStatusBase inverter)
        {
            this.logger.LogTrace("1:Method Start");

            var currentInverter = Enum.Parse<InverterIndex>(receivedMessage.DeviceIndex.ToString());

            if (receivedMessage.Data is IInverterSwitchOnFieldMessageData switchOnData)
            {
                if (inverter.CommonStatusWord.IsReadyToSwitchOn &
                    inverter.CommonStatusWord.IsVoltageEnabled &
                    inverter.CommonStatusWord.IsQuickStopTrue)
                {
                    if (inverter.CommonControlWord.HorizontalAxis == (switchOnData.AxisToSwitchOn == Axis.Horizontal))
                    {
                        if (inverter.CommonStatusWord.IsSwitchedOn)
                        {
                            this.logger.LogDebug($"Inverter Already active on selected axis {switchOnData.AxisToSwitchOn}");

                            var notificationMessageData = new InverterSwitchOnFieldMessageData(switchOnData.AxisToSwitchOn);
                            var notificationMessage = new FieldNotificationMessage(
                                notificationMessageData,
                                $"Inverter Switch On on axis {switchOnData.AxisToSwitchOn} End",
                                FieldMessageActor.FiniteStateMachines,
                                FieldMessageActor.InverterDriver,
                                FieldMessageType.InverterSwitchOn,
                                MessageStatus.OperationEnd,
                                (byte)currentInverter);

                            this.logger.LogTrace($"2:Type={notificationMessage.Type}:Destination={notificationMessage.Destination}:Status={notificationMessage.Status}");

                            this.eventAggregator.GetEvent<FieldNotificationEvent>().Publish(notificationMessage);
                        }
                        else
                        {
                            this.logger.LogDebug("3: Switch On the inverter state machine");
                            this.logger.LogDebug($"Inverter requires switching on selected axis {switchOnData.AxisToSwitchOn} inverter {inverter.SystemIndex}");

                            var currentStateMachine = new SwitchOnStateMachine(
                                switchOnData.AxisToSwitchOn,
                                inverter,
                                this.logger,
                                this.eventAggregator,
                                this.inverterCommandQueue,
                                this.serviceScopeFactory);

                            this.currentStateMachines.Add(currentInverter, currentStateMachine);
                            currentStateMachine.Start();
                        }
                    }
                    else
                    {
                        this.logger.LogDebug("4: Switch Off the inverter state machine");

                        inverter.CommonControlWord.HorizontalAxis = switchOnData.AxisToSwitchOn == Axis.Horizontal;

                        this.logger.LogDebug($"Inverter requires switching off axis {switchOnData.AxisToSwitchOn} inverter {inverter.SystemIndex}");

                        var currentStateMachine = new SwitchOffStateMachine(
                            inverter,
                            this.logger,
                            this.eventAggregator,
                            this.inverterCommandQueue,
                            this.serviceScopeFactory,
                            receivedMessage);

                        this.currentStateMachines.Add(currentInverter, currentStateMachine);
                        currentStateMachine.Start();
                    }
                }
                else
                {
                    inverter.CommonControlWord.HorizontalAxis = switchOnData.AxisToSwitchOn == Axis.Horizontal;

                    var currentStateMachine = new PowerOnStateMachine(
                        inverter,
                        this.logger,
                        this.eventAggregator,
                        this.inverterCommandQueue,
                        this.serviceScopeFactory,
                        receivedMessage);

                    this.currentStateMachines.Add(currentInverter, currentStateMachine);
                    currentStateMachine.Start();
                }
            }
            else
            {
                this.logger.LogError("3:Invalid message data for ProcessInverterSwitchOnMessage message Type");

                this.SendOperationErrorMessage(currentInverter, new InverterExceptionFieldMessageData(null, "Invalid message data for InverterStop message Type", 0), FieldMessageType.InverterSwitchOn);
            }
        }

        private void ProcessPositioningMessage(FieldCommandMessage receivedMessage, IInverterStatusBase inverter)
        {
            if (receivedMessage.Data is IPositioningFieldMessageData positioningData)
            {
                this.logger.LogTrace("1:Parse Message Data");

                if (inverter.IsStarted)
                {
                    this.currentAxis = positioningData.AxisMovement;

                    this.logger.LogTrace("4:Starting Positioning FSM");

                    try
                    {
                        var axisOrientation = positioningData.AxisMovement == Axis.Horizontal
                            ? Orientation.Horizontal
                            : Orientation.Vertical;

                        var currentPosition = 0;
                        if (inverter is AngInverterStatus angInverter)
                        {
                            currentPosition = this.currentAxis == Axis.Vertical
                                ? angInverter.CurrentPositionAxisVertical
                                : angInverter.CurrentPositionAxisHorizontal;
                        }
                        else if (inverter is AcuInverterStatus acuInverter)
                        {
                            currentPosition = acuInverter.CurrentPosition;
                        }

                        var position = positioningData.TargetPosition;
                        if (positioningData.MovementType == MovementType.Absolute)
                        {
                            var axis = positioningData.AxisMovement == Axis.Horizontal
                                ? this.elevatorDataProvider.GetHorizontalAxis()
                                : this.elevatorDataProvider.GetVerticalAxis();

                            position -= axis.Offset;

                            if (position < 0)
                            {
                                throw new Exception($"The requested target position ({positioningData.TargetPosition}) is below the axis lower bound ({axis.LowerBound}).");
                            }

                            if (axis.Orientation == Orientation.Vertical)
                            {
                                var beltDisplacement = this.elevatorDataProvider.ComputeDisplacement(positioningData.TargetPosition);
                                this.logger.LogInformation($"Belt elongation for height={positioningData.TargetPosition} is {beltDisplacement} [mm].");
                                position += beltDisplacement;
                            }
                        }

                        var targetPosition = this.elevatorDataProvider.ConvertMillimetersToPulses(position, axisOrientation);

                        var targetAcceleration = positioningData.TargetAcceleration
                            .Select(value => this.elevatorDataProvider.ConvertMillimetersToPulses(value, axisOrientation))
                            .ToArray();

                        var targetDeceleration = positioningData.TargetDeceleration
                            .Select(value => this.elevatorDataProvider.ConvertMillimetersToPulses(value, axisOrientation))
                            .ToArray();

                        var targetSpeed = positioningData.TargetSpeed
                            .Select(value => this.elevatorDataProvider.ConvertMillimetersToPulses(value, axisOrientation))
                            .ToArray();

                        var switchPosition = positioningData.SwitchPosition
                            .Select(value => this.elevatorDataProvider.ConvertMillimetersToPulses(value, axisOrientation))
                            .ToArray();

                        // TODO: what is '2' and '4'? Please replace with named constants.
                        var direction = (positioningData.Direction == HorizontalMovementDirection.Forwards) ? 2 : 4;

                        this.logger.LogDebug($"Direction: {positioningData.Direction}");
                        this.logger.LogDebug($"Position:");
                        for (var i = 0; i < positioningData.SwitchPosition.Length; i++)
                        {
                            this.logger.LogDebug($"{positioningData.SwitchPosition[i]} mm");
                        }

                        var positioningFieldData = new InverterPositioningFieldMessageData(
                            positioningData,
                            targetAcceleration,
                            targetDeceleration,
                            targetPosition,
                            targetSpeed,
                            switchPosition,
                            direction,
                            this.refreshTargetTable);

                        // TODO: why this comment? remove if not necessary!
                        //this.refreshTargetTable = false;

                        this.logger.LogTrace($"1:CurrentPositionAxis = {currentPosition}");
                        this.logger.LogTrace($"2:data.TargetPosition = {positioningFieldData.TargetPosition}");

                        this.logger.LogDebug($"Current axis: {this.currentAxis}; current position: {currentPosition}; target: {positioningData.TargetPosition} [impulses: {targetPosition}]; movement type: {positioningData.MovementType}");

                        switch (positioningData.MovementType)
                        {
                            case MovementType.Absolute:
                                if (currentPosition == positioningFieldData.TargetPosition)
                                {
                                    var msgNotification = new FieldNotificationMessage(
                                        null,
                                        "Axis already in position",
                                        FieldMessageActor.FiniteStateMachines,
                                        FieldMessageActor.InverterDriver,
                                        FieldMessageType.Positioning,
                                        MessageStatus.OperationEnd,
                                        inverter.SystemIndex);

                                    this.eventAggregator.GetEvent<FieldNotificationEvent>().Publish(msgNotification);
                                }
                                else
                                {
                                    this.axisPositionUpdateTimer[(int)inverter.SystemIndex]?.Change(AXIS_POSITION_UPDATE_INTERVAL, AXIS_POSITION_UPDATE_INTERVAL);
                                    var currentStateMachine = new PositioningStateMachine(
                                        positioningFieldData,
                                        inverter,
                                        this.logger,
                                        this.eventAggregator,
                                        this.inverterCommandQueue,
                                        this.serviceScopeFactory);

                                    this.currentStateMachines.Add(inverter.SystemIndex, currentStateMachine);
                                    currentStateMachine.Start();
                                }
                                break;

                            case MovementType.Relative:
                            case MovementType.TableTarget:
                                if (positioningFieldData.TargetPosition == 0)
                                {
                                    var msgNotification = new FieldNotificationMessage(
                                        null,
                                        "Axis already in position",
                                        FieldMessageActor.FiniteStateMachines,
                                        FieldMessageActor.InverterDriver,
                                        FieldMessageType.Positioning,
                                        MessageStatus.OperationEnd,
                                        inverter.SystemIndex);

                                    this.eventAggregator.GetEvent<FieldNotificationEvent>().Publish(msgNotification);
                                }
                                else
                                {
                                    this.axisPositionUpdateTimer[(int)inverter.SystemIndex]?.Change(AXIS_POSITION_UPDATE_INTERVAL, 500);
                                    if (positioningData.MovementType == MovementType.TableTarget)
                                    {
                                        var currentStateMachine = new PositioningTableStateMachine(
                                            positioningFieldData,
                                            this.dataOld,
                                            inverter,
                                            this.logger,
                                            this.eventAggregator,
                                            this.inverterCommandQueue,
                                            this.serviceScopeFactory);

                                        this.currentStateMachines.Add(inverter.SystemIndex, currentStateMachine);
                                        currentStateMachine.Start();
                                    }
                                    else
                                    {
                                        var currentStateMachine = new PositioningStateMachine(
                                            positioningFieldData,
                                            inverter,
                                            this.logger,
                                            this.eventAggregator,
                                            this.inverterCommandQueue,
                                            this.serviceScopeFactory);

                                        this.currentStateMachines.Add(inverter.SystemIndex, currentStateMachine);
                                        currentStateMachine.Start();
                                    }
                                }
                                break;
                        }
                    }
                    catch (Exception ex)
                    {
                        this.logger.LogError(ex, $"Exception {ex.Message}");
                        this.SendOperationErrorMessage(inverter.SystemIndex, new InverterExceptionFieldMessageData(ex, "Wrong message Data Values", 0), FieldMessageType.Positioning);
                    }
                }
                else
                {
                    this.logger.LogDebug("5:Inverter is not ready. Powering up the inverter");

                    var currentStateMachine = new PowerOnStateMachine(
                        inverter,
                        this.logger,
                        this.eventAggregator,
                        this.inverterCommandQueue,
                        this.serviceScopeFactory,
                        receivedMessage);
                    this.currentStateMachines.Add(inverter.SystemIndex, currentStateMachine);
                    currentStateMachine.Start();
                }
            }
            else
            {
                this.logger.LogError("6:Wrong message Data data type");

                var ex = new Exception();
                this.SendOperationErrorMessage(inverter.SystemIndex, new InverterExceptionFieldMessageData(ex, "Wrong message Data data type", 0), FieldMessageType.Positioning);
            }
        }

        private void ProcessPowerOffMessage(FieldCommandMessage receivedMessage, IInverterStatusBase inverter)
        {
            var currentInverter = Enum.Parse<InverterIndex>(receivedMessage.DeviceIndex.ToString());

            this.logger.LogTrace("1:Parse Message Data");

            if (inverter.IsStarted)
            {
                this.logger.LogTrace("3:Starting Power Off FSM");
                var currentStateMachine = new PowerOffStateMachine(
                    inverter,
                    this.logger,
                    this.eventAggregator,
                    this.inverterCommandQueue,
                    this.serviceScopeFactory);

                this.currentStateMachines.Add(currentInverter, currentStateMachine);
                currentStateMachine.Start();
            }
            else
            {
                this.logger.LogTrace("4:Inverter already powered off. Just notify operation completed");
                var endNotification = new FieldNotificationMessage(
                    new InverterPowerOnFieldMessageData(),
                    "Inverter Started",
                    FieldMessageActor.Any,
                    FieldMessageActor.InverterDriver,
                    FieldMessageType.InverterPowerOff,
                    MessageStatus.OperationEnd,
                    (byte)currentInverter);

                this.eventAggregator.GetEvent<FieldNotificationEvent>().Publish(endNotification);
            }
        }

        private void ProcessPowerOnMessage(FieldCommandMessage receivedMessage, IInverterStatusBase inverter)
        {
            var currentInverter = Enum.Parse<InverterIndex>(receivedMessage.DeviceIndex.ToString());

            this.logger.LogTrace("1:Parse Message Data");

            if (inverter.IsStarted)
            {
                this.logger.LogTrace("3:Inverter already powered on. Just notify operation completed");
                var endNotification = new FieldNotificationMessage(
                    new InverterPowerOnFieldMessageData(),
                    "Inverter Started",
                    FieldMessageActor.Any,
                    FieldMessageActor.InverterDriver,
                    FieldMessageType.InverterPowerOn,
                    MessageStatus.OperationEnd,
                    (byte)currentInverter);

                this.eventAggregator.GetEvent<FieldNotificationEvent>().Publish(endNotification);
            }
            else
            {
                this.logger.LogTrace("4:Starting Power On FSM");
                var currentStateMachine = new PowerOnStateMachine(
                    inverter,
                    this.logger,
                    this.eventAggregator,
                    this.inverterCommandQueue,
                    this.serviceScopeFactory);

                this.currentStateMachines.Add(currentInverter, currentStateMachine);
                currentStateMachine.Start();
            }
        }

        private void ProcessShutterPositioningMessage(FieldCommandMessage receivedMessage, IInverterStatusBase inverter)
        {
            var currentInverter = Enum.Parse<InverterIndex>(receivedMessage.DeviceIndex.ToString());

            if (receivedMessage.Data is IShutterPositioningFieldMessageData shutterPositioningData)
            {
                this.logger.LogTrace("1:Parse Message Data");

                if (inverter.IsStarted)
                {
                    this.logger.LogTrace("4:Starting ShutterPositioning FSM");

                    var convertedShutterPositioningData = new InverterShutterPositioningFieldMessageData(shutterPositioningData);
                    var currentStateMachine = new ShutterPositioningStateMachine(
                        convertedShutterPositioningData,
                        inverter,
                        this.logger,
                        this.eventAggregator,
                        this.inverterCommandQueue,
                        this.serviceScopeFactory);

                    this.currentStateMachines.Add(currentInverter, currentStateMachine);
                    currentStateMachine.Start();
                }
                else
                {
                    this.logger.LogDebug("5:Inverter is not ready. Powering up the inverter");

                    var currentStateMachine = new PowerOnStateMachine(
                        inverter,
                        this.logger,
                        this.eventAggregator,
                        this.inverterCommandQueue,
                        this.serviceScopeFactory,
                        receivedMessage);
                    this.currentStateMachines.Add(currentInverter, currentStateMachine);
                    currentStateMachine.Start();
                }
            }
            else
            {
                this.logger.LogError("5:Wrong message Data data type");

                var ex = new Exception();
                this.SendOperationErrorMessage(currentInverter, new InverterExceptionFieldMessageData(ex, "Wrong message Data data type", 0), FieldMessageType.ShutterPositioning);
            }
        }

        private void ProcessStopMessage(IInverterStatusBase inverter)
        {
            this.logger.LogTrace("1:Method Start");

            var currentStateMachine = new StopStateMachine(
                inverter,
                this.logger,
                this.eventAggregator,
                this.inverterCommandQueue,
                this.serviceScopeFactory);

            this.currentStateMachines.Add(inverter.SystemIndex, currentStateMachine);
            currentStateMachine.Start();
        }

        private void RequestAxisPositionUpdate(object state)
        {
            lock (this.syncAxisTimer)
            {
                var inverterIndex = (InverterIndex)state;
                var readAxisPositionMessage = new InverterMessage(inverterIndex, InverterParameterId.ActualPositionShaft);

                this.logger.LogTrace($"1:ReadAxisPositionMessage={readAxisPositionMessage}");

                this.axisIntervalStopwatch.Stop();
                this.AxisIntervalTimeData.AddValue(this.axisIntervalStopwatch.ElapsedTicks);
                this.axisIntervalStopwatch.Reset();
                this.axisIntervalStopwatch.Start();

                this.axisStopwatch.Reset();
                this.axisStopwatch.Start();
                if (this.inverterCommandQueue.Count(x => x.ParameterId == InverterParameterId.ActualPositionShaft && x.SystemIndex == (byte)inverterIndex) < 2)
                {
                    this.inverterCommandQueue.Enqueue(readAxisPositionMessage);
                }
            }
        }

        private void RequestSensorStatusUpdate(object state)
        {
            lock (this.syncSensorTimer)
            {
                var readSensorStatusMessage = new InverterMessage(InverterIndex.MainInverter, InverterParameterId.DigitalInputsOutputs);

                this.sensorIntervalStopwatch.Stop();
                this.SensorIntervalTimeData.AddValue(this.sensorIntervalStopwatch.ElapsedTicks);
                this.sensorIntervalStopwatch.Reset();
                this.sensorIntervalStopwatch.Start();

                this.sensorStopwatch.Reset();
                this.sensorStopwatch.Start();

                if (this.inverterCommandQueue.Count(x => x.ParameterId == InverterParameterId.DigitalInputsOutputs) < 2)
                {
                    this.logger.LogTrace($"1:ReadSensorStatusTimer={readSensorStatusMessage} Count:{this.inverterCommandQueue.Count(x => x.ParameterId == InverterParameterId.DigitalInputsOutputs)}");

                    this.inverterCommandQueue.Enqueue(readSensorStatusMessage);
                }
            }
        }

        // poll the inverter to have the Fault status
        private void RequestStatusWordMessage(object state)
        {
            lock (this.syncStatusTimer)
            {
                var inverterIndex = (InverterIndex)state;
                if (this.inverterCommandQueue.Count(x => x.ParameterId == InverterParameterId.StatusWordParam && x.SystemIndex == (byte)inverterIndex) < 2)
                {
                    var readStatusWordMessage = new InverterMessage(inverterIndex, InverterParameterId.StatusWordParam);

                    this.logger.LogTrace($"1:readStatusWordTimer={readStatusWordMessage}");

                    this.inverterCommandQueue.Enqueue(readStatusWordMessage);
                }
            }
        }

        private bool[] RetrieveInverterIOStatus(string currentMessageStringPayload, int inverterIndex)
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

                var encodedWord = encodedValues[inverterIndex / 2];

                if (!encodedWord.Equals("\0"))
                {
                    if (short.TryParse(encodedWord, out short values))
                    {
                        var dataByte = inverterIndex % 2;

                        for (var index = 8 * dataByte; index < 8 + (8 * dataByte); index++)
                        {
                            returnValue[index - (8 * dataByte)] = (values & (0x0001 << index)) > 0;
                        }
                    }
                    else
                    {
                        this.logger.LogError(currentMessageStringPayload);
                    }
                }
            }

            return returnValue;
        }

        private void SendHeartBeat(object state)
        {
            if (this.socketTransport.IsConnected)
            {
                var mainInverter = this.invertersProvider.GetMainInverter();

                mainInverter.CommonControlWord.HeartBeat = !mainInverter.CommonControlWord.HeartBeat;
                mainInverter.WaitingHeartbeatAck = true;

                var message = new InverterMessage(
                    InverterIndex.MainInverter,
                    (short)InverterParameterId.ControlWordParam,
                    mainInverter.CommonControlWord.Value);

                this.heartbeatQueue.Enqueue(message);
            }
        }

        private async Task StartHardwareCommunications()
        {
            this.logger.LogTrace("1:Method Start");

            var masterInverter = this.digitalDevicesDataProvider.GetInverterByIndex(InverterIndex.MainInverter);

            this.inverterAddress = masterInverter.IpAddress;
            this.inverterPort = masterInverter.TcpPort;

            this.socketTransport.Configure(this.inverterAddress, this.inverterPort);
            this.logger.LogInformation($"1:Configure Inverter {masterInverter.Index}, tcp-endpoint={this.inverterAddress}:{this.inverterPort}");

            try
            {
                await this.socketTransport.ConnectAsync();
            }
            catch (InverterDriverException ex)
            {
                this.logger.LogError($"1A: Exception {ex.Message}; Exception code={ex.InverterDriverExceptionCode}; Inner exception: {ex.InnerException.Message}");
            }
            catch (Exception ex)
            {
                this.logger.LogCritical($"2:Exception {ex.Message} while Connecting Receiver Socket Transport");

                this.SendOperationErrorMessage(InverterIndex.MainInverter, new InverterExceptionFieldMessageData(ex, "while Connecting Receiver Socket Transport", 0), FieldMessageType.InverterException);
                throw new InverterDriverException($"Exception {ex.Message} StartHardwareCommunications Failed 1", ex);
            }

            if (!this.socketTransport.IsConnected)
            {
                this.logger.LogError("3:Socket Transport failed to connect");

                var ex = new Exception();
                this.SendOperationErrorMessage(InverterIndex.MainInverter, new InverterExceptionFieldMessageData(ex, "Socket Transport failed to connect", 0), FieldMessageType.InverterError);
            }
            else
            {
                this.logger.LogInformation($"3:Connection OK ipAddress={this.inverterAddress}:Port={this.inverterPort}");
            }

            try
            {
                this.inverterReceiveTask.Start();
                this.inverterSendTask.Start();
            }
            catch (Exception ex)
            {
                this.logger.LogCritical($"4:Exception: {ex.Message} while starting service threads");

                this.SendOperationErrorMessage(InverterIndex.MainInverter, new InverterExceptionFieldMessageData(ex, "while starting service threads", 0), FieldMessageType.InverterException);
                throw new InverterDriverException($"Exception {ex.Message} StartHardwareCommunications Failed 2", ex);
            }
        }

        #endregion
    }
}
