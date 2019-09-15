using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataLayer.Interfaces;
using Ferretto.VW.MAS.InverterDriver.Diagnostics;
using Ferretto.VW.MAS.InverterDriver.Interface;
using Ferretto.VW.MAS.InverterDriver.Interface.StateMachines;
using Ferretto.VW.MAS.InverterDriver.InverterStatus.Interfaces;
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
using Ferretto.VW.MAS.Utils.Exceptions;
using Ferretto.VW.MAS.Utils.Messages;
using Ferretto.VW.MAS.Utils.Messages.FieldData;
using Ferretto.VW.MAS.Utils.Messages.FieldInterfaces;
using Ferretto.VW.MAS.Utils.Utilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Prism.Events;
using static Ferretto.VW.MAS.Utils.Utilities.BufferUtility;

// ReSharper disable ArrangeThisQualifier
// ReSharper disable ParameterHidesMember
namespace Ferretto.VW.MAS.InverterDriver
{
    public partial class InverterDriverService : BackgroundService
    {
        #region Fields

        private const int AXIS_POSITION_UPDATE_INTERVAL = 100;

        private const int HEARTBEAT_TIMEOUT = 300;   // 300

        private const int SENSOR_STATUS_UPDATE_INTERVAL = 500;

        private const int STATUS_WORD_UPDATE_INTERVAL = 300;

        private readonly Stopwatch axisIntervalStopwatch;

        private readonly Timer[] axisPositionUpdateTimer;

        private readonly Stopwatch axisStopwatch;

        private readonly BlockingConcurrentQueue<FieldCommandMessage> commandQueue;

        private readonly Task commandReceiveTask;

        private readonly Dictionary<InverterIndex, IInverterStateMachine> currentStateMachines;

        private readonly IConfigurationValueManagmentDataLayer dataLayerConfigurationValueManagement;

        private readonly IResolutionConversionDataLayer dataLayerResolutionConversion;

        private readonly IEventAggregator eventAggregator;

        private readonly BlockingConcurrentQueue<InverterMessage> heartbeatQueue;

        private readonly BlockingConcurrentQueue<InverterMessage> inverterCommandQueue;

        private readonly Task inverterReceiveTask;

        private readonly Task inverterSendTask;

        private readonly Dictionary<InverterIndex, IInverterStatusBase> inverterStatuses;

        private readonly ILogger logger;

        private readonly BlockingConcurrentQueue<FieldNotificationMessage> notificationQueue;

        private readonly Task notificationReceiveTask;

        private readonly Stopwatch readSpeedStopwatch;

        private readonly Stopwatch readWaitStopwatch;

        private readonly Stopwatch roundTripStopwatch;

        private readonly Stopwatch sensorIntervalStopwatch;

        private readonly Stopwatch sensorStopwatch;

        private readonly IServiceScopeFactory serviceScopeFactory;

        private readonly ISocketTransport socketTransport;

        private readonly Timer[] statusWordUpdateTimer;

        private readonly IVertimagConfigurationDataLayer vertimagConfiguration;

        private readonly ManualResetEventSlim writeEnableEvent;

        private Axis currentAxis;

        private bool disposed;

        private bool forceStatusPublish;

        private Timer heartBeatTimer;

        private byte[] receiveBuffer;

        private Timer sensorStatusUpdateTimer;

        private CancellationToken stoppingToken;

        #endregion

        #region Constructors

        public InverterDriverService(
            ILogger<InverterDriverService> logger,
            IEventAggregator eventAggregator,
            IServiceScopeFactory serviceScopeFactory,
            ISocketTransport socketTransport,
            IConfigurationValueManagmentDataLayer dataLayerConfigurationValueManagement,
            IVertimagConfigurationDataLayer vertimagConfiguration,
            IResolutionConversionDataLayer dataLayerResolutionConversion)
        {
            if (eventAggregator is null)
            {
                throw new ArgumentNullException(nameof(eventAggregator));
            }

            if (logger is null)
            {
                throw new ArgumentNullException(nameof(logger));
            }

            if (serviceScopeFactory is null)
            {
                throw new ArgumentNullException(nameof(serviceScopeFactory));
            }

            this.socketTransport = socketTransport;
            this.eventAggregator = eventAggregator;
            this.dataLayerConfigurationValueManagement = dataLayerConfigurationValueManagement;
            this.dataLayerResolutionConversion = dataLayerResolutionConversion;
            this.vertimagConfiguration = vertimagConfiguration;
            this.logger = logger;
            this.serviceScopeFactory = serviceScopeFactory;
            this.readWaitStopwatch = new Stopwatch();

            this.readSpeedStopwatch = new Stopwatch();

            this.roundTripStopwatch = new Stopwatch();

            this.axisStopwatch = new Stopwatch();

            this.axisIntervalStopwatch = new Stopwatch();

            this.sensorStopwatch = new Stopwatch();

            this.sensorIntervalStopwatch = new Stopwatch();

            this.ReadWaitTimeData = new InverterDiagnosticsData();

            this.ReadSpeedTimeData = new InverterDiagnosticsData();

            this.WriteRoundtripTimeData = new InverterDiagnosticsData();

            this.AxisTimeData = new InverterDiagnosticsData();

            this.AxisIntervalTimeData = new InverterDiagnosticsData();

            this.SensorTimeData = new InverterDiagnosticsData();

            this.SensorIntervalTimeData = new InverterDiagnosticsData();

            this.inverterStatuses = new Dictionary<InverterIndex, IInverterStatusBase>();

            this.currentStateMachines = new Dictionary<InverterIndex, IInverterStateMachine>();

            this.heartbeatQueue = new BlockingConcurrentQueue<InverterMessage>();
            this.inverterCommandQueue = new BlockingConcurrentQueue<InverterMessage>();

            this.commandQueue = new BlockingConcurrentQueue<FieldCommandMessage>();
            this.notificationQueue = new BlockingConcurrentQueue<FieldNotificationMessage>();

            this.writeEnableEvent = new ManualResetEventSlim(true);

            this.commandReceiveTask = new Task(this.CommandReceiveTaskFunction);
            this.notificationReceiveTask = new Task(async () => await this.NotificationReceiveTaskFunction());
            this.inverterReceiveTask = new Task(async () => await this.ReceiveInverterData());
            this.inverterSendTask = new Task(async () => await this.SendInverterCommand());

            this.logger.LogTrace("1:Subscription Command");

            this.InitializeMethodSubscriptions();

            this.axisPositionUpdateTimer = new Timer[(int)InverterIndex.Slave7 + 1];
            this.statusWordUpdateTimer = new Timer[(int)InverterIndex.Slave7 + 1];
        }

