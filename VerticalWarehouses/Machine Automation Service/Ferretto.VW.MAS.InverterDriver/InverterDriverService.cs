﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataLayer;
using Ferretto.VW.MAS.InverterDriver.Contracts;
using Ferretto.VW.MAS.InverterDriver.Diagnostics;
using Ferretto.VW.MAS.InverterDriver.Interface;
using Ferretto.VW.MAS.InverterDriver.InverterStatus.Interfaces;
using Ferretto.VW.MAS.InverterDriver.StateMachines;
using Ferretto.VW.MAS.Utils;
using Ferretto.VW.MAS.Utils.Enumerations;
using Ferretto.VW.MAS.Utils.Events;
using Ferretto.VW.MAS.Utils.Messages;
using Ferretto.VW.MAS.Utils.Messages.FieldData;
using Ferretto.VW.MAS.Utils.Messages.FieldInterfaces;
using Ferretto.VW.MAS.Utils.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Prism.Events;
using static Ferretto.VW.MAS.Utils.Utilities.BufferUtility;

// ReSharper disable ArrangeThisQualifier
// ReSharper disable ParameterHidesMember
namespace Ferretto.VW.MAS.InverterDriver
{
    internal partial class InverterDriverService : AutomationBackgroundService<FieldCommandMessage, FieldNotificationMessage, FieldCommandEvent, FieldNotificationEvent>
    {
        #region Fields

        private const int AXIS_POSITION_UPDATE_INTERVAL = 100;

        private const int HEARTBEAT_TIMEOUT = 300;

        private const int SENSOR_STATUS_UPDATE_INTERVAL = 500;

        private readonly Stopwatch axisIntervalStopwatch = new Stopwatch();

        private readonly InverterDiagnosticsData AxisIntervalTimeData = new InverterDiagnosticsData();

        private readonly Timer[] axisPositionUpdateTimer;

        private readonly Stopwatch axisStopwatch = new Stopwatch();

        private readonly InverterDiagnosticsData AxisTimeData = new InverterDiagnosticsData();

        private readonly Dictionary<InverterIndex, IInverterStateMachine> currentStateMachines = new Dictionary<InverterIndex, IInverterStateMachine>();

        private readonly IEventAggregator eventAggregator;

        private readonly BlockingConcurrentQueue<InverterMessage> heartbeatQueue = new BlockingConcurrentQueue<InverterMessage>();

        private readonly BlockingConcurrentQueue<InverterMessage> inverterCommandQueue = new BlockingConcurrentQueue<InverterMessage>();

        private readonly Task inverterReceiveTask;

        private readonly Task inverterSendTask;

        private readonly Stopwatch readSpeedStopwatch = new Stopwatch();

        private readonly InverterDiagnosticsData ReadSpeedTimeData = new InverterDiagnosticsData();

        private readonly Stopwatch readWaitStopwatch = new Stopwatch();

        private readonly InverterDiagnosticsData ReadWaitTimeData = new InverterDiagnosticsData();

        private readonly Stopwatch roundTripStopwatch = new Stopwatch();

        private readonly Stopwatch sensorIntervalStopwatch = new Stopwatch();

        private readonly InverterDiagnosticsData SensorIntervalTimeData = new InverterDiagnosticsData();

        private readonly Stopwatch sensorStopwatch = new Stopwatch();

        private readonly InverterDiagnosticsData SensorTimeData = new InverterDiagnosticsData();

        private readonly ISocketTransport socketTransport;

        private readonly Timer[] statusWordUpdateTimer;

        private readonly ManualResetEventSlim writeEnableEvent = new ManualResetEventSlim(true);

        private readonly InverterDiagnosticsData WriteRoundtripTimeData = new InverterDiagnosticsData();

        private Axis currentAxis;

        private bool disposed;

        private bool forceStatusPublish;

        private byte[] receiveBuffer;

        private Timer sensorStatusUpdateTimer;

        #endregion

        #region Constructors

