using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataLayer;
using Ferretto.VW.MAS.InverterDriver.Contracts;
using Ferretto.VW.MAS.InverterDriver.Diagnostics;
using Ferretto.VW.MAS.InverterDriver.Interface;
using Ferretto.VW.MAS.InverterDriver.StateMachines;
using Ferretto.VW.MAS.Utils;
using Ferretto.VW.MAS.Utils.Enumerations;
using Ferretto.VW.MAS.Utils.Events;
using Ferretto.VW.MAS.Utils.Messages;
using Ferretto.VW.MAS.Utils.Messages.FieldData;
using Ferretto.VW.MAS.Utils.Messages.FieldInterfaces;
using Ferretto.VW.MAS.Utils.Utilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Prism.Events;
using static Ferretto.VW.MAS.Utils.Utilities.BufferUtility;

namespace Ferretto.VW.MAS.InverterDriver
{
    internal partial class InverterDriverService : AutomationBackgroundService<FieldCommandMessage, FieldNotificationMessage, FieldCommandEvent, FieldNotificationEvent>
    {
        #region Fields

        private const int AXIS_POSITION_UPDATE_INTERVAL = 100;

        private const int HEARTBEAT_TIMEOUT = 300;

        private const int SENSOR_STATUS_UPDATE_INTERVAL = 500;

        private readonly Timer[] axisPositionUpdateTimer;

        private readonly IConfiguration configuration;

        private readonly Dictionary<InverterIndex, IInverterStateMachine> currentStateMachines = new Dictionary<InverterIndex, IInverterStateMachine>();

        private readonly IEventAggregator eventAggregator;

        private readonly bool[] forceStatusPublish;

        private readonly BlockingConcurrentQueue<InverterMessage> inverterCommandQueue = new BlockingConcurrentQueue<InverterMessage>();

        private readonly InverterFaultCodes inverterFaultCodes;

        private readonly Task inverterReceiveTask;

        private readonly Task inverterSendTask;

        private readonly NordFaultCodes nordFaultCodes;

        private readonly ISocketTransportInverter socketTransportInverter;

        private readonly ISocketTransportMock socketTransportMock;

        private readonly ISocketTransportNord socketTransportNord;

        private readonly Timer[] statusWordUpdateTimer;

        private readonly ManualResetEventSlim writeEnableEvent = new ManualResetEventSlim(true);

        private Axis currentAxis;

        private bool isDisposed;

        private byte[] receiveBuffer;

        private Timer sensorStatusUpdateTimer;

        private ISocketTransport socketTransport;

        #endregion

        #region Constructors

        public InverterDriverService(
            ILogger<InverterDriverService> logger,
            IEventAggregator eventAggregator,
            IServiceScopeFactory serviceScopeFactory,
            ISocketTransportInverter socketTransportInverter,
            ISocketTransportMock socketTransportMock,
            ISocketTransportNord socketTransportNord,
            IConfiguration configuration)
            : base(eventAggregator, logger, serviceScopeFactory)
        {
            this.socketTransportInverter = socketTransportInverter ?? throw new ArgumentNullException(nameof(socketTransportInverter));
            this.socketTransportMock = socketTransportMock ?? throw new ArgumentNullException(nameof(socketTransportMock));
            this.socketTransportNord = socketTransportNord ?? throw new ArgumentNullException(nameof(socketTransportNord));
            this.socketTransport = this.socketTransportInverter;
            this.eventAggregator = eventAggregator ?? throw new ArgumentNullException(nameof(eventAggregator));
            this.configuration = configuration;

            this.inverterReceiveTask = new Task(async () => await this.ReceiveInverterData());
            this.inverterSendTask = new Task(async () => await this.SendInverterCommand());

            this.axisPositionUpdateTimer = new Timer[(int)InverterIndex.Slave7 + 1];
            this.statusWordUpdateTimer = new Timer[(int)InverterIndex.Slave7 + 1];
            this.forceStatusPublish = new bool[(int)InverterIndex.Slave7 + 1];

            this.explicitMessagesTask = new Task(async () => await this.ExplicitMessages());

            this.inverterFaultCodes = new InverterFaultCodes();

            this.nordFaultCodes = new NordFaultCodes();
        }

        #endregion

        #region Methods

        public void Dispose(bool disposing)
        {
            if (this.isDisposed)
            {
                return;
            }

            if (disposing)
            {
                this.inverterCommandQueue?.Dispose();

                this.sensorStatusUpdateTimer?.Dispose();

                //this.heartBeatTimer?.Dispose();

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

            this.isDisposed = true;
        }

        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            if (this.isDisposed)
            {
                throw new ObjectDisposedException(this.GetType().Name);
            }

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
                    this.Logger.LogError($"Received error Message: {message}");
                    var errorCode = (int)DataModels.MachineErrorCode.InverterErrorBaseCode + message.UShortPayload;
                    if (!Enum.IsDefined(typeof(DataModels.MachineErrorCode), errorCode))
                    {
                        errorCode = (int)DataModels.MachineErrorCode.InverterErrorBaseCode;
                    }

                    serviceProvider
                        .GetRequiredService<IErrorsProvider>()
                        .RecordNew((DataModels.MachineErrorCode)errorCode, additionalText: message.SystemIndex.ToString());
                }