        #endregion

        #region Properties

        public InverterDiagnosticsData AxisIntervalTimeData { get; }

        public InverterDiagnosticsData AxisTimeData { get; }

        public InverterDiagnosticsData ReadSpeedTimeData { get; }

        public InverterDiagnosticsData ReadWaitTimeData { get; }

        public InverterDiagnosticsData SensorIntervalTimeData { get; }

        public InverterDiagnosticsData SensorTimeData { get; }

        public InverterDiagnosticsData WriteRoundtripTimeData { get; }

        #endregion

        #region Methods

        public void Dispose(bool disposing)
        {
            if (this.disposed)
            {
                return;
            }

            if (disposing)
            {
                this.heartBeatTimer?.Dispose();
                this.sensorStatusUpdateTimer?.Dispose();
                for (var id = InverterIndex.MainInverter; id <= InverterIndex.Slave7; id++)
                {
                    this.axisPositionUpdateTimer[(int)id]?.Dispose();
                    this.statusWordUpdateTimer[(int)id]?.Dispose();
                }
                this.writeEnableEvent?.Dispose();
            }

            this.disposed = true;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            this.logger.LogTrace("1:Method Start");

            this.stoppingToken = stoppingToken;

            try
            {
                this.commandReceiveTask.Start();
                this.notificationReceiveTask.Start();
            }
            catch (Exception ex)
            {
                this.logger.LogCritical($"2:Exception: {ex.Message} while starting service threads");

                this.SendOperationErrorMessage(InverterIndex.None, new InverterExceptionFieldMessageData(ex, "Inverter Driver Exception", 0), FieldMessageType.InverterException);
            }

            return Task.CompletedTask;
        }

        private void CommandReceiveTaskFunction()
        {
            this.sensorStatusUpdateTimer?.Dispose();
            this.sensorStatusUpdateTimer = new Timer(this.RequestSensorStatusUpdate, null, -1, Timeout.Infinite);

            for (var id = InverterIndex.MainInverter; id <= InverterIndex.Slave7; id++)
            {
                this.axisPositionUpdateTimer[(int)id]?.Dispose();
                this.axisPositionUpdateTimer[(int)id] = new Timer(this.RequestAxisPositionUpdate, id, -1, Timeout.Infinite);
                this.statusWordUpdateTimer[(int)id]?.Dispose();
                this.statusWordUpdateTimer[(int)id] = new Timer(this.RequestStatusWordMessage, id, -1, Timeout.Infinite);
            }

            do
            {
                try
                {
                    this.commandQueue.TryDequeue(Timeout.Infinite, this.stoppingToken, out var receivedMessage);

                    this.OnCommandReceived(receivedMessage);
                }
                catch (OperationCanceledException)
                {
                    this.logger.LogDebug("2:Method End operation cancelled");

                    return;
                }
                catch (Exception ex)
                {
                    this.logger.LogError($"3:Exception: {ex.Message}");

                    this.SendOperationErrorMessage(InverterIndex.None, new InverterExceptionFieldMessageData(ex, "Inverter Driver Exception", 0), FieldMessageType.InverterException);

                    return;
                }
            }
            while (!this.stoppingToken.IsCancellationRequested);
        }

