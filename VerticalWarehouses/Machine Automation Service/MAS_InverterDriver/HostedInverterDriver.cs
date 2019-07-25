using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Prism.Events;
using static Ferretto.VW.MAS_InverterDriver.BufferUtility;

// ReSharper disable ArrangeThisQualifier
// ReSharper disable ParameterHidesMember
namespace Ferretto.VW.MAS.InverterDriver
{
    public partial class HostedInverterDriver : BackgroundService
    {
        #region Fields

        private const int AXIS_POSITION_UPDATE_INTERVAL = 100;

        private const int HEARTBEAT_TIMEOUT = 300;   // 300

        private const int SENSOR_STATUS_UPDATE_INTERVAL = 500;

        private readonly Stopwatch axisIntervalStopwatch;

        private readonly Stopwatch axisStopwatch;

        private readonly BlockingConcurrentQueue<FieldCommandMessage> commandQueue;

        private readonly Task commandReceiveTask;

        private readonly IConfigurationValueManagmentDataLayer dataLayerConfigurationValueManagement;

        private readonly IResolutionConversion dataLayerResolutionConversion;

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

        private readonly ISocketTransport socketTransport;

        private readonly IVertimagConfiguration vertimagConfiguration;

        private readonly ManualResetEventSlim writeEnableEvent;

        private Timer axisPositionUpdateTimer;

        private Axis currentAxis;

        private IInverterStateMachine currentStateMachine;

        private bool disposed;

        private bool forceStatusPublish;

        private Timer heartBeatTimer;

        private InverterIndex inverterIndexToStop;

        private Timer sensorStatusUpdateTimer;

        // index of inverter to Stop
        private int shaftPositionUpdateNumberOfTimes;

        private CancellationToken stoppingToken;

        private byte[] ReceiveBuffer;

        #endregion

        #region Constructors

        public HostedInverterDriver(
            IEventAggregator eventAggregator,
            ISocketTransport socketTransport,
            IConfigurationValueManagmentDataLayer dataLayerConfigurationValueManagement,
            IResolutionConversion dataLayerResolutionConversion,
            IVertimagConfiguration vertimagConfiguration,
            ILogger<HostedInverterDriver> logger)
        {
            this.socketTransport = socketTransport;
            this.eventAggregator = eventAggregator;
            this.dataLayerConfigurationValueManagement = dataLayerConfigurationValueManagement;
            this.dataLayerResolutionConversion = dataLayerResolutionConversion;
            this.vertimagConfiguration = vertimagConfiguration;
            this.logger = logger;

            this.inverterIndexToStop = InverterIndex.MainInverter;

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
        }

        #endregion

        #region Destructors

        ~HostedInverterDriver()
        {
            this.Dispose(false);
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

        private IInverterStateMachine CurrentStateMachine
        {
            get => this.currentStateMachine;
            set
            {
                if (this.currentStateMachine != value)
                {
                    this.currentStateMachine?.Dispose();
                    this.currentStateMachine = value;
                }
            }
        }

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
                this.axisPositionUpdateTimer?.Dispose();
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

                this.SendOperationErrorMessage(new InverterExceptionFieldMessageData(ex, "Inverter Driver Exception", 0), FieldMessageType.InverterException);
            }

            return Task.CompletedTask;
        }

