using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Ferretto.VW.Common_Utils.Messages.Enumerations;
using Ferretto.VW.MAS_DataLayer.Interfaces;
using Ferretto.VW.MAS_InverterDriver.Diagnostics;
using Ferretto.VW.MAS_InverterDriver.Interface;
using Ferretto.VW.MAS_InverterDriver.Interface.StateMachines;
using Ferretto.VW.MAS_InverterDriver.InverterStatus.Interfaces;
using Ferretto.VW.MAS_Utils.Enumerations;
using Ferretto.VW.MAS_Utils.Events;
using Ferretto.VW.MAS_Utils.Exceptions;
using Ferretto.VW.MAS_Utils.Messages;
using Ferretto.VW.MAS_Utils.Messages.FieldData;
using Ferretto.VW.MAS_Utils.Messages.FieldInterfaces;
using Ferretto.VW.MAS_Utils.Utilities;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Prism.Events;

// ReSharper disable ArrangeThisQualifier
// ReSharper disable ParameterHidesMember
namespace Ferretto.VW.MAS_InverterDriver
{
    public partial class HostedInverterDriver : BackgroundService
    {
        #region Fields

        private const int AXIS_POSITION_UPDATE_INTERVAL = 50;

        private const int HEARTBEAT_TIMEOUT = 300;   // 300

        private const int SENSOR_STATUS_UPDATE_INTERVAL = 500;

        private readonly BlockingConcurrentQueue<FieldCommandMessage> commandQueue;

        private readonly Task commandReceiveTask;

        private readonly IDataLayerConfigurationValueManagment dataLayerConfigurationValueManagement;

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

        private readonly ISocketTransport socketTransport;

        private readonly IVertimagConfiguration vertimagConfiguration;

        private readonly ManualResetEventSlim writeEnableEvent;

        private Timer axisPositionUpdateTimer;

        private Axis currentAxis;

        private IInverterStateMachine currentStateMachine;

        private bool disposed;

        private bool forceStatusPublish;

        private Timer heartBeatTimer;

        private Timer sensorStatusUpdateTimer;

        private int shaftPositionUpdateNumberOfTimes;

        private CancellationToken stoppingToken;

        #endregion

        #region Constructors

        public HostedInverterDriver(
            IEventAggregator eventAggregator,
            ISocketTransport socketTransport,
            IDataLayerConfigurationValueManagment dataLayerConfigurationValueManagement,
            IVertimagConfiguration vertimagConfiguration,
            ILogger<HostedInverterDriver> logger)
        {
            this.socketTransport = socketTransport;
            this.eventAggregator = eventAggregator;
            this.dataLayerConfigurationValueManagement = dataLayerConfigurationValueManagement;
            this.vertimagConfiguration = vertimagConfiguration;
            this.logger = logger;

            this.readWaitStopwatch = new Stopwatch();

            this.readSpeedStopwatch = new Stopwatch();

            this.roundTripStopwatch = new Stopwatch();

            this.ReadWaitTimeData = new InverterDiagnosticsData();

            this.ReadSpeadTimeData = new InverterDiagnosticsData();

            this.WriteRoundtripTimeData = new InverterDiagnosticsData();

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

        public InverterDiagnosticsData ReadSpeadTimeData { get; }

        public InverterDiagnosticsData ReadWaitTimeData { get; }

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

                if (receivedMessage.Type == FieldMessageType.InverterStop)
                {
                    this.currentStateMachine?.Release();

                    this.currentStateMachine?.Dispose();

                    this.logger.LogTrace("4: Stop the timer for update shaft position");
                    this.axisPositionUpdateTimer.Change(Timeout.Infinite, Timeout.Infinite);

                    this.ProcessStopMessage(receivedMessage);

                    continue;
                }

                if (this.currentStateMachine != null)
                {
                    this.logger.LogTrace($"5:Inverter Driver already executing operation {this.currentStateMachine.GetType()}");

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

                    this.SendOperationErrorMessage(new InverterExceptionFieldMessageData(ex, "Inverter Driver Exception", 0), FieldMessageType.InverterException);

                    return;
                }

                switch (receivedMessage.Type)
                {
                    case FieldMessageType.DataLayerReady:

                        await this.StartHardwareCommunications();
                        await this.InitializeInverterStatus();

                        break;

                    case FieldMessageType.Positioning:
                        {
                            if (receivedMessage.Status == MessageStatus.OperationEnd)
                            {
                                this.currentStateMachine?.Dispose();
                                this.currentStateMachine = null;

                                this.logger.LogTrace("4: Stop the timer for update shaft position");
                                this.axisPositionUpdateTimer.Change(Timeout.Infinite, Timeout.Infinite);
                            }

                            break;
                        }
                    case FieldMessageType.CalibrateAxis:
                    case FieldMessageType.ShutterPositioning:
                    case FieldMessageType.InverterPowerOff:
                    case FieldMessageType.InverterSwitchOn:
                    case FieldMessageType.InverterStop:

                        if (receivedMessage.Status == MessageStatus.OperationEnd)
                        {
                            this.currentStateMachine?.Dispose();
                            this.currentStateMachine = null;
                        }

                        break;

                    case FieldMessageType.InverterSwitchOff:
                        if (receivedMessage.Status == MessageStatus.OperationEnd)
                        {
                            this.currentStateMachine?.Dispose();
                            this.currentStateMachine = null;

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
                            this.currentStateMachine?.Dispose();
                            this.currentStateMachine = null;

                            var nextMessage = ((InverterPowerOnFieldMessageData)receivedMessage.Data).NextCommandMessage;
                            if (nextMessage != null)
                            {
                                this.commandQueue.Enqueue(nextMessage);
                            }
                        }

                        break;
                }
            }
            while (!this.stoppingToken.IsCancellationRequested);
        }