        private async Task NotificationReceiveTaskFunction()
        {
            do
            {
                FieldNotificationMessage receivedMessage;
                try
                {
                    this.notificationQueue.TryDequeue(Timeout.Infinite, this.stoppingToken, out receivedMessage);

                    this.logger.LogTrace($"1:Notification received: {receivedMessage.Type}, destination: {receivedMessage.Destination}, source: {receivedMessage.Source}, status: {receivedMessage.Status}");
                }
                catch (OperationCanceledException)
                {
                    this.logger.LogDebug("2:Method End operation cancelled");

                    return;
                }
                catch (Exception ex)
                {
                    this.logger.LogDebug($"3:Exception: {ex.Message}");

                    this.SendOperationErrorMessage(InverterIndex.None, new InverterExceptionFieldMessageData(ex, "Inverter Driver Exception", 0), FieldMessageType.InverterException);

                    return;
                }

                var messageDeviceIndex = Enum.Parse<InverterIndex>(receivedMessage.DeviceIndex.ToString());
                this.currentStateMachines.TryGetValue(messageDeviceIndex, out var messageCurrentStateMachine);

                switch (receivedMessage.Type)
                {
                    case FieldMessageType.DataLayerReady:

                        await this.StartHardwareCommunications();
                        this.InitializeInverterStatus();

                        break;

                    case FieldMessageType.Positioning:
                        {
                            if (receivedMessage.Status == MessageStatus.OperationEnd ||
                                receivedMessage.Status == MessageStatus.OperationError)
                            {
                                this.logger.LogDebug($"Positioning Deallocating {messageCurrentStateMachine?.GetType()} state machine");
                                this.logger.LogTrace($"4:Deallocation SM {messageCurrentStateMachine?.GetType()}");

                                if (messageCurrentStateMachine is PositioningStateMachine)
                                {
                                    this.currentStateMachines.Remove(messageDeviceIndex);
                                }
                                else if (messageCurrentStateMachine is PositioningTableStateMachine)
                                {
                                    this.currentStateMachines.Remove(messageDeviceIndex);
                                }
                                else
                                {
                                    this.logger.LogDebug($"Try to deallocate {messageCurrentStateMachine?.GetType()} Handling {receivedMessage.Type}");
                                }

                                this.logger.LogTrace("4: Stop the timer for update shaft position");
                                this.axisPositionUpdateTimer[(int)messageDeviceIndex].Change(Timeout.Infinite, Timeout.Infinite);
                            }

                            if (receivedMessage.Status == MessageStatus.OperationStop)
                            {
                                this.logger.LogTrace($"5:Deallocation SM {messageCurrentStateMachine?.GetType()}");

                                if (messageCurrentStateMachine is PositioningStateMachine)
                                {
                                    this.currentStateMachines.Remove(messageDeviceIndex);
                                }
                                else if (messageCurrentStateMachine is PositioningTableStateMachine)
                                {
                                    this.currentStateMachines.Remove(messageDeviceIndex);
                                }
                                else
                                {
                                    this.logger.LogDebug($"Try to deallocate {messageCurrentStateMachine?.GetType()} Handling {receivedMessage.Type}");
                                }

                                this.logger.LogTrace("4: Stop the timer for update shaft position");
                                this.axisPositionUpdateTimer[(int)messageDeviceIndex].Change(Timeout.Infinite, Timeout.Infinite);

                                // Enqueue a message to execute the Stop states machine
                                var stopMessage = new FieldCommandMessage(
                                    null,
                                    "Stop inverter",
                                    FieldMessageActor.InverterDriver,
                                    FieldMessageActor.InverterDriver,
                                    FieldMessageType.InverterStop,
                                    receivedMessage.DeviceIndex);
                                this.commandQueue.Enqueue(stopMessage);
                            }

                            break;
                        }
                    case FieldMessageType.CalibrateAxis:

                        if (receivedMessage.Status == MessageStatus.OperationEnd ||
                            receivedMessage.Status == MessageStatus.OperationError)
                        {
                            this.logger.LogDebug($"CalibrateAxis Deallocating {messageCurrentStateMachine?.GetType()} state machine");

                            if (messageCurrentStateMachine is CalibrateAxisStateMachine)
                            {
                                this.currentStateMachines.Remove(messageDeviceIndex);
                            }
                            else
                            {
                                this.logger.LogDebug($"Try to deallocate {messageCurrentStateMachine?.GetType()} Handling {receivedMessage.Type}");
                            }
                        }
                        if (receivedMessage.Status == MessageStatus.OperationStop)
                        {
                            if (messageCurrentStateMachine is CalibrateAxisStateMachine)
                            {
                                this.currentStateMachines.Remove(messageDeviceIndex);
                            }
                            else
                            {
                                this.logger.LogDebug($"Try to deallocate {messageCurrentStateMachine?.GetType()} Handling {receivedMessage.Type}");
                            }

                            // Enqueue a message to execute the Stop states machine
                            var stopMessage = new FieldCommandMessage(
                                null,
                                "Stop inverter",
                                FieldMessageActor.InverterDriver,
                                FieldMessageActor.InverterDriver,
                                FieldMessageType.InverterStop,
                                receivedMessage.DeviceIndex);
                            this.commandQueue.Enqueue(stopMessage);
                        }

                        break;

                    case FieldMessageType.ShutterPositioning:

                        this.logger.LogDebug($"ShutterPositioning Deallocating {messageCurrentStateMachine?.GetType()} state machine");
                        if (receivedMessage.Status == MessageStatus.OperationEnd ||
                            receivedMessage.Status == MessageStatus.OperationError)
                        {
                            if (messageCurrentStateMachine is ShutterPositioningStateMachine)
                            {
                                this.currentStateMachines.Remove(messageDeviceIndex);
                            }
                            else
                            {
                                this.logger.LogDebug($"Try to deallocate {messageCurrentStateMachine?.GetType()} Handling {receivedMessage.Type}");
                            }
                        }
                        if (receivedMessage.Status == MessageStatus.OperationStop)
                        {
                            if (messageCurrentStateMachine is ShutterPositioningStateMachine)
                            {
                                this.currentStateMachines.Remove(messageDeviceIndex);
                            }
                            else
                            {
                                this.logger.LogDebug($"Try to deallocate {messageCurrentStateMachine?.GetType()} Handling {receivedMessage.Type}");
                            }

                            // Enqueue a message to execute the Stop states machine
                            var stopMessage = new FieldCommandMessage(
                                null,
                                "Stop inverter",
                                FieldMessageActor.InverterDriver,
                                FieldMessageActor.InverterDriver,
                                FieldMessageType.InverterStop,
                                receivedMessage.DeviceIndex);
                            this.commandQueue.Enqueue(stopMessage);
                        }

                        break;

                    case FieldMessageType.InverterSwitchOn:
                    case FieldMessageType.InverterStop:

                        this.logger.LogDebug($"Deallocating {messageCurrentStateMachine?.GetType()} state machine ({receivedMessage.Type})");
                        if (receivedMessage.Status == MessageStatus.OperationEnd ||
                            receivedMessage.Status == MessageStatus.OperationError)
                        {
                            if (messageCurrentStateMachine is null)
                            {
                                this.logger.LogDebug($"State machine is null !!");
                            }

                            if (messageCurrentStateMachine is SwitchOnStateMachine ||
                                messageCurrentStateMachine is StopStateMachine)
                            {
                                this.currentStateMachines.Remove(messageDeviceIndex);
                            }
                            else
                            {
                                this.logger.LogDebug($"Try to deallocate {messageCurrentStateMachine?.GetType()} Handling {receivedMessage.Type}");
                            }
                        }

                        break;

                    case FieldMessageType.InverterSwitchOff:
                        if (receivedMessage.Status == MessageStatus.OperationEnd ||
                            receivedMessage.Status == MessageStatus.OperationError)
                        {
                            this.logger.LogDebug($"InverterSwitchOff Deallocating {messageCurrentStateMachine?.GetType()} state machine");

                            if (messageCurrentStateMachine is SwitchOffStateMachine)
                            {
                                this.currentStateMachines.Remove(messageDeviceIndex);
                            }
                            else
                            {
                                this.logger.LogDebug($"Try to deallocate {messageCurrentStateMachine?.GetType()} Handling {receivedMessage.Type}");
                            }

                            var nextMessage = ((InverterSwitchOffFieldMessageData)receivedMessage.Data).NextCommandMessage;
                            if (nextMessage != null)
                            {
                                this.commandQueue.Enqueue(nextMessage);
                            }
                        }

                        break;

                    case FieldMessageType.InverterPowerOn:

                        if (receivedMessage.Status == MessageStatus.OperationEnd ||
                            receivedMessage.Status == MessageStatus.OperationError)
                        {
                            this.logger.LogDebug($"Deallocating {messageCurrentStateMachine?.GetType()} state machine");

                            if (messageCurrentStateMachine is PowerOnStateMachine)
                            {
                                this.currentStateMachines.Remove(messageDeviceIndex);
                            }
                            else
                            {
                                this.logger.LogDebug($"Try to deallocate {messageCurrentStateMachine?.GetType()} Handling {receivedMessage.Type}");
                            }

                            var nextMessage = ((InverterPowerOnFieldMessageData)receivedMessage.Data).NextCommandMessage;
                            if (nextMessage != null)
                            {
                                this.commandQueue.Enqueue(nextMessage);
                            }
                        }

                        break;

                    case FieldMessageType.InverterPowerOff:

                        if (receivedMessage.Status == MessageStatus.OperationEnd ||
                            receivedMessage.Status == MessageStatus.OperationError)
                        {
                            this.logger.LogDebug($"Deallocating {messageCurrentStateMachine?.GetType()} state machine");

                            if (messageCurrentStateMachine is PowerOffStateMachine)
                            {
                                this.currentStateMachines.Remove(messageDeviceIndex);
                            }
                            else
                            {
                                this.logger.LogDebug($"Try to deallocate {messageCurrentStateMachine?.GetType()} Handling {receivedMessage.Type}");
                            }

                            var nextMessage = ((InverterPowerOffFieldMessageData)receivedMessage.Data).NextCommandMessage;
                            if (nextMessage != null)
                            {
                                this.commandQueue.Enqueue(nextMessage);
                            }
                        }

                        break;

                    case FieldMessageType.InverterFaultReset:

                        if (receivedMessage.Status == MessageStatus.OperationEnd ||
                            receivedMessage.Status == MessageStatus.OperationError)
                        {
                            this.logger.LogDebug($"InverterFaultReset Deallocating {messageCurrentStateMachine?.GetType()} state machine");

                            if (messageCurrentStateMachine is ResetFaultStateMachine)
                            {
                                this.currentStateMachines.Remove(messageDeviceIndex);
                            }
                            else
                            {
                                this.logger.LogDebug($"Try to deallocate {messageCurrentStateMachine?.GetType()} Handling {receivedMessage.Type}");
                            }
                        }

                        break;
                }

                if (receivedMessage.Source == FieldMessageActor.InverterDriver)
                {
                    if (receivedMessage.Status == MessageStatus.OperationEnd ||
                        receivedMessage.Status == MessageStatus.OperationStop)
                    {
                        var notificationMessageToFsm = receivedMessage;
                        //TEMP Set the destination of message to FSM
                        notificationMessageToFsm.Destination = FieldMessageActor.FiniteStateMachines;

                        this.eventAggregator?.GetEvent<FieldNotificationEvent>().Publish(notificationMessageToFsm);
                    }
                }
            }
            while (!this.stoppingToken.IsCancellationRequested);
        }