        private void CommandReceiveTaskFunction()
        {
            this.sensorStatusUpdateTimer?.Dispose();
            this.sensorStatusUpdateTimer = new Timer(this.RequestSensorStatusUpdate, null, -1, Timeout.Infinite);

            this.axisPositionUpdateTimer?.Dispose();
            this.axisPositionUpdateTimer = new Timer(this.RequestAxisPositionUpdate, null, -1, Timeout.Infinite);

            do
            {
                FieldCommandMessage receivedMessage;
                try
                {
                    this.commandQueue.TryDequeue(Timeout.Infinite, this.stoppingToken, out receivedMessage);

                    this.logger.LogTrace($"1:Command received: {receivedMessage.Type}, destination: {receivedMessage.Destination}, source: {receivedMessage.Source}");
                }
                catch (OperationCanceledException)
                {
                    this.logger.LogDebug("2:Method End operation cancelled");

                    return;
                }
                catch (Exception ex)
                {
                    this.logger.LogDebug($"3:Exception: {ex.Message}");

                    this.SendOperationErrorMessage(new InverterExceptionFieldMessageData(ex, "Inverter Driver Exception", 0), FieldMessageType.InverterException);

                    return;
                }

                if (this.inverterStatuses.Count == 0)
                {
                    this.logger.LogTrace("4:Invert Driver not configured for this message Type");

                    var ex = new Exception();
                    this.SendOperationErrorMessage(new InverterExceptionFieldMessageData(ex, "Invert Driver not configured for this message Type", 0), FieldMessageType.InverterError);

                    continue;
                }

                //if (receivedMessage.Type == FieldMessageType.InverterStop)
                //{
                //    this.currentStateMachine?.Dispose();

                //    this.ProcessStopMessage(receivedMessage);

                //    continue;
                //}

                if (this.CurrentStateMachine != null && receivedMessage.Type == FieldMessageType.InverterStop)
                {
                    if (receivedMessage.Data is InverterStopFieldMessageData stopMessageData)
                    {
                        this.inverterIndexToStop = stopMessageData.InverterToStop;
                    }
                    this.logger.LogTrace("4: Stop the timer for update shaft position");
                    this.axisPositionUpdateTimer.Change(Timeout.Infinite, Timeout.Infinite);

                    this.CurrentStateMachine?.Stop();

                    continue;
                }

                if (this.CurrentStateMachine != null)
                {
                    this.logger.LogTrace($"5:Inverter Driver already executing operation {this.CurrentStateMachine.GetType()}");

                    var ex = new Exception();
                    this.SendOperationErrorMessage(new InverterExceptionFieldMessageData(ex, "Inverter operation already in progress", 0), FieldMessageType.InverterError);

                    continue;
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
                        this.ProcessPositioningMessage(receivedMessage);
                        break;

                    case FieldMessageType.ShutterPositioning:
                        this.ProcessShutterPositioningMessage(receivedMessage);
                        break;

                    case FieldMessageType.InverterStatusUpdate:
                        this.ProcessInverterStatusUpdateMessage(receivedMessage);
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
                }

                this.logger.LogTrace($"Socket Timings: Read Wait Samples {this.ReadWaitTimeData.TotalSamples}, Max {this.ReadWaitTimeData.MaxValue}ms, Min {this.ReadWaitTimeData.MinValue}ms, Average {this.ReadWaitTimeData.AverageValue}ms, Deviation {this.ReadWaitTimeData.StandardDeviation}ms / Round Trip Samples {this.WriteRoundtripTimeData.TotalSamples}, Max {this.WriteRoundtripTimeData.MaxValue}ms, Min {this.WriteRoundtripTimeData.MinValue}ms, Average {this.WriteRoundtripTimeData.AverageValue}ms, Deviation {this.WriteRoundtripTimeData.StandardDeviation}ms");
                this.logger.LogTrace($"Axis Timings: Request interval Samples {this.AxisTimeData.TotalSamples}, Max {this.AxisTimeData.MaxValue}ms, Min {this.AxisTimeData.MinValue}ms, Average {this.AxisTimeData.AverageValue}ms, Deviation {this.AxisTimeData.StandardDeviation}ms / Round Trip Samples {this.AxisIntervalTimeData.TotalSamples}, Max {this.AxisIntervalTimeData.MaxValue}ms, Min {this.AxisIntervalTimeData.MinValue}ms, Average {this.AxisIntervalTimeData.AverageValue}ms, Deviation {this.AxisIntervalTimeData.StandardDeviation}ms");
                this.logger.LogTrace($"Sensor Timings: Request interval Samples {this.SensorTimeData.TotalSamples}, Max {this.SensorTimeData.MaxValue}ms, Min {this.SensorTimeData.MinValue}ms, Average {this.SensorTimeData.AverageValue}ms, Deviation {this.SensorTimeData.StandardDeviation}ms / Round Trip Samples {this.SensorIntervalTimeData.TotalSamples}, Max {this.SensorIntervalTimeData.MaxValue}ms, Min {this.SensorIntervalTimeData.MinValue}ms, Average {this.SensorIntervalTimeData.AverageValue}ms, Deviation {this.SensorIntervalTimeData.StandardDeviation}ms");
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

                    this.SendOperationErrorMessage(new InverterExceptionFieldMessageData(ex, "Inverter Driver Exception", 0), FieldMessageType.InverterException);

                    return;
                }

                switch (receivedMessage.Type)
                {
                    //case FieldMessageType.InverterError:
                    case FieldMessageType.DataLayerReady:

                        await this.StartHardwareCommunications();
                        this.InitializeInverterStatus();

                        break;

                    case FieldMessageType.Positioning:
                        {
                            if (receivedMessage.Status == MessageStatus.OperationEnd)
                            {
                                this.logger.LogDebug($"Positioning Deallocating {this.CurrentStateMachine?.GetType()} state machine");
                                this.logger.LogTrace($"4:Deallocation SM {this.CurrentStateMachine?.GetType()}");

                                if (this.CurrentStateMachine is PositioningStateMachine)
                                {
                                    this.CurrentStateMachine = null;
                                }
                                else
                                {
                                    this.logger.LogDebug($"Try to deallocate {this.CurrentStateMachine?.GetType()} Handling {receivedMessage.Type}");
                                }

                                this.logger.LogTrace("4: Stop the timer for update shaft position");
                                this.axisPositionUpdateTimer.Change(Timeout.Infinite, Timeout.Infinite);
                            }

                            if (receivedMessage.Status == MessageStatus.OperationStop)
                            {
                                this.logger.LogTrace($"5:Deallocation SM {this.CurrentStateMachine?.GetType()}");

                                if (this.CurrentStateMachine is PositioningStateMachine)
                                {
                                    this.CurrentStateMachine = null;
                                }
                                else
                                {
                                    this.logger.LogDebug($"Try to deallocate {this.CurrentStateMachine?.GetType()} Handling {receivedMessage.Type}");
                                }

                                this.logger.LogTrace("4: Stop the timer for update shaft position");
                                this.axisPositionUpdateTimer.Change(Timeout.Infinite, Timeout.Infinite);

                                // Enqueue a message to execute the Stop states machine
                                var stopMessageData = new InverterStopFieldMessageData(this.inverterIndexToStop);
                                var stopMessage = new FieldCommandMessage(
                                    stopMessageData,
                                    "Stop inverter",
                                    FieldMessageActor.InverterDriver,
                                    FieldMessageActor.InverterDriver,
                                    FieldMessageType.InverterStop);
                                if (stopMessage != null)
                                {
                                    this.commandQueue.Enqueue(stopMessage);
                                }
                            }

                            break;
                        }
                    case FieldMessageType.CalibrateAxis:

                        if (receivedMessage.Status == MessageStatus.OperationEnd)
                        {
                            this.logger.LogDebug($"CalibrateAxis Deallocating {this.CurrentStateMachine?.GetType()} state machine");

                            if (this.CurrentStateMachine is CalibrateAxisStateMachine)
                            {
                                this.CurrentStateMachine = null;
                            }
                            else
                            {
                                this.logger.LogDebug($"Try to deallocate {this.CurrentStateMachine?.GetType()} Handling {receivedMessage.Type}");
                            }
                        }
                        if (receivedMessage.Status == MessageStatus.OperationStop)
                        {
                            if (this.CurrentStateMachine is CalibrateAxisStateMachine)
                            {
                                this.CurrentStateMachine = null;
                            }
                            else
                            {
                                this.logger.LogDebug($"Try to deallocate {this.CurrentStateMachine?.GetType()} Handling {receivedMessage.Type}");
                            }

                            // Enqueue a message to execute the Stop states machine
                            var stopMessageData = new InverterStopFieldMessageData(this.inverterIndexToStop);
                            var stopMessage = new FieldCommandMessage(
                                stopMessageData,
                                "Stop inverter",
                                FieldMessageActor.InverterDriver,
                                FieldMessageActor.InverterDriver,
                                FieldMessageType.InverterStop);
                            if (stopMessage != null)
                            {
                                this.commandQueue.Enqueue(stopMessage);
                            }
                        }

                        break;

                    case FieldMessageType.ShutterPositioning:

                        this.logger.LogDebug($"ShutterPositioning Deallocating {this.CurrentStateMachine?.GetType()} state machine");
                        if (receivedMessage.Status == MessageStatus.OperationEnd)
                        {
                            if (this.CurrentStateMachine is ShutterPositioningStateMachine)
                            {
                                this.CurrentStateMachine = null;
                            }
                            else
                            {
                                this.logger.LogDebug($"Try to deallocate {this.CurrentStateMachine?.GetType()} Handling {receivedMessage.Type}");
                            }
                        }
                        if (receivedMessage.Status == MessageStatus.OperationStop)
                        {
                            if (this.CurrentStateMachine is ShutterPositioningStateMachine)
                            {
                                this.CurrentStateMachine = null;
                            }
                            else
                            {
                                this.logger.LogDebug($"Try to deallocate {this.CurrentStateMachine?.GetType()} Handling {receivedMessage.Type}");
                            }

                            // Enqueue a message to execute the Stop states machine
                            var stopMessageData = new InverterStopFieldMessageData(this.inverterIndexToStop);
                            var stopMessage = new FieldCommandMessage(
                                stopMessageData,
                                "Stop inverter",
                                FieldMessageActor.InverterDriver,
                                FieldMessageActor.InverterDriver,
                                FieldMessageType.InverterStop);
                            if (stopMessage != null)
                            {
                                this.commandQueue.Enqueue(stopMessage);
                            }
                        }

                        break;

                    case FieldMessageType.InverterPowerOff:
                    case FieldMessageType.InverterSwitchOn:
                    case FieldMessageType.InverterStop:

                        this.logger.LogDebug($"Deallocating {this.CurrentStateMachine?.GetType()} state machine ({receivedMessage.Type})");
                        if (receivedMessage.Status == MessageStatus.OperationEnd)
                        {
                            if (this.CurrentStateMachine is null)
                            {
                                this.logger.LogDebug($"State machine {this.CurrentStateMachine?.GetType()} is null !!");
                            }

                            if (this.CurrentStateMachine is PowerOffStateMachine ||
                                this.CurrentStateMachine is SwitchOnStateMachine ||
                                this.CurrentStateMachine is StopStateMachine)
                            {
                                this.CurrentStateMachine = null;
                            }
                            else
                            {
                                this.logger.LogDebug($"Try to deallocate {this.CurrentStateMachine?.GetType()} Handling {receivedMessage.Type}");
                            }
                        }

                        break;

                    case FieldMessageType.InverterSwitchOff:
                        if (receivedMessage.Status == MessageStatus.OperationEnd)
                        {
                            this.logger.LogDebug($"InverterSwitchOff Deallocating {this.CurrentStateMachine?.GetType()} state machine");

                            if (this.CurrentStateMachine is SwitchOffStateMachine)
                            {
                                this.CurrentStateMachine = null;
                            }
                            else
                            {
                                this.logger.LogDebug($"Try to deallocate {this.CurrentStateMachine?.GetType()} Handling {receivedMessage.Type}");
                            }

                            var nextMessage = ((InverterSwitchOffFieldMessageData)receivedMessage.Data).NextCommandMessage;
                            if (nextMessage != null)
                            {
                                this.commandQueue.Enqueue(nextMessage);
                            }
                        }

                        break;

                    case FieldMessageType.InverterPowerOn:

                        if (receivedMessage.Status == MessageStatus.OperationEnd)
                        {
                            this.logger.LogDebug($"InverterPowerOn Deallocating {this.CurrentStateMachine?.GetType()} state machine");

                            if (this.CurrentStateMachine is PowerOnStateMachine)
                            {
                                this.CurrentStateMachine = null;
                            }
                            else
                            {
                                this.logger.LogDebug($"Try to deallocate {this.CurrentStateMachine?.GetType()} Handling {receivedMessage.Type}");
                            }

                            var nextMessage = ((InverterPowerOnFieldMessageData)receivedMessage.Data).NextCommandMessage;
                            if (nextMessage != null)
                            {
                                this.commandQueue.Enqueue(nextMessage);
                            }
                        }

                        break;
                }

                if (receivedMessage.Source == FieldMessageActor.InverterDriver)
                {
                    if (receivedMessage.Status == MessageStatus.OperationEnd ||
                        receivedMessage.Status == MessageStatus.OperationStop)
                    {
                        var notificationMessageToFSM = receivedMessage;
                        //TEMP Set the destination of message to FSM
                        notificationMessageToFSM.Destination = FieldMessageActor.FiniteStateMachines;

                        this.eventAggregator?.GetEvent<FieldNotificationEvent>().Publish(notificationMessageToFSM);
                    }
                }
            }
            while (!this.stoppingToken.IsCancellationRequested);
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
                        await this.socketTransport.ConnectAsync();
                    }
                    catch (InverterDriverException ex)
                    {
                        this.logger.LogError($"1: Exception {ex.Message}; Exception code={ex.InverterDriverExceptionCode}");
                    }
                    catch (Exception ex)
                    {
                        this.logger.LogError($"2:Exception {ex.Message} while Connecting Receiver Socket Transport");

                        this.SendOperationErrorMessage(new InverterExceptionFieldMessageData(ex, "while Connecting Receiver Socket Transport", 0), FieldMessageType.InverterException);
                    }

                    if (!this.socketTransport.IsConnected)
                    {
                        this.logger.LogError("3:Socket Transport failed to connect");

                        var ex = new Exception();
                        this.SendOperationErrorMessage(new InverterExceptionFieldMessageData(ex, "Socket Transport failed to connect", 0), FieldMessageType.InverterError);
                        continue;
                    }
                }
                byte[] inverterData;
                try
                {
                    this.readWaitStopwatch.Reset();
                    this.readWaitStopwatch.Start();

                    inverterData = await this.socketTransport.ReadAsync(this.stoppingToken);
                    this.ReceiveBuffer = this.ReceiveBuffer.AppendArrays(inverterData, inverterData.Length);

                    this.readWaitStopwatch.Stop();
                    this.roundTripStopwatch.Stop();
                    this.readSpeedStopwatch.Stop();
                    this.ReadSpeedTimeData.AddValue(this.readSpeedStopwatch.ElapsedTicks);
                    this.readSpeedStopwatch.Reset();
                    this.readSpeedStopwatch.Start();
                    this.ReadWaitTimeData.AddValue(this.readWaitStopwatch.ElapsedTicks);
                    this.WriteRoundtripTimeData.AddValue(this.roundTripStopwatch.ElapsedTicks);

                    this.writeEnableEvent.Set();
                }
                catch (OperationCanceledException)
                {
                    this.logger.LogDebug("2:Method End operation cancelled");

                    this.writeEnableEvent.Set();
                    return;
                }
                catch (InverterDriverException ex)
                {
                    this.logger.LogCritical($"2A: Exception {ex.Message}, InverterExceptionCode={ex.InverterDriverExceptionCode}");

                    this.SendOperationErrorMessage(new InverterExceptionFieldMessageData(ex, "Inverter Driver Exception", (int)ex.InverterDriverExceptionCode), FieldMessageType.InverterException);

                    this.writeEnableEvent.Set();
                    return;
                }
                catch(InvalidOperationException ex)
                {
                    // connection error
                    this.SendOperationErrorMessage(new InverterExceptionFieldMessageData(ex, "Inverter Driver Connection Error", 0), FieldMessageType.InverterException);
                    continue;
                }
                catch (Exception ex)
                {
                    this.logger.LogDebug($"3:Exception: {ex.Message}");

                    this.SendOperationErrorMessage(new InverterExceptionFieldMessageData(ex, "Inverter Driver Exeption", 0), FieldMessageType.InverterException);

                    this.writeEnableEvent.Set();
                    return;
                }

                //INFO: Byte 1 of read data contains packet length, zero means invalid packet
                if (inverterData == null)
                {
                    this.logger.LogError($"4:Inverter message is null");
                    this.ReceiveBuffer = null;
                    this.socketTransport.Disconnect();
                    continue;
                }
                if (this.ReceiveBuffer[1] == 0x00)
                {
                    this.logger.LogError($"5:Inverter message length is zero");
                    this.ReceiveBuffer = null;
                    this.socketTransport.Disconnect();
                    continue;
                }
                if(this.ReceiveBuffer.Length < this.ReceiveBuffer[1] + 2)
                {
                    this.logger.LogTrace($"5:Inverter message is not complete");
                    continue;
                }

                var ExtractedMessages = GetMessagesWithHeaderLengthToEnqueue(ref this.ReceiveBuffer);
                foreach( var ExtractedMessage in ExtractedMessages.Where( x => (x[1] + 2 ) >= x.Length))
                {
                    InverterMessage currentMessage;
                    try
                    {
                        currentMessage = new InverterMessage(ExtractedMessage);

                        this.logger.LogTrace($"6:currentMessage={currentMessage}");
                    }
                    catch (InverterDriverException)
                    {
                        this.ReceiveBuffer = null;
                        this.socketTransport.Disconnect();
                        break;
                    }
                    catch (Exception ex)
                    {
                        this.logger.LogTrace($"7:Exception {ex.Message} while parsing Inverter raw message bytes");

                        this.SendOperationErrorMessage(new InverterExceptionFieldMessageData(ex, $"Exception {ex.Message} while parsing Inverter raw message bytes", 0), FieldMessageType.InverterException);

                        this.ReceiveBuffer = null;
                        this.socketTransport.Disconnect();
                        break;
                    }

                    if (!Enum.TryParse(currentMessage.SystemIndex.ToString(), out InverterIndex inverterIndex))
                    {
                        this.logger.LogTrace($"8:Invalid system index {currentMessage.SystemIndex} defined in Inverter Message");

                        var ex = new Exception();
                        this.SendOperationErrorMessage(new InverterExceptionFieldMessageData(ex, $"Invalid system index {currentMessage.SystemIndex} defined in Inverter Message", 0), FieldMessageType.InverterError);

                        this.ReceiveBuffer = null;
                        this.socketTransport.Disconnect();
                        break;
                    }

                    if (currentMessage.IsWriteMessage)
                    {
                        this.logger.LogTrace("9:Evaluate Write Message");

                        this.EvaluateWriteMessage(currentMessage, inverterIndex);
                    }

                    if (currentMessage.IsReadMessage)
                    {
                        this.logger.LogTrace("10:Evaluate Read Message");

                        this.EvaluateReadMessage(currentMessage, inverterIndex);
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
                int handleIndex;

                this.logger.LogTrace($"1:Heartbeat Queue Length: {this.heartbeatQueue.Count}, Command queue length: {this.inverterCommandQueue.Count}");

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
                    this.writeEnableEvent.Reset();

                    //switch (handleIndex)
                    //{
                    //    case 0:
                    //        await this.ProcessHeartbeat();
                    //        break;

                    //    case 1:
                    //        await this.ProcessInverterCommand();
                    //        break;
                    //}
                    if (this.socketTransport.IsConnected && this.socketTransport.IsReadingOk
                        )
                    {
                        await this.ProcessInverterCommand();
                    }
                }
            }
            while (!this.stoppingToken.IsCancellationRequested);
        }

        private void SendOperationErrorMessage(IFieldMessageData messageData, FieldMessageType type)
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
                        ErrorLevel.Critical);
                        this.eventAggregator?.GetEvent<FieldNotificationEvent>().Publish(calibrateErrorNotification);
                    }
                    break;

                case FieldMessageType.InverterSwitchOff:
                    if (messageData is IInverterSwitchOffFieldMessageData switchOffData)
                    {
                        var inverterSwitchOffErrorNotification = new FieldNotificationMessage(
                        switchOffData,
                        $"Inverter status not configured for requested inverter {switchOffData.SystemIndex}",
                        FieldMessageActor.Any,
                        FieldMessageActor.InverterDriver,
                        FieldMessageType.InverterSwitchOff,
                        MessageStatus.OperationError,
                        ErrorLevel.Critical);
                        this.eventAggregator?.GetEvent<FieldNotificationEvent>().Publish(inverterSwitchOffErrorNotification);
                    }
                    break;

                case FieldMessageType.InverterSwitchOn:
                    if (messageData is IInverterSwitchOnFieldMessageData switchOnData)
                    {
                        var inverterSwitchOnErrorNotification = new FieldNotificationMessage(
                        switchOnData,
                        $"Inverter status not configured for requested inverter {switchOnData.SystemIndex}",
                        FieldMessageActor.Any,
                        FieldMessageActor.InverterDriver,
                        FieldMessageType.InverterSwitchOn,
                        MessageStatus.OperationError,
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
                        ErrorLevel.Critical);
                        this.eventAggregator?.GetEvent<FieldNotificationEvent>().Publish(shutterPositioningErrorNotification);
                    }
                    break;

                case FieldMessageType.InverterStop:
                    if (messageData is IInverterStopFieldMessageData stopData)
                    {
                        var inverterStopErrorNotification = new FieldNotificationMessage(
                       stopData,
                       $"Inverter status not configured for requested inverter {stopData.InverterToStop}",
                       FieldMessageActor.Any,
                       FieldMessageActor.InverterDriver,
                       FieldMessageType.InverterStop,
                       MessageStatus.OperationError,
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
                        ErrorLevel.Critical);
                        this.eventAggregator?.GetEvent<FieldNotificationEvent>().Publish(inverterUpdateStatusErrorNotification);
                    }
                    break;
            }
        }

        #endregion
    }
}