                if (message.IsWriteMessage)
                {
                    this.EvaluateWriteMessage(message, messageCurrentStateMachine, serviceProvider);
                }
                else
                {
                    this.EvaluateReadMessage(message, messageCurrentStateMachine, serviceProvider, this.inverterFaultCodes);
                }
                this.writeEnableEvent.Set();
                this.Logger.LogTrace($"writeEnableEvent unlocked");
            }
            catch (Exception ex)
            {
                this.Logger.LogError(ex, $"Exception while parsing Inverter raw message bytes {BitConverter.ToString(messageBytes)}");
                serviceProvider.GetRequiredService<IErrorsProvider>().RecordNew(DataModels.MachineErrorCode.InverterConnectionError, BayNumber.BayOne, ex.Message);

                this.SendOperationErrorMessage(InverterIndex.None, new InverterExceptionFieldMessageData(ex, $"Exception {ex.Message} while parsing Inverter raw message bytes", 0), FieldMessageType.InverterException);

                this.socketTransport.Disconnect();
            }
        }

        private async Task ReceiveInverterData()
        {
            this.Logger.LogTrace("1:Method Start");

            do
            {
                try
                {
                    using (var scope = this.ServiceScopeFactory.CreateScope())
                    {
                        var errorsProvider = scope.ServiceProvider.GetRequiredService<IErrorsProvider>();
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
                                errorsProvider.RecordNew(DataModels.MachineErrorCode.InverterConnectionError, BayNumber.BayOne);

                                var ex = new Exception();
                                this.SendOperationErrorMessage(InverterIndex.MainInverter, new InverterExceptionFieldMessageData(ex, "Socket Transport failed to connect", 0), FieldMessageType.InverterError);
                                continue;
                            }
                            else
                            {
                                this.Logger.LogInformation($"Connected to inverter's TCP address {this.inverterAddress}:{this.inverterPort}");
                                for (var i = 0; i < this.forceStatusPublish.Length; i++)
                                {
                                    this.forceStatusPublish[i] = true;
                                }
                            }

                            this.writeEnableEvent.Set();
                            this.Logger.LogTrace($"writeEnableEvent unlocked");
                        }

                        // socket connected
                        byte[] inverterData;
                        try
                        {
                            inverterData = await this.socketTransport.ReadAsync(this.CancellationToken);
                            if (inverterData == null || inverterData.Length == 0)
                            {
                                // connection error
                                this.Logger.LogError($"2:Inverter message is null");
                                errorsProvider.RecordNew(DataModels.MachineErrorCode.InverterConnectionError, BayNumber.BayOne);
                                this.SendOperationErrorMessage(InverterIndex.MainInverter, new InverterExceptionFieldMessageData(null, "Inverter Driver Connection Error", 0), FieldMessageType.InverterException);
                                continue;
                            }

                            this.receiveBuffer = this.receiveBuffer.AppendArrays(inverterData, inverterData.Length);
                        }
                        catch (Exception ex) when (ex is OperationCanceledException || ex is ThreadAbortException)
                        {
                            this.Logger.LogDebug("Terminating inverter read task.");

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
                            this.Logger.LogError($"Exception {ex.Message}; InnerException {ex.InnerException?.Message}", ex);
                            errorsProvider.RecordNew(DataModels.MachineErrorCode.InverterConnectionError, BayNumber.BayOne, ex.Message);
                            this.SendOperationErrorMessage(InverterIndex.MainInverter, new InverterExceptionFieldMessageData(ex, "Inverter Driver Connection Error", 0), FieldMessageType.InverterException);

                            continue;
                        }
                        catch (Exception ex)
                        {
                            this.Logger.LogCritical(ex, "Error while reading from inverter socket.");

                            this.SendOperationErrorMessage(InverterIndex.MainInverter, new InverterExceptionFieldMessageData(ex, "Inverter Driver Exeption", 0), FieldMessageType.InverterException);

                            return;
                        }

                        //INFO: Byte 1 of read data contains packet length
                        if (this.receiveBuffer[1] == 0x00)
                        {
                            // message error
                            this.Logger.LogError($"5:Inverter message length is zero: received {BitConverter.ToString(inverterData)}: message {BitConverter.ToString(this.receiveBuffer)}");
                            errorsProvider.RecordNew(DataModels.MachineErrorCode.InverterConnectionError, BayNumber.BayOne);
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

                        foreach (var extractedMessage in extractedMessages)
                        {
                            this.OnInverterMessageReceived(extractedMessage, scope.ServiceProvider);
                        }
                    }
                }
                catch (Exception ex) when (ex is OperationCanceledException || this.CancellationToken.IsCancellationRequested)
                {
                    this.Logger.LogDebug("Stopping inverter driver.");
                    return;
                }
            }

            while (!this.CancellationToken.IsCancellationRequested);
        }

        private async Task SendInverterCommand()
        {
            do
            {
                try
                {
                    if (this.inverterCommandQueue.TryPeek(Timeout.Infinite, this.CancellationToken, out var inverterMessage)
                        && inverterMessage != null)
                    {
                        this.Logger.LogTrace($"1:inverterMessage={inverterMessage}");
                        if (this.writeEnableEvent.Wait(Timeout.Infinite, this.CancellationToken))
                        {
                            this.Logger.LogTrace($"2:Command queue length: {this.inverterCommandQueue.Count}");

                            if (Debugger.IsAttached
                                &&
                                this.inverterCommandQueue.Count > 200)
                            {
                                Debugger.Break();
                            }

                            if (this.socketTransport.IsConnected)
                            {
                                this.writeEnableEvent.Reset();
                                this.Logger.LogTrace($"3:writeEnableEvent locked");

                                var result = await this.ProcessInverterCommand(inverterMessage);

                                if (result)
                                {
                                    this.inverterCommandQueue.Dequeue(out _);
                                }
                                else
                                {
                                    this.writeEnableEvent.Set();
                                    this.Logger.LogTrace($"writeEnableEvent unlocked");
                                }
                            }
                            else
                            {
                                Thread.Sleep(5);
                            }
                        }
                    }
                }
                catch (Exception ex) when (ex is OperationCanceledException || ex is ThreadAbortException)
                {
                    this.Logger.LogDebug("Terminating inverter write task.");
                    break;
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
                       ErrorLevel.Error);
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
                    ErrorLevel.Error);
                    this.eventAggregator.GetEvent<FieldNotificationEvent>().Publish(exceptionMsg);
                    break;

                case FieldMessageType.CalibrateAxis:
                    if (messageData is InverterExceptionFieldMessageData calibrateData)
                    {
                        var calibrateErrorNotification = new FieldNotificationMessage(
                        calibrateData,
                        "Requested Inverter is not configured",
                        FieldMessageActor.Any,
                        FieldMessageActor.InverterDriver,
                        FieldMessageType.CalibrateAxis,
                        MessageStatus.OperationError,
                        (byte)inverterIndex,
                        ErrorLevel.Error);
                        this.eventAggregator.GetEvent<FieldNotificationEvent>().Publish(calibrateErrorNotification);
                    }
                    break;

                case FieldMessageType.InverterSwitchOff:
                    if (messageData is InverterExceptionFieldMessageData switchOffData)
                    {
                        var inverterSwitchOffErrorNotification = new FieldNotificationMessage(
                        switchOffData,
                        $"Inverter status not configured for requested inverter {inverterIndex}",
                        FieldMessageActor.Any,
                        FieldMessageActor.InverterDriver,
                        FieldMessageType.InverterSwitchOff,
                        MessageStatus.OperationError,
                        (byte)inverterIndex,
                        ErrorLevel.Error);
                        this.eventAggregator.GetEvent<FieldNotificationEvent>().Publish(inverterSwitchOffErrorNotification);
                    }
                    break;

                case FieldMessageType.InverterSwitchOn:
                    if (messageData is InverterExceptionFieldMessageData switchOnData)
                    {
                        var inverterSwitchOnErrorNotification = new FieldNotificationMessage(
                        switchOnData,
                        $"Inverter status not configured for requested inverter {inverterIndex}",
                        FieldMessageActor.Any,
                        FieldMessageActor.InverterDriver,
                        FieldMessageType.InverterSwitchOn,
                        MessageStatus.OperationError,
                        (byte)inverterIndex,
                        ErrorLevel.Error);
                        this.eventAggregator.GetEvent<FieldNotificationEvent>().Publish(inverterSwitchOnErrorNotification);
                    }
                    break;

                case FieldMessageType.Positioning:

                    if (messageData is InverterExceptionFieldMessageData positioningData)
                    {
                        var positioningErrorNotification = new FieldNotificationMessage(
                        null,
                        positioningData.ExceptionDescription,
                        FieldMessageActor.Any,
                        FieldMessageActor.InverterDriver,
                        FieldMessageType.Positioning,
                        MessageStatus.OperationError,
                        (byte)inverterIndex,
                        ErrorLevel.Error);
                        this.eventAggregator.GetEvent<FieldNotificationEvent>().Publish(positioningErrorNotification);
                    }
                    break;

                case FieldMessageType.InverterPowerOff:
                    if (messageData is InverterExceptionFieldMessageData powerOffData)
                    {
                        var inverterPowerOfferrorNotification = new FieldNotificationMessage(
                        powerOffData,
                        "Requested Inverter is not configured",
                        FieldMessageActor.Any,
                        FieldMessageActor.InverterDriver,
                        FieldMessageType.InverterPowerOff,
                        MessageStatus.OperationError,
                        (byte)inverterIndex,
                        ErrorLevel.Error);
                        this.eventAggregator.GetEvent<FieldNotificationEvent>().Publish(inverterPowerOfferrorNotification);
                    }
                    break;

                case FieldMessageType.InverterPowerOn:
                    if (messageData is InverterExceptionFieldMessageData powerOnData)
                    {
                        var inverterPowerOnerrorNotification = new FieldNotificationMessage(
                        powerOnData,
                        "Requested Inverter is not configured",
                        FieldMessageActor.Any,
                        FieldMessageActor.InverterDriver,
                        FieldMessageType.InverterPowerOn,
                        MessageStatus.OperationError,
                        (byte)inverterIndex,
                        ErrorLevel.Error);
                        this.eventAggregator.GetEvent<FieldNotificationEvent>().Publish(inverterPowerOnerrorNotification);
                    }
                    break;

                case FieldMessageType.ShutterPositioning:
                    if (messageData is InverterExceptionFieldMessageData shutterPositioningData)
                    {
                        var shutterPositioningErrorNotification = new FieldNotificationMessage(
                        shutterPositioningData,
                        "Requested Inverter is not configured",
                        FieldMessageActor.Any,
                        FieldMessageActor.InverterDriver,
                        FieldMessageType.ShutterPositioning,
                        MessageStatus.OperationError,
                        (byte)inverterIndex,
                        ErrorLevel.Error);
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
                   ErrorLevel.Error);
                    this.eventAggregator.GetEvent<FieldNotificationEvent>().Publish(inverterStopErrorNotification);
                    break;

                case FieldMessageType.InverterStatusUpdate:
                    if (messageData is InverterExceptionFieldMessageData updateData)
                    {
                        var inverterUpdateStatusErrorNotification = new FieldNotificationMessage(
                        updateData,
                        "Wrong message Data data type",
                        FieldMessageActor.Any,
                        FieldMessageActor.InverterDriver,
                        FieldMessageType.InverterStatusUpdate,
                        MessageStatus.OperationError,
                        (byte)inverterIndex,
                        ErrorLevel.Error);
                        this.eventAggregator.GetEvent<FieldNotificationEvent>().Publish(inverterUpdateStatusErrorNotification);
                    }
                    break;

                case FieldMessageType.InverterSetTimer:
                    if (messageData is InverterExceptionFieldMessageData setTimerData)
                    {
                        var inverterUpdateStatusErrorNotification = new FieldNotificationMessage(
                        setTimerData,
                        "Wrong message Data data type",
                        FieldMessageActor.Any,
                        FieldMessageActor.InverterDriver,
                        FieldMessageType.InverterSetTimer,
                        MessageStatus.OperationError,
                        (byte)inverterIndex,
                        ErrorLevel.Error);
                        this.eventAggregator.GetEvent<FieldNotificationEvent>().Publish(inverterUpdateStatusErrorNotification);
                    }
                    break;

                case FieldMessageType.InverterReading:
                    if (messageData is InverterExceptionFieldMessageData inverterReadingData)
                    {
                        var inverterUpdateStatusErrorNotification = new FieldNotificationMessage(
                        inverterReadingData,
                        "Error during reading inverter parameter",
                        FieldMessageActor.Any,
                        FieldMessageActor.InverterDriver,
                        FieldMessageType.InverterReading,
                        MessageStatus.OperationError,
                        (byte)inverterIndex,
                        ErrorLevel.Error);
                        this.eventAggregator.GetEvent<FieldNotificationEvent>().Publish(inverterUpdateStatusErrorNotification);
                    }
                    break;

                case FieldMessageType.InverterProgramming:
                    if (messageData is InverterExceptionFieldMessageData inverterProgrammingData)
                    {
                        var inverterUpdateStatusErrorNotification = new FieldNotificationMessage(
                        inverterProgrammingData,
                        "Error during writing inverter parameter",
                        FieldMessageActor.Any,
                        FieldMessageActor.InverterDriver,
                        FieldMessageType.InverterProgramming,
                        MessageStatus.OperationError,
                        (byte)inverterIndex,
                        ErrorLevel.Error);
                        this.eventAggregator.GetEvent<FieldNotificationEvent>().Publish(inverterUpdateStatusErrorNotification);
                    }
                    break;
            }
        }

        #endregion
    }
}