        private void OnCommandReceived(FieldCommandMessage receivedMessage)
        {
            this.logger.LogTrace($"1:Command received: {receivedMessage.Type}, destination: {receivedMessage.Destination}, source: {receivedMessage.Source}");

            var messageDeviceIndex = Enum.Parse<InverterIndex>(receivedMessage.DeviceIndex.ToString());

            if (this.inverterStatuses.Count == 0)
            {
                this.logger.LogError("4:Invert Driver not configured for this message Type");

                var ex = new Exception();
                this.SendOperationErrorMessage(messageDeviceIndex, new InverterExceptionFieldMessageData(ex, "Invert Driver not configured for this message Type", 0), FieldMessageType.InverterError);

                return;
            }

            this.currentStateMachines.TryGetValue(messageDeviceIndex, out var messageCurrentStateMachine);

            if (messageCurrentStateMachine != null && receivedMessage.Type == FieldMessageType.InverterStop)
            {
                this.logger.LogTrace("4: Stop the timer for update shaft position");
                this.axisPositionUpdateTimer[(int)messageDeviceIndex].Change(Timeout.Infinite, Timeout.Infinite);

                messageCurrentStateMachine.Stop();

                return;
            }

            if (messageCurrentStateMachine != null && receivedMessage.Type != FieldMessageType.InverterSetTimer)
            {
                this.logger.LogWarning($"5:Inverter Driver already executing operation {messageCurrentStateMachine.GetType()}");
                this.logger.LogError($"5a: Message {receivedMessage.Type} will be discarded!");
                var ex = new Exception();
                this.SendOperationErrorMessage(messageDeviceIndex, new InverterExceptionFieldMessageData(ex, "Inverter operation already in progress", 0), FieldMessageType.InverterError);

                return;
            }

            switch (receivedMessage.Type)
            {
                case FieldMessageType.CalibrateAxis:
                    this.ProcessCalibrateAxisMessage(receivedMessage);
                    break;

                case FieldMessageType.InverterPowerOff:
                    this.ProcessPowerOffMessage(receivedMessage);
                    break;

                case FieldMessageType.InverterPowerOn:
                    this.ProcessPowerOnMessage(receivedMessage);
                    break;

                case FieldMessageType.Positioning:
                case FieldMessageType.TorqueCurrentSampling:
                    this.ProcessPositioningMessage(receivedMessage);
                    break;

                case FieldMessageType.ShutterPositioning:
                    this.ProcessShutterPositioningMessage(receivedMessage);
                    break;

                case FieldMessageType.InverterSetTimer:
                    this.ProcessInverterSetTimerMessage(receivedMessage);
                    break;

                case FieldMessageType.InverterSwitchOff:
                    this.ProcessInverterSwitchOffMessage(receivedMessage);
                    break;

                case FieldMessageType.InverterSwitchOn:
                    this.ProcessInverterSwitchOnMessage(receivedMessage);
                    break;

                case FieldMessageType.InverterStop:
                    this.ProcessStopMessage(receivedMessage);
                    break;

                case FieldMessageType.InverterFaultReset:
                    this.ProcessFaultResetMessage(receivedMessage);
                    break;

                case FieldMessageType.InverterDisable:
                    this.ProcessDisableMessage(receivedMessage);
                    break;
            }

            var notificationMessageData = new MachineStatusActiveMessageData(MessageActor.InverterDriver, receivedMessage.Type.ToString(), MessageVerbosity.Info);
            var notificationMessage = new NotificationMessage(
                notificationMessageData,
                $"Inverter current machine status {receivedMessage.Type}",
                MessageActor.Any,
                MessageActor.InverterDriver,
                MessageType.MachineStatusActive,
                MessageStatus.OperationStart);

            this.eventAggregator?.GetEvent<NotificationEvent>().Publish(notificationMessage);

            this.logger.LogTrace($"Socket Timings: Read Wait Samples {this.ReadWaitTimeData.TotalSamples}, Max {this.ReadWaitTimeData.MaxValue}ms, Min {this.ReadWaitTimeData.MinValue}ms, Average {this.ReadWaitTimeData.AverageValue}ms, Deviation {this.ReadWaitTimeData.StandardDeviation}ms / Round Trip Samples {this.WriteRoundtripTimeData.TotalSamples}, Max {this.WriteRoundtripTimeData.MaxValue}ms, Min {this.WriteRoundtripTimeData.MinValue}ms, Average {this.WriteRoundtripTimeData.AverageValue}ms, Deviation {this.WriteRoundtripTimeData.StandardDeviation}ms");
            this.logger.LogTrace($"Axis Timings: Request interval Samples {this.AxisTimeData.TotalSamples}, Max {this.AxisTimeData.MaxValue}ms, Min {this.AxisTimeData.MinValue}ms, Average {this.AxisTimeData.AverageValue}ms, Deviation {this.AxisTimeData.StandardDeviation}ms / Round Trip Samples {this.AxisIntervalTimeData.TotalSamples}, Max {this.AxisIntervalTimeData.MaxValue}ms, Min {this.AxisIntervalTimeData.MinValue}ms, Average {this.AxisIntervalTimeData.AverageValue}ms, Deviation {this.AxisIntervalTimeData.StandardDeviation}ms");
            this.logger.LogTrace($"Sensor Timings: Request interval Samples {this.SensorTimeData.TotalSamples}, Max {this.SensorTimeData.MaxValue}ms, Min {this.SensorTimeData.MinValue}ms, Average {this.SensorTimeData.AverageValue}ms, Deviation {this.SensorTimeData.StandardDeviation}ms / Round Trip Samples {this.SensorIntervalTimeData.TotalSamples}, Max {this.SensorIntervalTimeData.MaxValue}ms, Min {this.SensorIntervalTimeData.MinValue}ms, Average {this.SensorIntervalTimeData.AverageValue}ms, Deviation {this.SensorIntervalTimeData.StandardDeviation}ms");
        }