        public InverterDriverService(
            ILogger<InverterDriverService> logger,
            IEventAggregator eventAggregator,
            IServiceScopeFactory serviceScopeFactory,
            ISocketTransport socketTransport)
            : base(eventAggregator, logger, serviceScopeFactory)
        {
            this.socketTransport = socketTransport ?? throw new ArgumentNullException(nameof(socketTransport));
            this.eventAggregator = eventAggregator ?? throw new ArgumentNullException(nameof(eventAggregator));

            this.inverterReceiveTask = new Task(async () => await this.ReceiveInverterData());
            this.inverterSendTask = new Task(async () => await this.SendInverterCommand());

            this.axisPositionUpdateTimer = new Timer[(int)InverterIndex.Slave7 + 1];
            this.statusWordUpdateTimer = new Timer[(int)InverterIndex.Slave7 + 1];
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
                this.sensorStatusUpdateTimer?.Dispose();

                foreach (var timer in this.axisPositionUpdateTimer)
                {
                    timer?.Dispose();
                }

                foreach (var timer in this.statusWordUpdateTimer)
                {
                    timer?.Dispose();
                }

                this.writeEnableEvent?.Dispose();
            }

            this.disposed = true;
        }

        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            await base.StartAsync(cancellationToken);

            this.sensorStatusUpdateTimer?.Dispose();
            this.sensorStatusUpdateTimer = new Timer(this.RequestSensorStatusUpdate, null, -1, Timeout.Infinite);