        private async Task ReceiveInverterData()
        {
            this.logger.LogTrace("1:Method Start");

            do
            {
                byte[] inverterData;
                try
                {
                    this.readWaitStopwatch.Reset();
                    this.readWaitStopwatch.Start();

                    inverterData = await this.socketTransport.ReadAsync(this.stoppingToken);

                    this.readWaitStopwatch.Stop();
                    this.roundTripStopwatch.Stop();
                    this.readSpeedStopwatch.Stop();
                    this.ReadSpeadTimeData.AddValue(this.readSpeedStopwatch.ElapsedTicks);
                    this.readSpeedStopwatch.Reset();
                    this.readSpeedStopwatch.Start();
                    this.ReadWaitTimeData.AddValue(this.readWaitStopwatch.ElapsedTicks);
                    this.WriteRoundtripTimeData.AddValue(this.roundTripStopwatch.ElapsedTicks);
                }
                catch (OperationCanceledException)
                {
                    this.logger.LogDebug("2:Method End operation cancelled");

                    return;
                }
                catch (InverterDriverException ex)
                {
                    this.logger.LogCritical($"2A: Exception {ex.Message}, InverterExceptionCode={ex.InverterDriverExceptionCode}");

                    this.SendOperationErrorMessage(new InverterExceptionFieldMessageData(ex, "Inverter Driver Exception", (int)ex.InverterDriverExceptionCode), FieldMessageType.InverterException);

                    return;
                }
                catch (Exception ex)
                {
                    this.logger.LogDebug($"3:Exception: {ex.Message}");

                    this.SendOperationErrorMessage(new InverterExceptionFieldMessageData(ex, "Inverter Driver Exeption", 0), FieldMessageType.InverterException);

                    return;
                }

                //INFO: Byte 1 of read data contains packet length, zero means invalid packet
                if (inverterData == null)
                {
                    this.logger.LogTrace($"4:Inverter message is null");
                    continue;
                }
                if (inverterData[1] == 0x00)
                {
                    this.logger.LogTrace($"5:Inverter message length is zero");
                    continue;
                }

                InverterMessage currentMessage;
                try
                {
                    currentMessage = new InverterMessage(inverterData);

                    this.logger.LogTrace($"6:currentMessage={currentMessage}");

                    this.writeEnableEvent.Set();
                }
                catch (InverterDriverException)
                {
                    continue;
                }
                catch (Exception ex)
                {
                    this.logger.LogTrace($"7:Exception {ex.Message} while parsing Inverter raw message bytes");

                    this.SendOperationErrorMessage(new InverterExceptionFieldMessageData(ex, $"Exception {ex.Message} while parsing Inverter raw message bytes", 0), FieldMessageType.InverterException);

                    return;
                }

                if (!Enum.TryParse(currentMessage.SystemIndex.ToString(), out InverterIndex inverterIndex))
                {
                    this.logger.LogTrace($"8:Invalid system index {currentMessage.SystemIndex} defined in Inverter Message");

                    var ex = new Exception();
                    this.SendOperationErrorMessage(new InverterExceptionFieldMessageData(ex, $"Invalid system index {currentMessage.SystemIndex} defined in Inverter Message", 0), FieldMessageType.InverterError);

                    return;
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

                this.logger.LogTrace($"2:handleIndex={handleIndex}");

                if (this.writeEnableEvent.Wait(Timeout.Infinite, this.stoppingToken))
                {
                    this.writeEnableEvent.Reset();

                    switch (handleIndex)
                    {
                        case 0:
                            await this.ProcessHeartbeat();
                            break;

                        case 1:
                            await this.ProcessInverterCommand();
                            break;
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
                       FieldMessageActor.InverterDriver,
                       FieldMessageActor.Any,
                       FieldMessageType.InverterError,
                       MessageStatus.OperationError,
                       ErrorLevel.Critical);
                    this.eventAggregator.GetEvent<FieldNotificationEvent>().Publish(errorMsg);
                    break;

                case FieldMessageType.InverterException:
                    var exceptionMsg = new FieldNotificationMessage(
                     messageData,
                     "Inverter Driver Exception",
                    FieldMessageActor.InverterDriver,
                    FieldMessageActor.Any,
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