        private async Task ReceiveInverterData()
        {
            this.logger.LogTrace("1:Method Start");

            do
            {
                if (!this.socketTransport.IsConnected)
                {
                    try
                    {
                        this.receiveBuffer = null;
                        await this.socketTransport.ConnectAsync();
                    }
                    catch (InverterDriverException ex)
                    {
                        this.logger.LogError($"1: Exception {ex.Message}; Exception code={ex.InverterDriverExceptionCode}; Inner exception: {ex.InnerException.Message}");
                    }
                    catch (Exception ex)
                    {
                        this.logger.LogError($"2:Exception {ex.Message} while Connecting Receiver Socket Transport");

                        this.SendOperationErrorMessage(InverterIndex.MainInverter, new InverterExceptionFieldMessageData(ex, "while Connecting Receiver Socket Transport", 0), FieldMessageType.InverterException);
                        throw new InverterDriverException($"Exception {ex.Message} ReceiveInverterData Failed 1", ex);
                    }

                    if (!this.socketTransport.IsConnected)
                    {
                        this.logger.LogError("3:Socket Transport failed to connect");

                        var ex = new Exception();
                        this.SendOperationErrorMessage(InverterIndex.MainInverter, new InverterExceptionFieldMessageData(ex, "Socket Transport failed to connect", 0), FieldMessageType.InverterError);
                        continue;
                    }
                    else
                    {
                        this.logger.LogInformation($"3:Connection OK ipAddress={this.inverterAddress}:Port={this.inverterPort}");
                    }

                    this.writeEnableEvent.Set();
                }

                // socket connected
                byte[] inverterData;
                try
                {
                    this.readWaitStopwatch.Reset();
                    this.readWaitStopwatch.Start();

                    inverterData = await this.socketTransport.ReadAsync(this.stoppingToken);
                    if (inverterData == null || inverterData.Length == 0)
                    {
                        // connection error
                        this.logger.LogError($"2:Inverter message is null");
                        this.SendOperationErrorMessage(InverterIndex.MainInverter, new InverterExceptionFieldMessageData(null, "Inverter Driver Connection Error", 0), FieldMessageType.InverterException);
                        continue;
                    }
                    this.receiveBuffer = this.receiveBuffer.AppendArrays(inverterData, inverterData.Length);

                    this.readWaitStopwatch.Stop();
                    this.roundTripStopwatch.Stop();
                    this.readSpeedStopwatch.Stop();
                    this.ReadSpeedTimeData.AddValue(this.readSpeedStopwatch.ElapsedTicks);
                    this.readSpeedStopwatch.Reset();
                    this.readSpeedStopwatch.Start();
                    this.ReadWaitTimeData.AddValue(this.readWaitStopwatch.ElapsedTicks);
                    this.WriteRoundtripTimeData.AddValue(this.roundTripStopwatch.ElapsedTicks);
                }
                catch (OperationCanceledException)
                {
                    this.logger.LogDebug("2:Method End - operation cancelled");

                    return;
                }
                catch (InverterDriverException ex)
                {
                    this.logger.LogCritical($"2A: Exception {ex.Message}, InverterExceptionCode={ex.InverterDriverExceptionCode}");

                    this.SendOperationErrorMessage(InverterIndex.MainInverter, new InverterExceptionFieldMessageData(ex, "Inverter Driver Exception", (int)ex.InverterDriverExceptionCode), FieldMessageType.InverterException);

                    throw new InverterDriverException($"Exception {ex.Message} ReceiveInverterData Failed 2", ex);
                }
                catch (InvalidOperationException ex)
                {
                    // connection error
                    this.logger.LogError($"Exception {ex.Message}; InnerException {ex.InnerException?.Message ?? string.Empty}");
                    this.SendOperationErrorMessage(InverterIndex.MainInverter, new InverterExceptionFieldMessageData(ex, "Inverter Driver Connection Error", 0), FieldMessageType.InverterException);
                    continue;
                }
                catch (Exception ex)
                {
                    this.logger.LogCritical($"3:Exception: {ex.Message}");

                    this.SendOperationErrorMessage(InverterIndex.MainInverter, new InverterExceptionFieldMessageData(ex, "Inverter Driver Exeption", 0), FieldMessageType.InverterException);

                    throw new InverterDriverException($"Exception {ex.Message} ReceiveInverterData Failed 3", ex);
                }

                //INFO: Byte 1 of read data contains packet length
                if (this.receiveBuffer[1] == 0x00)
                {
                    // message error
                    this.logger.LogError($"5:Inverter message length is zero: received {BitConverter.ToString(inverterData)}: message {BitConverter.ToString(this.receiveBuffer)}");
                    this.SendOperationErrorMessage(InverterIndex.MainInverter, new InverterExceptionFieldMessageData(null, "Inverter Driver Connection Error", 0), FieldMessageType.InverterException);
                    this.socketTransport.Disconnect();
                    continue;
                }
                if (this.receiveBuffer.Length < 2 || this.receiveBuffer.Length < this.receiveBuffer[1] + 2)
                {
                    // this is not an error: we try to recover from messages received in more pieces
                    this.logger.LogTrace($"5:Inverter message is not complete: received {BitConverter.ToString(inverterData)}: message {BitConverter.ToString(this.receiveBuffer)}");
                    continue;
                }

                var extractedMessages = GetMessagesWithHeaderLengthToEnqueue(ref this.receiveBuffer, 4, 1, 2);
                if (extractedMessages.Count > 0)
                {
                    this.writeEnableEvent.Set();
                }

                foreach (var extractedMessage in extractedMessages)
                {
                    InverterMessage currentMessage;
                    try
                    {
                        currentMessage = new InverterMessage(extractedMessage);

                        this.logger.LogTrace($"6:currentMessage={currentMessage}");
                    }
                    catch (Exception ex)
                    {
                        this.logger.LogError($"7:Exception {ex.Message} while parsing Inverter raw message bytes {BitConverter.ToString(extractedMessage)}");

                        this.SendOperationErrorMessage(InverterIndex.None, new InverterExceptionFieldMessageData(ex, $"Exception {ex.Message} while parsing Inverter raw message bytes", 0), FieldMessageType.InverterException);

                        this.socketTransport.Disconnect();
                        break;
                    }

                    if (!Enum.TryParse(currentMessage.SystemIndex.ToString(), out InverterIndex inverterIndex))
                    {
                        this.logger.LogError($"8:Invalid system index {currentMessage.SystemIndex} defined in Inverter Message {BitConverter.ToString(extractedMessage)}");

                        var ex = new Exception();
                        this.SendOperationErrorMessage(InverterIndex.None, new InverterExceptionFieldMessageData(ex, $"Invalid system index {currentMessage.SystemIndex} defined in Inverter Message", 0), FieldMessageType.InverterError);

                        this.socketTransport.Disconnect();
                        break;
                    }

                    this.currentStateMachines.TryGetValue(inverterIndex, out var messageCurrentStateMachine);

                    if (currentMessage.IsWriteMessage)
                    {
                        this.logger.LogTrace("9:Evaluate Write Message");

                        this.EvaluateWriteMessage(currentMessage, inverterIndex, messageCurrentStateMachine);
                    }

                    if (currentMessage.IsReadMessage)
                    {
                        this.logger.LogTrace("10:Evaluate Read Message");

                        this.EvaluateReadMessage(currentMessage, inverterIndex, messageCurrentStateMachine);
                    }
                }
            }
            while (!this.stoppingToken.IsCancellationRequested);
        }