            for (var id = InverterIndex.MainInverter; id <= InverterIndex.Slave7; id++)
            {
                this.axisPositionUpdateTimer[(int)id]?.Dispose();
                this.axisPositionUpdateTimer[(int)id] = new Timer(this.RequestAxisPositionUpdate, id, -1, Timeout.Infinite);
                this.statusWordUpdateTimer[(int)id]?.Dispose();
                this.statusWordUpdateTimer[(int)id] = new Timer(this.RequestStatusWordMessage, id, -1, Timeout.Infinite);
            }
        }

        protected override bool FilterCommand(FieldCommandMessage command)
        {
            return
                command.Destination == FieldMessageActor.InverterDriver
                ||
                command.Destination == FieldMessageActor.Any;
        }

        protected override bool FilterNotification(FieldNotificationMessage notification)
        {
            return
                notification.Destination == FieldMessageActor.InverterDriver
                ||
                notification.Destination == FieldMessageActor.Any;
        }

        private void OnInverterMessageReceived(byte[] messageBytes, IServiceProvider serviceProvider)
        {
            try
            {
                var message = InverterMessage.FromBytes(messageBytes);

                this.Logger.LogTrace($"Received Inverter Message: {message}");

                this.currentStateMachines.TryGetValue(message.SystemIndex, out var messageCurrentStateMachine);

                if (message.IsError)
                {
                    var inverterErrorCode = message.UShortPayload;

                    var errorCode = (int)DataModels.MachineErrors.InverterErrorBaseCode + inverterErrorCode;

                    if (!Enum.IsDefined(typeof(DataModels.MachineErrors), errorCode))
                    {
                        errorCode = (int)DataModels.MachineErrors.InverterErrorBaseCode;
                    }

                    serviceProvider
                        .GetRequiredService<IErrorsProvider>()
                        .RecordNew((DataModels.MachineErrors)errorCode);
                }

                if (message.IsWriteMessage)
                {
                    this.EvaluateWriteMessage(message, messageCurrentStateMachine, serviceProvider);
                }
                else
                {
                    this.EvaluateReadMessage(message, messageCurrentStateMachine, serviceProvider);
                }
            }
            catch (Exception ex)
            {
                this.Logger.LogError($"7:Exception {ex.Message} while parsing Inverter raw message bytes {BitConverter.ToString(messageBytes)}");

                this.SendOperationErrorMessage(InverterIndex.None, new InverterExceptionFieldMessageData(ex, $"Exception {ex.Message} while parsing Inverter raw message bytes", 0), FieldMessageType.InverterException);

                this.socketTransport.Disconnect();
            }
        }

        private async Task ReceiveInverterData()
        {
            this.Logger.LogTrace("1:Method Start");

            using (var scope = this.ServiceScopeFactory.CreateScope())
            {
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
                            this.Logger.LogError($"1: Exception {ex.Message}; Exception code={ex.InverterDriverExceptionCode};\nInner exception: {ex.InnerException.Message}");
                        }
                        catch (Exception ex)
                        {
                            this.Logger.LogError($"2:Exception {ex.Message} while Connecting Receiver Socket Transport");

                            this.SendOperationErrorMessage(InverterIndex.MainInverter, new InverterExceptionFieldMessageData(ex, "while Connecting Receiver Socket Transport", 0), FieldMessageType.InverterException);
                            throw new InverterDriverException($"Exception {ex.Message} ReceiveInverterData Failed 1", ex);
                        }

                        if (!this.socketTransport.IsConnected)
                        {
                            this.Logger.LogError("3:Socket Transport failed to connect");

                            var ex = new Exception();
                            this.SendOperationErrorMessage(InverterIndex.MainInverter, new InverterExceptionFieldMessageData(ex, "Socket Transport failed to connect", 0), FieldMessageType.InverterError);
                            continue;
                        }
                        else
                        {
                            this.Logger.LogInformation($"3:Connection OK ipAddress={this.inverterAddress}:Port={this.inverterPort}");
                            this.forceStatusPublish = true;
                        }

                        this.writeEnableEvent.Set();
                    }

                    // socket connected
                    byte[] inverterData;
                    try
                    {
                        this.readWaitStopwatch.Reset();
                        this.readWaitStopwatch.Start();

                        inverterData = await this.socketTransport.ReadAsync(this.CancellationToken);
                        if (inverterData == null || inverterData.Length == 0)
                        {
                            // connection error
                            this.Logger.LogError($"2:Inverter message is null");
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
                        this.Logger.LogDebug("2:Method End - operation cancelled");

                        return;
                    }
                    catch (InverterDriverException ex)
                    {
                        this.Logger.LogCritical($"2A: Exception {ex.Message}, InverterExceptionCode={ex.InverterDriverExceptionCode}");

                        this.SendOperationErrorMessage(InverterIndex.MainInverter, new InverterExceptionFieldMessageData(ex, "Inverter Driver Exception", (int)ex.InverterDriverExceptionCode), FieldMessageType.InverterException);

                        throw new InverterDriverException($"Exception {ex.Message} ReceiveInverterData Failed 2", ex);
                    }
                    catch (InvalidOperationException ex)
                    {
                        // connection error
                        this.Logger.LogError($"Exception {ex.Message}; InnerException {ex.InnerException?.Message ?? string.Empty}");
                        this.SendOperationErrorMessage(InverterIndex.MainInverter, new InverterExceptionFieldMessageData(ex, "Inverter Driver Connection Error", 0), FieldMessageType.InverterException);
                        continue;
                    }
                    catch (Exception ex)
                    {
                        this.Logger.LogCritical($"3:Exception: {ex.Message}");

                        this.SendOperationErrorMessage(InverterIndex.MainInverter, new InverterExceptionFieldMessageData(ex, "Inverter Driver Exeption", 0), FieldMessageType.InverterException);

                        throw new InverterDriverException($"Exception {ex.Message} ReceiveInverterData Failed 3", ex);
                    }

                    //INFO: Byte 1 of read data contains packet length
                    if (this.receiveBuffer[1] == 0x00)
                    {
                        // message error
                        this.Logger.LogError($"5:Inverter message length is zero: received {BitConverter.ToString(inverterData)}: message {BitConverter.ToString(this.receiveBuffer)}");
                        this.SendOperationErrorMessage(InverterIndex.MainInverter, new InverterExceptionFieldMessageData(null, "Inverter Driver Connection Error", 0), FieldMessageType.InverterException);
                        this.socketTransport.Disconnect();
                        continue;
                    }
                    if (this.receiveBuffer.Length < 2 || this.receiveBuffer.Length < this.receiveBuffer[1] + 2)
                    {
                        // this is not an error: we try to recover from messages received in more pieces
                        this.Logger.LogTrace($"5:Inverter message is not complete: received {BitConverter.ToString(inverterData)}: message {BitConverter.ToString(this.receiveBuffer)}");
                        continue;
                    }

                    var extractedMessages = GetMessagesWithHeaderLengthToEnqueue(ref this.receiveBuffer, 4, 1, 2);
                    if (extractedMessages.Count > 0)
                    {
                        this.writeEnableEvent.Set();
                    }

                    foreach (var extractedMessage in extractedMessages)
                    {
                        this.OnInverterMessageReceived(extractedMessage, scope.ServiceProvider);
                    }
                }
                while (!this.CancellationToken.IsCancellationRequested);
            }
        }

        private async Task SendInverterCommand()
        {
            //INFO Create WaitHandle array to wait for multiple events
            var commandHandles = new[]
            {
                this.heartbeatQueue.WaitHandle,
                this.inverterCommandQueue.WaitHandle
            };

            IAngInverterStatus mainInverter = null;

            do
            {
                this.Logger.LogTrace($"1:Heartbeat Queue Length: {this.heartbeatQueue.Count}, Command queue length: {this.inverterCommandQueue.Count}");

                if (Debugger.IsAttached
                    &&
                    this.inverterCommandQueue.Count > 20)
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

                    this.Logger.LogTrace($"2:handleIndex={handleIndex} {Thread.CurrentThread.ManagedThreadId}");

                    try
                    {
                        if (this.writeEnableEvent.Wait(Timeout.Infinite, this.CancellationToken))
                        {
                            if (this.socketTransport.IsConnected)
                            {
                                this.writeEnableEvent.Reset();

                                var result = false;

                                switch (handleIndex)
                                {
                                    case 0:
                                        mainInverter = mainInverter ?? this.ServiceScopeFactory
                                            .CreateScope()
                                            .ServiceProvider
                                            .GetRequiredService<IInvertersProvider>()
                                            .GetMainInverter();

                                        result = await this.ProcessHeartbeat(mainInverter);
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
                    catch (OperationCanceledException)
                    {
                        this.Logger.LogInformation("WriteEnable wait was canceled.");
                        break;
                    }
                }
                else
                {
                    Thread.Sleep(5);
                }
            }
            while (!this.CancellationToken.IsCancellationRequested);
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
                        this.eventAggregator.GetEvent<FieldNotificationEvent>().Publish(calibrateErrorNotification);
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
                        this.eventAggregator.GetEvent<FieldNotificationEvent>().Publish(inverterSwitchOffErrorNotification);
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
                        this.eventAggregator.GetEvent<FieldNotificationEvent>().Publish(inverterSwitchOnErrorNotification);
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
                        this.eventAggregator.GetEvent<FieldNotificationEvent>().Publish(positioningErrorNotification);
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
                        this.eventAggregator.GetEvent<FieldNotificationEvent>().Publish(inverterPowerOfferrorNotification);
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
                        this.eventAggregator.GetEvent<FieldNotificationEvent>().Publish(inverterPowerOnerrorNotification);
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
                        this.eventAggregator.GetEvent<FieldNotificationEvent>().Publish(shutterPositioningErrorNotification);
                    }
                    break;

                case FieldMessageType.InverterStop:
                    var inverterStopErrorNotification = new FieldNotificationMessage(
                   null,
                   $"Inverter status not configured for requested inverter {inverterIndex}",
                   FieldMessageActor.Any,
                   FieldMessageActor.InverterDriver,
                   FieldMessageType.InverterStop,
                   MessageStatus.OperationError,
                   (byte)inverterIndex,
                   ErrorLevel.Critical);
                    this.eventAggregator.GetEvent<FieldNotificationEvent>().Publish(inverterStopErrorNotification);
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
                        this.eventAggregator.GetEvent<FieldNotificationEvent>().Publish(inverterUpdateStatusErrorNotification);
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
                        this.eventAggregator.GetEvent<FieldNotificationEvent>().Publish(inverterUpdateStatusErrorNotification);
                    }
                    break;
            }
        }

        #endregion
    }
}
