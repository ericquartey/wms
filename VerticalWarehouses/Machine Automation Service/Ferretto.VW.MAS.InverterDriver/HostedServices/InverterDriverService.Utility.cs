using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataLayer;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS.InverterDriver.Contracts;
using Ferretto.VW.MAS.InverterDriver.Diagnostics;
using Ferretto.VW.MAS.InverterDriver.InverterStatus;
using Ferretto.VW.MAS.InverterDriver.InverterStatus.Interfaces;
using Ferretto.VW.MAS.InverterDriver.StateMachines;
using Ferretto.VW.MAS.InverterDriver.StateMachines.CalibrateAxis;
using Ferretto.VW.MAS.InverterDriver.StateMachines.InverterProgramming;
using Ferretto.VW.MAS.InverterDriver.StateMachines.InverterReading;
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
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Ferretto.VW.MAS.InverterDriver
{
    partial class InverterDriverService
    {
        #region Fields

        private readonly object syncAxisTimer = new object();

        private readonly object syncSensorTimer = new object();

        private readonly object syncStatusTimer = new object();

        private IInverterPositioningFieldMessageData dataOld;

        //private Timer heartBeatTimer;

        private IPAddress inverterAddress;

        private int inverterPort;

        private bool isHeartBeatOn;

        private bool refreshTargetTable = false;

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

                            this.Logger.LogTrace($"1:ReadAxisPositionMessage={readAxisPositionMessage}");

                            this.inverterCommandQueue.Enqueue(readAxisPositionMessage);
                        }
                        else
                        {
                            this.Logger.LogTrace("2:Change axis update interval");
                            this.axisPositionUpdateTimer[(int)inverterIndex].Change(updateData.UpdateInterval, updateData.UpdateInterval);
                        }
                        this.forceStatusPublish[(int)inverterIndex] = true;
                    }
                    else
                    {
                        this.Logger.LogTrace("3:Stop axis update timer");
                        this.axisPositionUpdateTimer[(int)inverterIndex].Change(Timeout.Infinite, Timeout.Infinite);
                    }
                    break;

                case InverterTimer.SensorStatus:
                    if (updateData.Enable)
                    {
                        if (updateData.UpdateInterval == 0)
                        {
                            var readSensorStatusMessage = new InverterMessage(InverterIndex.MainInverter, InverterParameterId.DigitalInputsOutputs);

                            this.Logger.LogTrace($"1:ReadSensorStatusMessage={readSensorStatusMessage}");

                            this.inverterCommandQueue.Enqueue(readSensorStatusMessage);
                        }
                        else
                        {
                            this.Logger.LogTrace("2:Change sensor update interval");
                            this.sensorStatusUpdateTimer.Change(updateData.UpdateInterval, updateData.UpdateInterval);
                        }
                        this.forceStatusPublish[(int)inverterIndex] = true;
                    }
                    else
                    {
                        this.Logger.LogTrace("3:Stop sensor update timer");
                        this.sensorStatusUpdateTimer.Change(-1, Timeout.Infinite);
                    }
                    break;

                case InverterTimer.StatusWord:
                    if (updateData.Enable)
                    {
                        if (updateData.UpdateInterval == 0)
                        {
                            if (this.inverterCommandQueue.Count(x => x.ParameterId == InverterParameterId.StatusWord && x.SystemIndex == inverterIndex) < 2)
                            {
                                var readStatusWordMessage = new InverterMessage(inverterIndex, InverterParameterId.StatusWord);

                                this.Logger.LogTrace($"1:ReadStatusWordMessage={readStatusWordMessage}");

                                this.inverterCommandQueue.Enqueue(readStatusWordMessage);
                            }
                        }
                        else
                        {
                            this.Logger.LogTrace("2:Change status word interval");
                            this.statusWordUpdateTimer[(int)inverterIndex].Change(updateData.UpdateInterval, updateData.UpdateInterval);
                        }
                        this.forceStatusPublish[(int)inverterIndex] = true;
                    }
                    else
                    {
                        this.Logger.LogTrace("3:Stop status word timer");
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

        private void EvaluateReadMessage(
            InverterMessage message,
            IInverterStateMachine currentStateMachine,
            IServiceProvider serviceProvider)
        {
            this.Logger.LogTrace($"1:currentMessage={message}");

            var invertersProvider = serviceProvider.GetRequiredService<IInvertersProvider>();
            //this.Logger.LogTrace($"1a:currentMessage={message}");

            if (message.ParameterId == InverterParameterId.StatusWord)
            {
                var inverter = invertersProvider.GetByIndex(message.SystemIndex);

                if (inverter.CommonStatusWord.Value != message.UShortPayload)
                {
                    var msgNotification = new FieldNotificationMessage(
                        new InverterStatusWordFieldMessageData(message.UShortPayload),
                        "Inverter Status Word update",
                        FieldMessageActor.DeviceManager,
                        FieldMessageActor.InverterDriver,
                        FieldMessageType.InverterStatusWord,
                        MessageStatus.OperationExecuting,
                        (byte)message.SystemIndex);

                    this.eventAggregator.GetEvent<FieldNotificationEvent>().Publish(msgNotification);
                }

                if (inverter != null)
                {
                    inverter.CommonStatusWord.Value = message.UShortPayload;
                }

                //if (!currentStateMachine?.ValidateCommandResponse(message) ?? false)
                //{
                //    if (this.inverterCommandQueue.Count(x => x.ParameterId == InverterParameterId.StatusWord && x.SystemIndex == message.SystemIndex) <= 2)
                //    {
                //        var readStatusWordMessage = new InverterMessage(message.SystemIndex, InverterParameterId.StatusWord);

                //        this.Logger.LogTrace($"2:readStatusWordMessage={readStatusWordMessage}");

                //        this.inverterCommandQueue.Enqueue(readStatusWordMessage);
                //    }
                //}
                //else
                //{
                //    this.Logger.LogTrace("3:Validate Command Response True");
                //}
            }
            else if (message.ParameterId == InverterParameterId.DigitalInputsOutputs)
            {
                this.Logger.LogTrace($"4:StatusDigitalSignals.StringPayload={message.StringPayload}");

                foreach (var inverter in invertersProvider.GetAll())
                {
                    //this.Logger.LogTrace($"4.1");
                    var ioStatuses = this.RetrieveInverterIOStatus(message.StringPayload, (int)inverter.SystemIndex);

                    switch (inverter)
                    {
                        case AngInverterStatus angInverter:
                            //this.Logger.LogTrace($"4.2");

                            // INFO The Overrun elevator must be inverted (WORKAROUND)
                            ioStatuses[5] = !ioStatuses[5];

                            if (serviceProvider.GetRequiredService<IHostingEnvironment>().IsEnvironment("Bender"))
                            {
                                ioStatuses[7] = angInverter.ANG_ZeroCradleSensor;
                            }
                            if (angInverter.UpdateInputsStates(ioStatuses) || this.forceStatusPublish[(int)inverter.SystemIndex])
                            {
                                this.Logger.LogTrace("Ang Sensor Update");

                                var msgNotification = new FieldNotificationMessage(
                                    new InverterStatusUpdateFieldMessageData(angInverter.Inputs),
                                    "Inverter Inputs update",
                                    FieldMessageActor.DeviceManager,
                                    FieldMessageActor.InverterDriver,
                                    FieldMessageType.InverterStatusUpdate,
                                    MessageStatus.OperationExecuting,
                                    angInverter.SystemIndex);

                                this.eventAggregator.GetEvent<FieldNotificationEvent>().Publish(msgNotification);
                            }

                            break;

                        case AcuInverterStatus acuInverter:
                            //this.Logger.LogTrace($"4.3");

                            var baysDataProvider = serviceProvider.GetRequiredService<IBaysDataProvider>();
                            var bayNumber = baysDataProvider.GetByInverterIndex(inverter.SystemIndex);
                            if (baysDataProvider.GetByNumber(bayNumber).IsExternal)
                            {
                                // INFO The ACU_ZeroSensor must be inverted (WORKAROUND)
                                ioStatuses[2] = !ioStatuses[2];
                                ioStatuses[5] = !ioStatuses[5];
                            }

                            if (acuInverter.UpdateInputsStates(ioStatuses) || this.forceStatusPublish[(int)inverter.SystemIndex])
                            {
                                this.Logger.LogTrace("Acu Sensor Update");

                                var msgNotification = new FieldNotificationMessage(
                                    new InverterStatusUpdateFieldMessageData(acuInverter.Inputs),
                                    "Inverter Inputs update",
                                    FieldMessageActor.DeviceManager,
                                    FieldMessageActor.InverterDriver,
                                    FieldMessageType.InverterStatusUpdate,
                                    MessageStatus.OperationExecuting,
                                    acuInverter.SystemIndex);

                                this.eventAggregator.GetEvent<FieldNotificationEvent>().Publish(msgNotification);
                            }
                            this.currentStateMachines.TryGetValue(acuInverter.SystemIndex, out var acuStateMachine);
                            acuStateMachine?.ValidateCommandResponse(message);
                            break;

                        case AglInverterStatus aglInverter:
                            //this.Logger.LogTrace($"4.4");

                            if (aglInverter.UpdateInputsStates(ioStatuses) || this.forceStatusPublish[(int)inverter.SystemIndex])
                            {
                                this.Logger.LogTrace("Agl Sensor Update");
                                var msgNotification = new FieldNotificationMessage(
                                    new InverterStatusUpdateFieldMessageData(aglInverter.Inputs),
                                    "Inverter Inputs update",
                                    FieldMessageActor.DeviceManager,
                                    FieldMessageActor.InverterDriver,
                                    FieldMessageType.InverterStatusUpdate,
                                    MessageStatus.OperationExecuting,
                                    aglInverter.SystemIndex);

                                this.eventAggregator.GetEvent<FieldNotificationEvent>().Publish(msgNotification);
                            }

                            break;
                    }
                    //this.Logger.LogTrace($"4.5");
                    this.forceStatusPublish[(int)inverter.SystemIndex] = false;
                }
            }
            else if (message.ParameterId == InverterParameterId.ActualPositionShaft)
            {
                this.Logger.LogTrace($"5:ActualPositionShaft.UIntPayload={message.IntPayload}");

                var inverter = invertersProvider.GetByIndex(message.SystemIndex);

                var elevatorDataProvider = serviceProvider.GetRequiredService<IElevatorDataProvider>();

                if ((inverter is AngInverterStatus || inverter is AcuInverterStatus)
                    && inverter is IPositioningInverterStatus positioningInverter)
                {
                    var axis = (message.SystemIndex == InverterIndex.MainInverter && !inverter.CommonControlWord.HorizontalAxis)
                        ? Axis.Vertical
                        : Axis.Horizontal;

                    if (message.SystemIndex > InverterIndex.Slave1)
                    {
                        axis = Axis.BayChain;
                    }

                    this.Logger.LogTrace($"5a:ActualPositionShaft inverter={inverter.SystemIndex}; axis={axis}; value={message.IntPayload}; current={this.currentAxis}");

                    if (positioningInverter.UpdateInverterCurrentPosition(axis, message.IntPayload) || this.forceStatusPublish[(int)message.SystemIndex])
                    {
                        var axisOrientation = (axis == Axis.Horizontal || axis == Axis.BayChain) ? Orientation.Horizontal : Orientation.Vertical;

                        double currentAxisPosition = 0;
                        if (message.IntPayload != 0)
                        {
                            if (axis == Axis.BayChain)
                            {
                                currentAxisPosition = invertersProvider.ConvertPulsesToMillimeters(message.IntPayload, inverter);
                            }
                            else
                            {
                                currentAxisPosition = invertersProvider.ConvertPulsesToMillimeters(message.IntPayload, axisOrientation);
                            }
                        }

                        var offset = (axis == Axis.Vertical)
                            ? elevatorDataProvider.GetAxis(Orientation.Vertical).Offset
                            : elevatorDataProvider.GetAxis(Orientation.Horizontal).Offset;
                        currentAxisPosition += offset;
                        this.Logger.LogTrace($"5b:ActualPositionShaft inverter={inverter.SystemIndex}; axis={axis}; currentAxisPosition={currentAxisPosition}");

                        var notificationData = new InverterStatusUpdateFieldMessageData(axis, inverter.Inputs, currentAxisPosition);
                        var msgNotification = new FieldNotificationMessage(
                            notificationData,
                            "Inverter encoder value update",
                            FieldMessageActor.DeviceManager,
                            FieldMessageActor.InverterDriver,
                            FieldMessageType.InverterStatusUpdate,
                            MessageStatus.OperationExecuting,
                            (byte)message.SystemIndex);

                        this.eventAggregator.GetEvent<FieldNotificationEvent>().Publish(msgNotification);

                        this.forceStatusPublish[(int)message.SystemIndex] = false;
                    }
                }
            }
            else if (message.ParameterId == InverterParameterId.ProfileInput)
            {
                var notificationData = new MeasureProfileFieldMessageData(profile: message.UShortPayload);
                var msgNotification = new FieldNotificationMessage(
                    notificationData,
                    "Inverter measure profile",
                    FieldMessageActor.DeviceManager,
                    FieldMessageActor.InverterDriver,
                    FieldMessageType.MeasureProfile,
                    MessageStatus.OperationEnd,
                    (byte)message.SystemIndex);

                this.eventAggregator.GetEvent<FieldNotificationEvent>().Publish(msgNotification);

                this.Logger.LogDebug($"ProfileInput inverter={message.SystemIndex}; value={message.UShortPayload}");
            }
            else if (message.ParameterId == InverterParameterId.CurrentError)
            {
                var error = message.UShortPayload;
                this.Logger.LogDebug($"Inverter Fault: 0x{error:X4}; inverter={message.SystemIndex}; {InverterFaultCodes.GetErrorByCode(error)}");
                if (error > 0
                    && error != 0x1454
                    )
                {
                    using (var scope = this.ServiceScopeFactory.CreateScope())
                    {
                        // Retrieve the bay number related to the inverter index
                        var baysProvider = serviceProvider.GetRequiredService<IBaysDataProvider>();
                        var bayNumber = baysProvider.GetByInverterIndex(message.SystemIndex);

                        // Adds error related to the InverterFaultDetected
                        var errorsProvider = scope.ServiceProvider.GetRequiredService<IErrorsProvider>();
                        var idx = (int)message.SystemIndex + 1; // it has the systemIndex on base 1
                        errorsProvider.RecordNew(idx, error, bayNumber, InverterFaultCodes.GetErrorByCode(error));
                    }
                }
            }
            currentStateMachine?.ValidateCommandResponse(message);
        }

        private void EvaluateWriteMessage(
            InverterMessage message,
            IInverterStateMachine currentStateMachine,
            IServiceProvider serviceProvider)
        {
            this.Logger.LogTrace($"1:currentMessage={message}");

            //if (message.ParameterId == InverterParameterId.ControlWord
            //    &&
            //    message.SystemIndex == InverterIndex.MainInverter)
            //{
            //    this.Logger.LogTrace("2:Evaluate Control word");

            //    var mainInverter = serviceProvider.GetRequiredService<IInvertersProvider>().GetMainInverter();
            //}

            if ((currentStateMachine?.ValidateCommandMessage(message) ?? false)
                && this.inverterCommandQueue.Count(x => x.ParameterId == InverterParameterId.StatusWord && x.SystemIndex == message.SystemIndex) < 1
                )
            {
                this.Logger.LogTrace("6:Request Status word");
                var readStatusWordMessage = new InverterMessage(message.SystemIndex, InverterParameterId.StatusWord);
                this.inverterCommandQueue.Enqueue(readStatusWordMessage);
            }
        }

        private async Task GenerateHeartBeatAsync()
        {
            try
            {
                do
                {
                    this.RequestHeartBeat(InverterIndex.MainInverter);
                    Thread.Sleep(HEARTBEAT_TIMEOUT);
                }
                while (!this.CancellationToken.IsCancellationRequested);
            }
            catch (Exception ex) when (ex is OperationCanceledException || ex is ThreadAbortException)
            {
                this.Logger.LogTrace("Stopping heartBeat generation.");
            }
            catch (Exception ex)
            {
                this.Logger.LogCritical($"Exception: {ex.Message}");
            }
        }

        private void InitializeTimers(IServiceProvider serviceProvider)
        {
            try
            {
                //this.heartBeatTimer = new Timer(this.RequestHeartBeat, InverterIndex.MainInverter, TimeSpan.Zero, TimeSpan.FromMilliseconds(HEARTBEAT_TIMEOUT));
                var machine = serviceProvider.GetRequiredService<IMachineProvider>();
                if (machine.IsHeartBeat())
                {
                    Task.Run(this.GenerateHeartBeatAsync);
                }
                this.sensorStatusUpdateTimer?.Change(SENSOR_STATUS_UPDATE_INTERVAL, SENSOR_STATUS_UPDATE_INTERVAL);
                //this.statusWordUpdateTimer?.Change(STATUS_WORD_UPDATE_INTERVAL, STATUS_WORD_UPDATE_INTERVAL);
            }
            catch (Exception ex)
            {
                this.Logger.LogCritical($"3:Exception: {ex.Message} while starting heartBeat update timer");

                this.SendOperationErrorMessage(InverterIndex.None, new InverterExceptionFieldMessageData(ex, "Exception while starting heartBeat update timer", 0), FieldMessageType.InverterException);

                //TODO: try to re-create the timer and start once again
            }
        }

        private void ProcessCalibrateAxisMessage(FieldCommandMessage receivedMessage, IInverterStatusBase inverter)
        {
            if (receivedMessage.Data is ICalibrateAxisFieldMessageData calibrateData)
            {
                this.Logger.LogTrace("1:Parse Message Data");

                if (inverter.IsStarted)
                {
                    this.Logger.LogTrace("3:Starting Calibrate Axis FSM");
                    this.axisPositionUpdateTimer[(int)inverter.SystemIndex]?.Change(250, 250);

                    this.Logger.LogTrace("Start the timer for update status word");
                    this.statusWordUpdateTimer[(int)inverter.SystemIndex]?.Change(100, 100);

                    this.Logger.LogDebug($"Starting Calibrate Axis {calibrateData.AxisToCalibrate}");

                    this.currentAxis = calibrateData.AxisToCalibrate;
                    var currentStateMachine = new CalibrateAxisStateMachine(
                        this.currentAxis,
                        calibrateData.CalibrationType,
                        inverter,
                        this.Logger,
                        this.eventAggregator,
                        this.inverterCommandQueue,
                        this.ServiceScopeFactory);

                    this.currentStateMachines.Add(inverter.SystemIndex, currentStateMachine);
                    currentStateMachine.Start();
                }
                else
                {
                    this.Logger.LogDebug("4:Inverter is not ready. Powering up the inverter");

                    this.Logger.LogTrace("Start the timer for update status word");
                    this.statusWordUpdateTimer[(int)inverter.SystemIndex]?.Change(100, 200);

                    var currentStateMachine = new PowerOnStateMachine(
                        calibrateData.AxisToCalibrate,
                        inverter,
                        this.Logger,
                        this.eventAggregator,
                        this.inverterCommandQueue,
                        this.ServiceScopeFactory,
                        receivedMessage);

                    this.currentStateMachines.Add(inverter.SystemIndex, currentStateMachine);
                    currentStateMachine.Start();
                }

                if (inverter.SystemIndex == InverterIndex.MainInverter || inverter.SystemIndex == InverterIndex.Slave1)
                {
                    this.dataOld = null;
                }
            }
            else
            {
                this.Logger.LogError("5:Wrong message Data data type");

                var ex = new Exception();
                this.SendOperationErrorMessage(inverter.SystemIndex, new InverterExceptionFieldMessageData(ex, "Wrong message Data data type", 0), FieldMessageType.CalibrateAxis);
            }
        }

        private void ProcessDisableMessage(IInverterStatusBase inverter)
        {
            try
            {
                this.Logger.LogTrace("Start the timer for update status word");
                this.statusWordUpdateTimer[(int)inverter.SystemIndex]?.Change(100, 200);

                var currentStateMachine = new SwitchOffStateMachine(
                   inverter,
                   this.Logger,
                   this.eventAggregator,
                   this.inverterCommandQueue,
                   this.ServiceScopeFactory);

                this.currentStateMachines.Add(inverter.SystemIndex, currentStateMachine);
                currentStateMachine.Start();
            }
            catch (Exception ex)
            {
                this.Logger.LogError(ex, "Error while processing the disable message.");
                this.SendOperationErrorMessage(inverter.SystemIndex, new InverterExceptionFieldMessageData(ex, "Inverter status not configured for requested inverter Type", 0), FieldMessageType.InverterDisable);
            }
        }

        private void ProcessFaultResetMessage(IInverterStatusBase inverter)
        {
            this.Logger.LogTrace("1:Method Start");

            this.Logger.LogTrace("Start the timer for update status word");
            this.statusWordUpdateTimer[(int)inverter.SystemIndex]?.Change(100, 200);

            var currentStateMachine = new ResetFaultStateMachine(
                inverter,
                inverter.SystemIndex,
                this.Logger,
                this.eventAggregator,
                this.inverterCommandQueue,
                this.ServiceScopeFactory);

            this.currentStateMachines.Add(inverter.SystemIndex, currentStateMachine);
            currentStateMachine.Start();

            this.refreshTargetTable = true;
        }

        private async Task<bool> ProcessInverterCommand(InverterMessage message)
        {
            var result = false;

            var inverterMessagePacket = message.IsWriteMessage ? message.ToBytes() : message.GetReadMessage();

            try
            {
                result = await this.socketTransport.WriteAsync(inverterMessagePacket, message.SendDelay, this.CancellationToken) == inverterMessagePacket.Length;
                this.Logger.LogTrace($"1:ParameterId={message.ParameterId}:SendDelay{message.SendDelay}:Queue{this.inverterCommandQueue.Count}:inverterMessage={message}");
            }
            catch (InverterDriverException ex)
            {
                this.Logger.LogError(ex, $"Exception {ex.Message}, InverterExceptionCode={ex.InverterDriverExceptionCode}");
            }

            return result;
        }

        private void ProcessInverterProgrammingMessage(FieldCommandMessage receivedMessage, IInverterStatusBase inverter)
        {
            var currentInverter = Enum.Parse<InverterIndex>(receivedMessage.DeviceIndex.ToString());

            if (receivedMessage.Data is IInverterProgrammingFieldMessageData inverterProgrammingData)
            {
                this.Logger.LogTrace("1:Parse Message Data");
                this.statusWordUpdateTimer[(int)inverter.SystemIndex]?.Change(100, 10000);

                if (inverter.IsStarted)
                {
                    this.Logger.LogError("Inverter is started");
                }
                //if (!inverter.IsStarted)
                //{
                this.Logger.LogTrace("4:Starting InverterProgramming FSM");

                this.Logger.LogTrace("Start the timer for update status word");
                this.statusWordUpdateTimer[(int)inverter.SystemIndex]?.Change(100, 200);

                var inverterProgrammingFieldMessageData = new InverterProgrammingFieldMessageData(inverterProgrammingData.InverterParametersData);
                var currentStateMachine = new InverterProgrammigState(
                    inverter,
                    inverterProgrammingFieldMessageData,
                    this.eventAggregator,
                    this.inverterCommandQueue,
                    this.ServiceScopeFactory,
                    this.Logger);

                this.currentStateMachines.Add(currentInverter, currentStateMachine);
                currentStateMachine.Start();
                //}
                //else
                //{
                //    this.Logger.LogError("Inverter is started");

                //    var ex = new Exception();
                //    this.SendOperationErrorMessage(currentInverter, new InverterExceptionFieldMessageData(ex, "Inverter is started", 0), FieldMessageType.InverterProgramming);
                //}
            }
            else
            {
                this.Logger.LogError("5:Wrong message Data data type");

                var ex = new Exception();
                this.SendOperationErrorMessage(currentInverter, new InverterExceptionFieldMessageData(ex, "Wrong message Data data type", 0), FieldMessageType.InverterProgramming);
            }
        }

        private void ProcessInverterReadingMessage(FieldCommandMessage receivedMessage, IInverterStatusBase inverter)
        {
            var currentInverter = Enum.Parse<InverterIndex>(receivedMessage.DeviceIndex.ToString());

            if (receivedMessage.Data is IInverterReadingFieldMessageData inverterProgrammingData)
            {
                this.Logger.LogTrace("1:Parse Message Data");

                this.statusWordUpdateTimer[(int)inverter.SystemIndex]?.Change(100, 10000);

                if (inverter.IsStarted)
                {
                    this.Logger.LogError("Inverter is started");
                }

                //if (!inverter.IsStarted)
                //{
                this.Logger.LogTrace("4:Starting InverterReading FSM");

                this.Logger.LogTrace("Start the timer for update status word");
                this.statusWordUpdateTimer[(int)inverter.SystemIndex]?.Change(100, 200);

                var inverterReadingFieldMessageData = new InverterReadingFieldMessageData(inverterProgrammingData.InverterParametersData);
                var currentStateMachine = new InverterReadingState(
                    inverter,
                    inverterReadingFieldMessageData,
                    this.eventAggregator,
                    this.inverterCommandQueue,
                    this.ServiceScopeFactory,
                    this.Logger);

                this.currentStateMachines.Add(currentInverter, currentStateMachine);
                currentStateMachine.Start();
                //}
                //else
                //{
                //    this.Logger.LogError("Inverter is started");

                //    var ex = new Exception();
                //    this.SendOperationErrorMessage(currentInverter, new InverterExceptionFieldMessageData(ex, "Inverter is started", 0), FieldMessageType.InverterProgramming);
                //}
            }
            else
            {
                this.Logger.LogError("5:Wrong message Data data type");

                var ex = new Exception();
                this.SendOperationErrorMessage(currentInverter, new InverterExceptionFieldMessageData(ex, "Wrong message Data data type", 0), FieldMessageType.InverterReading);
            }
        }

        private void ProcessInverterSetTimerMessage(FieldCommandMessage receivedMessage, IInverterStatusBase inverter)
        {
            this.Logger.LogTrace("1:Method Start");
            if (receivedMessage.Data is IInverterSetTimerFieldMessageData updateData)
            {
                updateData.InverterIndex = inverter.SystemIndex;
                this.ConfigureTimer(updateData);
            }
            else
            {
                this.Logger.LogError("2:Wrong message Data data type");

                var ex = new Exception();
                this.SendOperationErrorMessage(inverter.SystemIndex, new InverterExceptionFieldMessageData(ex, "Wrong message Data data type", 0), FieldMessageType.InverterStatusUpdate);
            }
        }

        private void ProcessInverterSwitchOffMessage(IInverterStatusBase inverter)
        {
            this.Logger.LogTrace("1:Method Start");

            this.Logger.LogTrace("Start the timer for update status word");
            this.statusWordUpdateTimer[(int)inverter.SystemIndex]?.Change(100, 200);

            var currentStateMachine = new SwitchOffStateMachine(
                inverter,
                this.Logger,
                this.eventAggregator,
                this.inverterCommandQueue,
                this.ServiceScopeFactory);

            this.currentStateMachines.Add(inverter.SystemIndex, currentStateMachine);

            currentStateMachine.Start();
        }

        private void ProcessInverterSwitchOnMessage(FieldCommandMessage receivedMessage, IInverterStatusBase inverter)
        {
            this.Logger.LogTrace("1:Method Start");

            var currentInverter = Enum.Parse<InverterIndex>(receivedMessage.DeviceIndex.ToString());

            if (receivedMessage.Data is IInverterSwitchOnFieldMessageData switchOnData)
            {
                if (inverter.CommonStatusWord.IsReadyToSwitchOn &
                    inverter.CommonStatusWord.IsVoltageEnabled &
                    inverter.CommonStatusWord.IsQuickStopTrue)
                {
                    if (inverter.CommonControlWord.HorizontalAxis == (switchOnData.AxisToSwitchOn == Axis.Horizontal)
                        && inverter.CommonStatusWord.IsSwitchedOn
                        )
                    {
                        this.Logger.LogDebug($"Inverter Already active on selected axis {switchOnData.AxisToSwitchOn}");

                        var notificationMessageData = new InverterSwitchOnFieldMessageData(switchOnData.AxisToSwitchOn);
                        var notificationMessage = new FieldNotificationMessage(
                            notificationMessageData,
                            $"Inverter Switch On on axis {switchOnData.AxisToSwitchOn} End",
                            FieldMessageActor.DeviceManager,
                            FieldMessageActor.InverterDriver,
                            FieldMessageType.InverterSwitchOn,
                            MessageStatus.OperationEnd,
                            (byte)currentInverter);

                        this.Logger.LogTrace($"2:Type={notificationMessage.Type}:Destination={notificationMessage.Destination}:Status={notificationMessage.Status}");

                        this.eventAggregator.GetEvent<FieldNotificationEvent>().Publish(notificationMessage);
                    }
                    else
                    {
                        this.Logger.LogDebug("3: Switch On the inverter state machine");
                        this.Logger.LogDebug($"Inverter requires switching on selected axis {switchOnData.AxisToSwitchOn} inverter {inverter.SystemIndex}");

                        this.Logger.LogTrace("Start the timer for update status word");
                        this.statusWordUpdateTimer[(int)inverter.SystemIndex]?.Change(100, 200);

                        var currentStateMachine = new SwitchOnStateMachine(
                            switchOnData.AxisToSwitchOn,
                            inverter,
                            this.Logger,
                            this.eventAggregator,
                            this.inverterCommandQueue,
                            this.ServiceScopeFactory);

                        this.currentStateMachines.Add(currentInverter, currentStateMachine);
                        currentStateMachine.Start();
                    }
                }
                else
                {
                    //inverter.CommonControlWord.HorizontalAxis = switchOnData.AxisToSwitchOn == Axis.Horizontal;
                    this.Logger.LogDebug("6:Inverter is not ready. Powering up the inverter");

                    this.Logger.LogTrace("Start the timer for update status word");
                    this.statusWordUpdateTimer[(int)inverter.SystemIndex]?.Change(100, 200);

                    var currentStateMachine = new PowerOnStateMachine(
                        switchOnData.AxisToSwitchOn,
                        inverter,
                        this.Logger,
                        this.eventAggregator,
                        this.inverterCommandQueue,
                        this.ServiceScopeFactory,
                        receivedMessage);

                    this.currentStateMachines.Add(currentInverter, currentStateMachine);
                    currentStateMachine.Start();
                }
            }
            else
            {
                this.Logger.LogError("3:Invalid message data for ProcessInverterSwitchOnMessage message Type");

                this.SendOperationErrorMessage(currentInverter, new InverterExceptionFieldMessageData(null, "Invalid message data for InverterStop message Type", 0), FieldMessageType.InverterSwitchOn);
            }
        }

        private void ProcessMeasureProfileMessage(IInverterStatusBase inverter)
        {
            this.Logger.LogTrace("1:Method Start");

            if (this.inverterCommandQueue.Count(x => x.ParameterId == InverterParameterId.ProfileInput) < 2)
            {
                var inverterMessage = new InverterMessage(inverter.SystemIndex, InverterParameterId.ProfileInput);

                this.Logger.LogTrace($"1:inverterMessage={inverterMessage}");

                this.inverterCommandQueue.Enqueue(inverterMessage);
            }
        }

        private void ProcessPositioningMessage(
            FieldCommandMessage receivedMessage,
            IInverterStatusBase inverter,
            IServiceProvider serviceProvider)
        {
            if (receivedMessage.Data is IPositioningFieldMessageData positioningData)
            {
                this.Logger.LogTrace("1:Parse Message Data");

                if (inverter.IsStarted)
                {
                    this.currentAxis = positioningData.AxisMovement;

                    this.Logger.LogTrace("4:Starting Positioning FSM");

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

                        var invertersProvider = serviceProvider.GetRequiredService<IInvertersProvider>();
                        var targetPosition = invertersProvider.ComputePositioningValues(inverter, positioningData, axisOrientation, currentPosition, this.refreshTargetTable, out var positioningFieldData);
                        this.refreshTargetTable = false;

                        this.Logger.LogDebug($"Inverter: {inverter.SystemIndex}; " +
                            $"Current axis: {this.currentAxis}; " +
                            $"current position: {currentPosition}; " +
                            $"target: [{targetPosition} impulses]; " +
                            $"speed: {positioningFieldData.TargetSpeed[0]}; " +
                            $"acceleration: {positioningFieldData.TargetAcceleration[0]}; " +
                            $"movement type: {positioningFieldData.MovementType}");

                        this.axisPositionUpdateTimer[(int)inverter.SystemIndex]?.Change(Timeout.Infinite, Timeout.Infinite);

                        switch (positioningData.MovementType)
                        {
                            case MovementType.Absolute:
                                if (currentPosition == positioningFieldData.TargetPosition)
                                {
                                    var msgNotification = new FieldNotificationMessage(
                                        null,
                                        "Axis already in position",
                                        FieldMessageActor.DeviceManager,
                                        FieldMessageActor.InverterDriver,
                                        FieldMessageType.Positioning,
                                        MessageStatus.OperationEnd,
                                        inverter.SystemIndex);

                                    this.eventAggregator.GetEvent<FieldNotificationEvent>().Publish(msgNotification);
                                }
                                else
                                {
                                    //this.axisPositionUpdateTimer[(int)inverter.SystemIndex]?.Change(AXIS_POSITION_UPDATE_INTERVAL, AXIS_POSITION_UPDATE_INTERVAL);
                                    var currentStateMachine = new PositioningStateMachine(
                                        positioningFieldData,
                                        inverter,
                                        this.Logger,
                                        this.eventAggregator,
                                        this.inverterCommandQueue,
                                        this.ServiceScopeFactory);

                                    this.currentStateMachines.Add(inverter.SystemIndex, currentStateMachine);
                                    currentStateMachine.Start();
                                }
                                break;

                            case MovementType.Relative:
                                if (positioningFieldData.TargetPosition == 0)
                                {
                                    var msgNotification = new FieldNotificationMessage(
                                        null,
                                        "Axis already in position",
                                        FieldMessageActor.DeviceManager,
                                        FieldMessageActor.InverterDriver,
                                        FieldMessageType.Positioning,
                                        MessageStatus.OperationEnd,
                                        inverter.SystemIndex);

                                    this.eventAggregator.GetEvent<FieldNotificationEvent>().Publish(msgNotification);
                                }
                                else
                                {
                                    var currentStateMachine = new PositioningStateMachine(
                                        positioningFieldData,
                                        inverter,
                                        this.Logger,
                                        this.eventAggregator,
                                        this.inverterCommandQueue,
                                        this.ServiceScopeFactory);

                                    this.currentStateMachines.Add(inverter.SystemIndex, currentStateMachine);
                                    currentStateMachine.Start();
                                }
                                break;

                            case MovementType.TableTarget:
                                if (currentPosition == positioningFieldData.TargetPosition)
                                {
                                    var msgNotification = new FieldNotificationMessage(
                                        null,
                                        "Axis already in position",
                                        FieldMessageActor.DeviceManager,
                                        FieldMessageActor.InverterDriver,
                                        FieldMessageType.Positioning,
                                        MessageStatus.OperationEnd,
                                        inverter.SystemIndex);

                                    this.eventAggregator.GetEvent<FieldNotificationEvent>().Publish(msgNotification);
                                }
                                else
                                {
                                    var currentStateMachine = new PositioningTableStateMachine(
                                        positioningFieldData,
                                        this.dataOld,
                                        inverter,
                                        this.Logger,
                                        this.eventAggregator,
                                        this.inverterCommandQueue,
                                        this.ServiceScopeFactory);

                                    this.currentStateMachines.Add(inverter.SystemIndex, currentStateMachine);
                                    currentStateMachine.Start();
                                }
                                break;
                        }
                    }
                    catch (Exception ex)
                    {
                        this.Logger.LogError(ex, $"Error processing positioning message.");
                        this.SendOperationErrorMessage(inverter.SystemIndex, new InverterExceptionFieldMessageData(ex, "Wrong message Data Values", 0), FieldMessageType.Positioning);
                    }
                }
                else
                {
                    this.Logger.LogDebug("5:Inverter is not ready. Powering up the inverter");

                    var currentStateMachine = new PowerOnStateMachine(
                        positioningData.AxisMovement,
                        inverter,
                        this.Logger,
                        this.eventAggregator,
                        this.inverterCommandQueue,
                        this.ServiceScopeFactory,
                        receivedMessage);
                    this.currentStateMachines.Add(inverter.SystemIndex, currentStateMachine);
                    currentStateMachine.Start();
                }
            }
            else
            {
                this.Logger.LogError("6:Wrong message Data data type");

                var ex = new Exception();
                this.SendOperationErrorMessage(inverter.SystemIndex, new InverterExceptionFieldMessageData(ex, "Wrong message Data data type", 0), FieldMessageType.Positioning);
            }
        }

        private void ProcessPowerOffMessage(FieldCommandMessage receivedMessage, IInverterStatusBase inverter)
        {
            var currentInverter = Enum.Parse<InverterIndex>(receivedMessage.DeviceIndex.ToString());

            this.Logger.LogTrace("1:Parse Message Data");

            if (inverter.IsStarted)
            {
                this.Logger.LogDebug("3:Starting Power Off FSM");

                this.Logger.LogTrace("Start the timer for update status word");
                this.statusWordUpdateTimer[(int)inverter.SystemIndex]?.Change(100, 200);

                var currentStateMachine = new PowerOffStateMachine(
                    inverter,
                    this.Logger,
                    this.eventAggregator,
                    this.inverterCommandQueue,
                    this.ServiceScopeFactory);

                this.currentStateMachines.Add(currentInverter, currentStateMachine);
                currentStateMachine.Start();
            }
            else
            {
                this.Logger.LogTrace("4:Inverter already powered off. Just notify operation completed");
                var endNotification = new FieldNotificationMessage(
                    new InverterPowerOffFieldMessageData(),
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

            this.Logger.LogTrace("1:Parse Message Data");

            if (inverter.IsStarted)
            {
                this.Logger.LogTrace("3:Inverter already powered on. Just notify operation completed");
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
                this.Logger.LogDebug("7:Inverter is not ready. Powering up the inverter");

                this.Logger.LogTrace("Start the timer for update status word");
                this.statusWordUpdateTimer[(int)inverter.SystemIndex]?.Change(100, 200);

                var currentStateMachine = new PowerOnStateMachine(
                    Axis.Vertical,
                    inverter,
                    this.Logger,
                    this.eventAggregator,
                    this.inverterCommandQueue,
                    this.ServiceScopeFactory);

                this.currentStateMachines.Add(currentInverter, currentStateMachine);
                currentStateMachine.Start();
            }
        }

        private void ProcessReadCurrentError(IInverterStatusBase inverter)
        {
            if (!this.inverterCommandQueue.Any(x => x.ParameterId == InverterParameterId.CurrentError && x.SystemIndex == inverter.SystemIndex))
            {
                var inverterMessage = new InverterMessage(inverter.SystemIndex, InverterParameterId.CurrentError);

                this.Logger.LogTrace($"1:inverterMessage={inverterMessage}");

                this.inverterCommandQueue.Enqueue(inverterMessage);
            }
        }

        private void ProcessShutterPositioningMessage(FieldCommandMessage receivedMessage, IInverterStatusBase inverter)
        {
            var currentInverter = Enum.Parse<InverterIndex>(receivedMessage.DeviceIndex.ToString());

            if (receivedMessage.Data is IShutterPositioningFieldMessageData shutterPositioningData)
            {
                this.Logger.LogTrace("1:Parse Message Data");

                if (inverter.IsStarted)
                {
                    this.Logger.LogTrace("4:Starting ShutterPositioning FSM");

                    this.Logger.LogTrace("Start the timer for update status word");
                    this.statusWordUpdateTimer[(int)inverter.SystemIndex]?.Change(100, 200);

                    var convertedShutterPositioningData = new InverterShutterPositioningFieldMessageData(shutterPositioningData);
                    var currentStateMachine = new ShutterPositioningStateMachine(
                        convertedShutterPositioningData,
                        inverter,
                        this.Logger,
                        this.eventAggregator,
                        this.inverterCommandQueue,
                        this.ServiceScopeFactory);

                    this.currentStateMachines.Add(currentInverter, currentStateMachine);
                    currentStateMachine.Start();
                }
                else
                {
                    this.Logger.LogDebug("5:Inverter is not ready. Powering up the inverter");

                    this.Logger.LogTrace("Start the timer for update status word");
                    this.statusWordUpdateTimer[(int)inverter.SystemIndex]?.Change(100, 200);

                    var currentStateMachine = new PowerOnStateMachine(
                        Axis.Vertical,
                        inverter,
                        this.Logger,
                        this.eventAggregator,
                        this.inverterCommandQueue,
                        this.ServiceScopeFactory,
                        receivedMessage);
                    this.currentStateMachines.Add(currentInverter, currentStateMachine);
                    currentStateMachine.Start();
                }
            }
            else
            {
                this.Logger.LogError("5:Wrong message Data data type");

                var ex = new Exception();
                this.SendOperationErrorMessage(currentInverter, new InverterExceptionFieldMessageData(ex, "Wrong message Data data type", 0), FieldMessageType.ShutterPositioning);
            }
        }

        private void ProcessStopMessage(IInverterStatusBase inverter)
        {
            this.Logger.LogTrace("1:Method Start");

            this.Logger.LogTrace("Start the timer for update status word");
            this.statusWordUpdateTimer[(int)inverter.SystemIndex]?.Change(100, 200);

            var currentStateMachine = new StopStateMachine(
                inverter,
                this.Logger,
                this.eventAggregator,
                this.inverterCommandQueue,
                this.ServiceScopeFactory);

            this.currentStateMachines.Add(inverter.SystemIndex, currentStateMachine);
            currentStateMachine.Start();
        }

        private void RequestAxisPositionUpdate(object state)
        {
            lock (this.syncAxisTimer)
            {
                var inverterIndex = (InverterIndex)state;
                {
                    var readAxisPositionMessage = new InverterMessage(inverterIndex, InverterParameterId.ActualPositionShaft);

                    this.Logger.LogTrace($"1:ReadAxisPositionMessage={readAxisPositionMessage}");

                    if (this.inverterCommandQueue
                            .Count(x =>
                                x.ParameterId == InverterParameterId.ActualPositionShaft
                                &&
                                x.SystemIndex == inverterIndex) < 2)
                    {
                        this.inverterCommandQueue.Enqueue(readAxisPositionMessage);
                    }
                }
            }
        }

        private void RequestHeartBeat(object state)
        {
            //lock (this.syncHeartBeatTimer)
            {
                //this.Logger.LogTrace($"0");
                if (this.socketTransport.IsConnected
                    && state is InverterIndex.MainInverter
                    && this.inverterCommandQueue.Count(x => x.ParameterId == InverterParameterId.HeartBeatTimer1) < 2
                    )
                {
                    var value = (short)(this.isHeartBeatOn ? 1 : 0);

                    var heartBeatMessage = new InverterMessage(
                        (byte)state,
                        (short)InverterParameterId.HeartBeatTimer1,
                        value,
                        InverterDataset.HeartBeat);

                    this.Logger.LogTrace($"1.RequestHeartBeat={heartBeatMessage}");
                    this.inverterCommandQueue.Enqueue(heartBeatMessage);
                    //this.Logger.LogTrace($"2.RequestHeartBeat={heartBeatMessage}");

                    this.isHeartBeatOn = !this.isHeartBeatOn;
                }
                else
                {
                    this.Logger.LogDebug($"2.RequestHeartBeat missed: count {this.inverterCommandQueue.Count(x => x.ParameterId == InverterParameterId.HeartBeatTimer1)}");
                }
            }
        }

        private void RequestSensorStatusUpdate(object state)
        {
            lock (this.syncSensorTimer)
            {
                var readSensorStatusMessage = new InverterMessage(InverterIndex.MainInverter, InverterParameterId.DigitalInputsOutputs);

                if (this.inverterCommandQueue.Count(x => x.ParameterId == InverterParameterId.DigitalInputsOutputs) < 1)
                {
                    this.Logger.LogTrace($"1:ReadSensorStatusTimer={readSensorStatusMessage} Count:{this.inverterCommandQueue.Count(x => x.ParameterId == InverterParameterId.DigitalInputsOutputs)}");

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
                if (this.inverterCommandQueue.Count(x => x.ParameterId == InverterParameterId.StatusWord && x.SystemIndex == inverterIndex) < 2)
                {
                    var readStatusWordMessage = new InverterMessage(inverterIndex, InverterParameterId.StatusWord);

                    this.Logger.LogTrace($"1:readStatusWordTimer={readStatusWordMessage}");

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

                if (!encodedWord.Equals("\0", StringComparison.InvariantCultureIgnoreCase))
                {
                    if (int.TryParse(encodedWord, out var values))
                    {
                        var dataByte = inverterIndex % 2;

                        for (var index = 8 * dataByte; index < 8 + (8 * dataByte); index++)
                        {
                            returnValue[index - (8 * dataByte)] = (values & (0x0001 << index)) > 0;
                        }
                    }
                    else
                    {
                        this.Logger.LogError($"Not valid Sensors in inverter message: {currentMessageStringPayload}");
                    }
                }
            }

            return returnValue;
        }

        private async Task StartHardwareCommunicationsAsync(IServiceProvider serviceProvider)
        {
            this.Logger.LogTrace("1:Method Start");

            var masterInverter = serviceProvider
                .GetRequiredService<IDigitalDevicesDataProvider>()
                .GetInverterByIndex(InverterIndex.MainInverter);

            this.inverterAddress = masterInverter.IpAddress;
            this.inverterPort = masterInverter.TcpPort;

            this.socketTransport.Configure(this.inverterAddress, this.inverterPort);
            this.Logger.LogInformation($"1:Configure Inverter {masterInverter.Index}, tcp-endpoint={this.inverterAddress}:{this.inverterPort}");

            try
            {
                await this.socketTransport.ConnectAsync();
            }
            catch (InverterDriverException ex)
            {
                this.Logger.LogError($"1A: Exception {ex.Message}; Exception code={ex.InverterDriverExceptionCode};\nInner exception: {ex.InnerException.Message}");
            }
            catch (Exception ex)
            {
                this.Logger.LogCritical($"Error while connecting Receiver Socket Transport: {ex.Message}");

                this.SendOperationErrorMessage(InverterIndex.MainInverter, new InverterExceptionFieldMessageData(ex, "while Connecting Receiver Socket Transport", 0), FieldMessageType.InverterException);
                throw new InverterDriverException($"Exception {ex.Message} StartHardwareCommunications Failed 1", ex);
            }

            if (!this.socketTransport.IsConnected)
            {
                this.Logger.LogError("3:Socket Transport failed to connect");

                var ex = new Exception();
                this.SendOperationErrorMessage(InverterIndex.MainInverter, new InverterExceptionFieldMessageData(ex, "Socket Transport failed to connect", 0), FieldMessageType.InverterError);
            }
            else
            {
                this.Logger.LogInformation($"3:Connection OK ipAddress={this.inverterAddress}:Port={this.inverterPort}");
            }

            try
            {
                this.inverterReceiveTask.Start();
                this.inverterSendTask.Start();
            }
            catch (Exception ex)
            {
                this.Logger.LogCritical($"Error while starting inverter socket threads: {ex.Message}");

                this.SendOperationErrorMessage(InverterIndex.MainInverter, new InverterExceptionFieldMessageData(ex, "while starting service threads", 0), FieldMessageType.InverterException);
                throw new InverterDriverException($"Exception {ex.Message} StartHardwareCommunications Failed 2", ex);
            }
        }

        #endregion
    }
}