        private async Task SendInverterCommand()
        {
            //INFO Create WaitHandle array to wait for multiple events
            var commandHandles = new[]
            {
                this.heartbeatQueue.WaitHandle,
                this.inverterCommandQueue.WaitHandle
            };

            do
            {
                this.logger.LogTrace($"1:Heartbeat Queue Length: {this.heartbeatQueue.Count}, Command queue length: {this.inverterCommandQueue.Count}");

                if (this.inverterCommandQueue.Count > 2000 && Debugger.IsAttached)
                {
                    Debugger.Break();
                }

                if (this.socketTransport.IsConnected)
                {
                    int handleIndex;

                    if (this.heartbeatQueue.Count == 0 && this.inverterCommandQueue.Count == 0)
                    {
                        handleIndex = WaitHandle.WaitAny(commandHandles);
                    }
                    else
                    {
                        handleIndex = this.heartbeatQueue.Count > this.inverterCommandQueue.Count ? 0 : 1;
                    }

                    this.logger.LogTrace($"2:handleIndex={handleIndex} {Thread.CurrentThread.ManagedThreadId}");

                    if (this.writeEnableEvent.Wait(Timeout.Infinite, this.stoppingToken))
                    {
                        if (this.socketTransport.IsConnected)
                        {
                            this.writeEnableEvent.Reset();

                            var result = false;

                            switch (handleIndex)
                            {
                                case 0:
                                    result = await this.ProcessHeartbeat();
                                    break;

                                case 1:
                                    result = await this.ProcessInverterCommand();
                                    break;
                            }

                            if (!result)
                            {
                                this.writeEnableEvent.Set();
                            }
                        }
                    }
                }
                else
                {
                    Thread.Sleep(5);
                }
            }
            while (!this.stoppingToken.IsCancellationRequested);
        }

        private void SendOperationErrorMessage(InverterIndex inverterIndex, IFieldMessageData messageData, FieldMessageType type)
        {
            switch (type)
            {
                case FieldMessageType.InverterError:
                    var errorMsg = new FieldNotificationMessage(
                        messageData,
                        "Inverter Driver Error",
                       FieldMessageActor.Any,
                       FieldMessageActor.InverterDriver,
                       FieldMessageType.InverterError,
                       MessageStatus.OperationError,
                        (byte)inverterIndex,
                       ErrorLevel.Critical);
                    this.eventAggregator.GetEvent<FieldNotificationEvent>().Publish(errorMsg);
                    break;

                case FieldMessageType.InverterException:
                    var exceptionMsg = new FieldNotificationMessage(
                     messageData,
                     "Inverter Driver Exception",
                    FieldMessageActor.Any,
                    FieldMessageActor.InverterDriver,
                    FieldMessageType.InverterException,
                    MessageStatus.OperationError,
                     (byte)inverterIndex,
                    ErrorLevel.Critical);
                    this.eventAggregator.GetEvent<FieldNotificationEvent>().Publish(exceptionMsg);
                    break;

                case FieldMessageType.CalibrateAxis:
                    if (messageData is ICalibrateAxisFieldMessageData calibrateData)
                    {
                        var calibrateErrorNotification = new FieldNotificationMessage(
                        calibrateData,
                        "Requested Inverter is not configured",
                        FieldMessageActor.Any,
                        FieldMessageActor.InverterDriver,
                        FieldMessageType.CalibrateAxis,
                        MessageStatus.OperationError,
                        (byte)inverterIndex,
                        ErrorLevel.Critical);
                        this.eventAggregator?.GetEvent<FieldNotificationEvent>().Publish(calibrateErrorNotification);
                    }
                    break;

                case FieldMessageType.InverterSwitchOff:
                    if (messageData is IInverterSwitchOffFieldMessageData switchOffData)
                    {
                        var inverterSwitchOffErrorNotification = new FieldNotificationMessage(
                        switchOffData,
                        $"Inverter status not configured for requested inverter {inverterIndex}",
                        FieldMessageActor.Any,
                        FieldMessageActor.InverterDriver,
                        FieldMessageType.InverterSwitchOff,
                        MessageStatus.OperationError,
                        (byte)inverterIndex,
                        ErrorLevel.Critical);
                        this.eventAggregator?.GetEvent<FieldNotificationEvent>().Publish(inverterSwitchOffErrorNotification);
                    }
                    break;

                case FieldMessageType.InverterSwitchOn:
                    if (messageData is IInverterSwitchOnFieldMessageData switchOnData)
                    {
                        var inverterSwitchOnErrorNotification = new FieldNotificationMessage(
                        switchOnData,
                        $"Inverter status not configured for requested inverter {inverterIndex}",
                        FieldMessageActor.Any,
                        FieldMessageActor.InverterDriver,
                        FieldMessageType.InverterSwitchOn,
                        MessageStatus.OperationError,
                        (byte)inverterIndex,
                        ErrorLevel.Critical);
                        this.eventAggregator?.GetEvent<FieldNotificationEvent>().Publish(inverterSwitchOnErrorNotification);
                    }
                    break;

                case FieldMessageType.Positioning:

                    if (messageData is IPositioningFieldMessageData positioningData)
                    {
                        var positioningErrorNotification = new FieldNotificationMessage(
                        positioningData,
                        "Requested Inverter is not configured",
                        FieldMessageActor.Any,
                        FieldMessageActor.InverterDriver,
                        FieldMessageType.Positioning,
                        MessageStatus.OperationError,
                        (byte)inverterIndex,
                        ErrorLevel.Critical);
                        this.eventAggregator?.GetEvent<FieldNotificationEvent>().Publish(positioningErrorNotification);
                    }
                    break;

                case FieldMessageType.InverterPowerOff:
                    if (messageData is IInverterPowerOffFieldMessageData powerOffData)
                    {
                        var inverterPowerOfferrorNotification = new FieldNotificationMessage(
                        powerOffData,
                        "Requested Inverter is not configured",
                        FieldMessageActor.Any,
                        FieldMessageActor.InverterDriver,
                        FieldMessageType.InverterPowerOff,
                        MessageStatus.OperationError,
                        (byte)inverterIndex,
                        ErrorLevel.Critical);
                        this.eventAggregator?.GetEvent<FieldNotificationEvent>().Publish(inverterPowerOfferrorNotification);
                    }
                    break;

                case FieldMessageType.InverterPowerOn:
                    if (messageData is IInverterPowerOnFieldMessageData powerOnData)
                    {
                        var inverterPowerOnerrorNotification = new FieldNotificationMessage(
                        powerOnData,
                        "Requested Inverter is not configured",
                        FieldMessageActor.Any,
                        FieldMessageActor.InverterDriver,
                        FieldMessageType.InverterPowerOn,
                        MessageStatus.OperationError,
                        (byte)inverterIndex,
                        ErrorLevel.Critical);
                        this.eventAggregator?.GetEvent<FieldNotificationEvent>().Publish(inverterPowerOnerrorNotification);
                    }
                    break;

                case FieldMessageType.ShutterPositioning:
                    if (messageData is IShutterPositioningFieldMessageData shutterPositioningData)
                    {
                        var shutterPositioningErrorNotification = new FieldNotificationMessage(
                        shutterPositioningData,
                        "Requested Inverter is not configured",
                        FieldMessageActor.Any,
                        FieldMessageActor.InverterDriver,
                        FieldMessageType.ShutterPositioning,
                        MessageStatus.OperationError,
                        (byte)inverterIndex,
                        ErrorLevel.Critical);
                        this.eventAggregator?.GetEvent<FieldNotificationEvent>().Publish(shutterPositioningErrorNotification);
                    }
                    break;

                case FieldMessageType.InverterStop:
                    if (messageData is IInverterStopFieldMessageData stopData)
                    {
                        var inverterStopErrorNotification = new FieldNotificationMessage(
                       stopData,
                       $"Inverter status not configured for requested inverter {inverterIndex}",
                       FieldMessageActor.Any,
                       FieldMessageActor.InverterDriver,
                       FieldMessageType.InverterStop,
                       MessageStatus.OperationError,
                       (byte)inverterIndex,
                       ErrorLevel.Critical);
                        this.eventAggregator?.GetEvent<FieldNotificationEvent>().Publish(inverterStopErrorNotification);
                    }
                    break;

                case FieldMessageType.InverterStatusUpdate:
                    if (messageData is IInverterStatusUpdateFieldMessageData updateData)
                    {
                        var inverterUpdateStatusErrorNotification = new FieldNotificationMessage(
                        updateData,
                        "Wrong message Data data type",
                        FieldMessageActor.Any,
                        FieldMessageActor.InverterDriver,
                        FieldMessageType.InverterStatusUpdate,
                        MessageStatus.OperationError,
                        (byte)inverterIndex,
                        ErrorLevel.Critical);
                        this.eventAggregator?.GetEvent<FieldNotificationEvent>().Publish(inverterUpdateStatusErrorNotification);
                    }
                    break;

                case FieldMessageType.InverterSetTimer:
                    if (messageData is IInverterSetTimerFieldMessageData setTimerData)
                    {
                        var inverterUpdateStatusErrorNotification = new FieldNotificationMessage(
                        setTimerData,
                        "Wrong message Data data type",
                        FieldMessageActor.Any,
                        FieldMessageActor.InverterDriver,
                        FieldMessageType.InverterSetTimer,
                        MessageStatus.OperationError,
                        (byte)inverterIndex,
                        ErrorLevel.Critical);
                        this.eventAggregator?.GetEvent<FieldNotificationEvent>().Publish(inverterUpdateStatusErrorNotification);
                    }
                    break;
            }
        }

        #endregion
    }
}
