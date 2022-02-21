using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataLayer;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS.Utils.Enumerations;
using Ferretto.VW.MAS.Utils.Events;
using Ferretto.VW.MAS.Utils.Exceptions;
using Ferretto.VW.MAS.Utils.Messages;
using Ferretto.VW.MAS.Utils.Messages.FieldData;
using Ferretto.VW.MAS.Utils.Messages.FieldInterfaces;
using Ferretto.VW.MAS.Utils.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Prism.Events;

namespace Ferretto.VW.MAS.IODriver
{
    internal sealed partial class IoDevice : IIoDevice, IDisposable
    {
        #region Fields

        private const int IoPollingInterval = 50;

        private readonly BayNumber bayNumber;

        private readonly IoIndex deviceIndex;

        private readonly IEventAggregator eventAggregator;

        private readonly BlockingConcurrentQueue<IoWriteMessage> ioCommandQueue = new BlockingConcurrentQueue<IoWriteMessage>();

        private readonly Task ioReceiveTask;

        private readonly Task ioSendTask;

        private readonly IoStatus ioStatus;

        private readonly IIoTransport ioTransport;

        private readonly IPAddress ipAddress;

        private readonly bool isCarousel;

        private readonly bool isDoubleBay;

        private readonly bool isExternalBay;

        private readonly ILogger logger;

        private readonly IoStatus mainIoDevice;

        private readonly int port;

        private readonly IServiceScopeFactory serviceScopeFactory;

        private readonly CancellationToken stoppingToken;

        private readonly object syncAccess = new object();

        private readonly ManualResetEventSlim writeEnableEvent = new ManualResetEventSlim(true);

        private bool commandExecuting;

        private IIoStateMachine currentStateMachine;

        private bool forceIoStatusPublish;

        private bool isDisposed;

        private Timer pollIoTimer;

        private byte[] receiveBuffer;

        #endregion

        #region Constructors

        public IoDevice(
            IEventAggregator eventAggregator,
            IIoDevicesProvider ioDeviceService,
            IServiceScopeFactory serviceScopeFactory,
            IIoTransport shdTransport,
            IPAddress ipAddress,
            int port,
            IoIndex index,
            Bay bay,
            ILogger logger,
            CancellationToken cancellationToken,
            IHostingEnvironment env)
        {
            this.eventAggregator = eventAggregator;
            this.ipAddress = ipAddress;
            this.port = port;
            this.deviceIndex = index;
            this.logger = logger;
            this.ioTransport = shdTransport;
            this.stoppingToken = cancellationToken;
            this.isCarousel = bay.Carousel != null;
            this.isExternalBay = bay.IsExternal;
            this.isDoubleBay = bay.IsDouble;
            this.bayNumber = bay.Number;
            this.serviceScopeFactory = serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));

            this.ioReceiveTask = new Task(async () => await this.ReceiveIoDataTaskFunction(env));
            this.ioSendTask = new Task(async () => await this.SendIoCommandTaskFunction());

            this.mainIoDevice = ioDeviceService.Devices.SingleOrDefault(s => s.IoIndex == IoIndex.IoDevice1);
            this.ioStatus = ioDeviceService.Devices.SingleOrDefault(s => s.IoIndex == index) ?? throw new ArgumentNullException(nameof(index));
        }

        #endregion

        #region Properties

        public bool IsCommandExecuting
        {
            get
            {
                var value = false;
                lock (this.syncAccess)
                {
                    value = this.commandExecuting;
                }
                return value;
            }
        }

        private IIoStateMachine CurrentStateMachine
        {
            get => this.currentStateMachine;
            set
            {
                if (this.currentStateMachine != value)
                {
                    this.currentStateMachine = value;
                }

                //var objectName = string.Empty;
                //if (this.currentStateMachine != null)
                //{
                //    objectName = this.currentStateMachine.GetType().Name;
                //}

                //var notificationMessageData = new MachineStatusActiveMessageData(MessageActor.IoDriver, objectName, MessageVerbosity.Info);
                //var notificationMessage = new NotificationMessage(
                //    notificationMessageData,
                //    !string.IsNullOrEmpty(objectName) ? $"IoDriver current machine state {objectName}" : $"IoDriver current machine is null",
                //    MessageActor.Any,
                //    MessageActor.IoDriver,
                //    MessageType.MachineStatusActive,
                //    BayNumber.None,
                //    BayNumber.None,
                //    MessageStatus.OperationStart);

                //this.eventAggregator.GetEvent<NotificationEvent>().Publish(notificationMessage);
            }
        }

        #endregion

        #region Methods

        public void DestroyStateMachine()
        {
            if (this.isDisposed)
            {
                throw new ObjectDisposedException(nameof(IoDevice));
            }

            if (this.CurrentStateMachine is IDisposable stateMachine)
            {
                stateMachine.Dispose();
            }

            this.CurrentStateMachine = null;
            this.commandExecuting = false;
        }

        public void Disconnect()
        {
            try
            {
                this.ioTransport.Disconnect();
                this.logger.LogInformation($"Disconnect I/O device {this.deviceIndex}");
            }
            catch (Exception ex)
            {
                this.logger.LogCritical($"Fatal error while disconnecting I/O device {this.deviceIndex}: {ex.Message} ");
            }
        }

        public void Dispose()
        {
            this.Dispose(true);
        }

        public async Task ReceiveIoDataTaskFunction(IHostingEnvironment env)
        {
            if (this.isDisposed)
            {
                throw new ObjectDisposedException(nameof(IoDevice));
            }

            this.logger.LogTrace("1:Method Start");

            const int N_BYTES_16 = 16;
            const int N_BYTES_8 = 8;

            do
            {
                if (!this.ioTransport.IsConnected)
                {
                    try
                    {
                        this.receiveBuffer = null;
                        await this.ioTransport.ConnectAsync(this.ipAddress, this.port);
                    }
                    catch (IoDriverException ex)
                    {
                        this.logger.LogError($"2:Exception: {ex.Message} while connecting to Modbus I/O master - ExceptionCode: {IoDriverExceptionCode.DeviceNotConnected};\nInner exception: {ex.InnerException.Message}");
                        this.SendOperationErrorMessage(new IoExceptionFieldMessageData(ex, "IO Driver Exception", (int)IoDriverExceptionCode.DeviceNotConnected));
                    }
                    catch (Exception ex)
                    {
                        this.logger.LogCritical($"Error while connecting to Modbus I/O master - ExceptionCode: {IoDriverExceptionCode.CreationFailure}");

                        this.SendOperationErrorMessage(new IoExceptionFieldMessageData(ex, "IO Driver Exception", (int)IoDriverExceptionCode.CreationFailure), isFatalError: true);

                        return;
                    }

                    if (!this.ioTransport.IsConnected)
                    {
                        this.logger.LogError("3:Socket Transport failed to connect");

                        using (var scope = this.serviceScopeFactory.CreateScope())
                        {
                            var errorsProvider = scope.ServiceProvider.GetRequiredService<IErrorsProvider>();

                            errorsProvider.RecordNew(DataModels.MachineErrorCode.IoDeviceConnectionError, this.bayNumber);
                        }

                        this.SendOperationErrorMessage(new IoExceptionFieldMessageData(new Exception(), "IO Driver Connection Error", (int)IoDriverExceptionCode.DeviceNotConnected));
                        continue;
                    }
                    else
                    {
                        this.logger.LogInformation($"3:Connection OK ipAddress={this.ipAddress}:Port={this.port}");
                    }

                    this.writeEnableEvent.Set();

                    var message = new IoWriteMessage(
                        this.ioStatus.ComunicationTimeOut,
                        this.ioStatus.UseSetupOutputLines,
                        this.ioStatus.SetupOutputLines,
                        this.ioStatus.DebounceInput);

                    this.logger.LogDebug(
                        $"1:ConfigurationMessage [comTout={this.ioStatus.ComunicationTimeOut} ms - debounceTime={this.ioStatus.DebounceInput} ms]");

                    this.ioCommandQueue.Enqueue(message);
                    this.forceIoStatusPublish = true;
                }

                byte[] telegram;
                try
                {
                    telegram = await this.ioTransport.ReadAsync(this.stoppingToken);

                    if (telegram == null || telegram.Length == 0)
                    {
                        // connection error
                        this.logger.LogError($"4:IO Driver message is null");
                        using (var scope = this.serviceScopeFactory.CreateScope())
                        {
                            var errorsProvider = scope.ServiceProvider.GetRequiredService<IErrorsProvider>();

                            errorsProvider.RecordNew(DataModels.MachineErrorCode.IoDeviceConnectionError, this.bayNumber);
                        }
                        var ex = new Exception();
                        this.SendOperationErrorMessage(new IoExceptionFieldMessageData(ex, "IO Driver Connection Error", (int)IoDriverExceptionCode.DeviceNotConnected));
                        continue;
                    }
                }
                catch (Exception ex) when (ex is OperationCanceledException || ex is ThreadAbortException)
                {
                    this.logger.LogDebug($"Terminating I/O Device {this.deviceIndex} read thread.");

                    return;
                }
                catch (IoDriverException ex)
                {
                    // connection error
                    this.logger.LogError(ex, $"3:Exception: {ex.Message} while connecting to Modbus I/O master - ExceptionCode: {IoDriverExceptionCode.DeviceNotConnected}; Inner exception: {ex.InnerException?.Message ?? string.Empty}");
                    using (var scope = this.serviceScopeFactory.CreateScope())
                    {
                        var errorsProvider = scope.ServiceProvider.GetRequiredService<IErrorsProvider>();

                        errorsProvider.RecordNew(DataModels.MachineErrorCode.IoDeviceConnectionError, this.bayNumber, ex.Message);
                    }
                    this.SendOperationErrorMessage(new IoExceptionFieldMessageData(ex, "IO Driver Connection Error", (int)IoDriverExceptionCode.DeviceNotConnected));
                    continue;
                }
                catch (Exception ex)
                {
                    this.logger.LogCritical($"Fatal error for I/O device {this.deviceIndex} while reading message {ex.Message} - ExceptionCode: {IoDriverExceptionCode.CreationFailure}");

                    this.SendOperationErrorMessage(new IoExceptionFieldMessageData(ex, "IO Driver Fatal Error", (int)IoDriverExceptionCode.DeviceNotConnected), isFatalError: true);

                    return;
                }

                this.receiveBuffer = this.receiveBuffer.AppendArrays(telegram, telegram.Length);

                // INFO: Byte 0 of read data contains packet length
                if (!this.IsHeaderValid(this.receiveBuffer[0]))
                {
                    // message error
                    this.logger.LogError($"5:IO Driver message length error: received {BitConverter.ToString(telegram)}: message {BitConverter.ToString(this.receiveBuffer)}");
                    using (var scope = this.serviceScopeFactory.CreateScope())
                    {
                        var errorsProvider = scope.ServiceProvider.GetRequiredService<IErrorsProvider>();

                        errorsProvider.RecordNew(DataModels.MachineErrorCode.IoDeviceConnectionError, this.bayNumber);
                    }
                    var ex = new Exception();
                    this.SendOperationErrorMessage(new IoExceptionFieldMessageData(ex, "IO Driver Connection Error", (int)IoDriverExceptionCode.DeviceNotConnected));
                    this.ioTransport.Disconnect();
                    continue;
                }

                if (this.receiveBuffer.Length < this.receiveBuffer[0])
                {
                    // this is not an error: we try to recover from messages received in more pieces
                    this.logger.LogWarning($"5:IO Driver message is not complete: received {BitConverter.ToString(telegram)}: message {BitConverter.ToString(this.receiveBuffer)}");
                    continue;
                }

                var extractedMessages = BufferUtility.GetMessagesWithHeaderLengthToEnqueue(ref this.receiveBuffer, 3, 0, 0);
                if (this.receiveBuffer.Length > 0)
                {
                    this.logger.LogWarning($"Message extracted: count {extractedMessages.Count}: left bytes {this.receiveBuffer.Length}");
                }

                if (extractedMessages.Count > 0)
                {
                    this.writeEnableEvent.Set();
                }

                var inputData = new bool[N_BYTES_16];
                var outputData = new bool[N_BYTES_8];
                byte[] configurationData;
                foreach (var extractedMessage in extractedMessages)
                {
                    if (this.IsMessageLengthValid(extractedMessage[1], extractedMessage[0]))
                    {
                        // message error
                        this.logger.LogError($"5:IO Driver message error: received {BitConverter.ToString(telegram)}: message {BitConverter.ToString(this.receiveBuffer)}");
                        using (var scope = this.serviceScopeFactory.CreateScope())
                        {
                            var errorsProvider = scope.ServiceProvider.GetRequiredService<IErrorsProvider>();

                            errorsProvider.RecordNew(DataModels.MachineErrorCode.IoDeviceConnectionError, this.bayNumber);
                        }
                        var ex = new Exception();
                        this.SendOperationErrorMessage(new IoExceptionFieldMessageData(ex, "IO Driver Connection Error", (int)IoDriverExceptionCode.DeviceNotConnected));
                        this.ioTransport.Disconnect();
                        break;
                    }

                    byte fwRelease;
                    byte errorCode;

                    ShdFormatDataOperation formatDataOperation;
                    try
                    {
                        // socket connected
                        this.ParsingDataBytes(
                            extractedMessage,
                            out var nBytesReceived,
                            out formatDataOperation,
                            out fwRelease,
                            ref inputData,
                            ref outputData,
                            out configurationData,
                            out errorCode);

                        this.ioStatus.FwRelease = fwRelease;
                    }
                    catch (Exception ex)
                    {
                        // message error
                        this.logger.LogError(ex, $"6:IO Driver message error: received {BitConverter.ToString(telegram)}: message {BitConverter.ToString(extractedMessage)}");
                        using (var scope = this.serviceScopeFactory.CreateScope())
                        {
                            var errorsProvider = scope.ServiceProvider.GetRequiredService<IErrorsProvider>();

                            errorsProvider.RecordNew(DataModels.MachineErrorCode.IoDeviceConnectionError, this.bayNumber, ex.Message);
                        }
                        this.SendOperationErrorMessage(new IoExceptionFieldMessageData(ex, "IO Driver Connection Error", (int)IoDriverExceptionCode.DeviceNotConnected));
                        this.ioTransport.Disconnect();
                        break;
                    }

                    switch (formatDataOperation)
                    {
                        case ShdFormatDataOperation.Data:

                            inputData[(int)IoPorts.AntiIntrusionBarrierBay] = !inputData[(int)IoPorts.AntiIntrusionBarrierBay];

                            // INFO The emergency button signal must be inverted
                            inputData[(int)IoPorts.MushroomEmergency] = !inputData[(int)IoPorts.MushroomEmergency];

                            // INFO the left carter signal is inverted and it depends from emergency button
                            inputData[(int)IoPorts.MicroCarterLeftSideBay] = !inputData[(int)IoPorts.MicroCarterLeftSideBay] && !inputData[(int)IoPorts.MushroomEmergency];

                            // INFO the right carter signal is inverted and it depends from emergency button and left carter
                            inputData[(int)IoPorts.MicroCarterRightSideBay] = !inputData[(int)IoPorts.MicroCarterRightSideBay] && !inputData[(int)IoPorts.MushroomEmergency] && !inputData[(int)IoPorts.MicroCarterLeftSideBay];

                            // INFO The sensor presence in bay must be inverted
                            inputData[(int)IoPorts.LoadingUnitInBay] = !inputData[(int)IoPorts.LoadingUnitInBay];

                            // INFO The sensor presence in lower bay must be inverted (NOT for carousel or External bay)
                            inputData[(int)IoPorts.LoadingUnitInLowerBay] = (this.isCarousel || (this.isExternalBay && !this.isDoubleBay)) ? inputData[(int)IoPorts.LoadingUnitInLowerBay] : !inputData[(int)IoPorts.LoadingUnitInLowerBay];

                            if (this.ioStatus.UpdateInputStates(inputData) || this.forceIoStatusPublish)
                            {
                                var data = new SensorsChangedFieldMessageData();
                                data.SensorsStates = inputData.ToArray();
                                var notificationMessage = new FieldNotificationMessage(
                                    data,
                                    "Update IO sensors",
                                    FieldMessageActor.DeviceManager,
                                    FieldMessageActor.IoDriver,
                                    FieldMessageType.SensorsChanged,
                                    MessageStatus.OperationExecuting,
                                    (byte)this.deviceIndex);
                                this.eventAggregator.GetEvent<FieldNotificationEvent>().Publish(notificationMessage);
                                this.logger.LogTrace($"IoDevice {this.deviceIndex}, data {notificationMessage.Data.ToString()}");

                                this.forceIoStatusPublish = false;

                                if (env.IsEnvironment("Bender"))
                                {
                                    notificationMessage.Destination = FieldMessageActor.InverterDriver;
                                    this.eventAggregator.GetEvent<FieldNotificationEvent>().Publish(notificationMessage);
                                }
                            }

                            var messageData = new IoReadMessage(
                                formatDataOperation,
                                fwRelease,
                                inputData,
                                outputData,
                                configurationData,
                                errorCode);
                            this.logger.LogTrace($"4:{messageData}: index {this.deviceIndex}");

                            this.CurrentStateMachine?.ProcessResponseMessage(messageData);
                            break;

                        case ShdFormatDataOperation.Ack:

                            var messageConfig = new IoReadMessage(
                                formatDataOperation,
                                fwRelease,
                                inputData,
                                outputData,
                                configurationData,
                                errorCode);
                            this.logger.LogTrace($"4: Configuration message={messageConfig}: index {this.deviceIndex}");

                            this.CurrentStateMachine?.ProcessResponseMessage(messageConfig);
                            break;

                        default:
                            break;
                    }
                }
            }
            while (!this.stoppingToken.IsCancellationRequested && !this.isDisposed);
        }

        public async Task SendIoCommandTaskFunction()
        {
            do
            {
                try
                {
                    if (this.ioCommandQueue.TryPeek(Timeout.Infinite, this.stoppingToken, out var shdMessage)
                         &&
                         shdMessage != null)
                    {
                        this.logger.LogTrace($"1:message={shdMessage}: index {this.deviceIndex}");
                    }

                    if (this.writeEnableEvent.Wait(Timeout.Infinite, this.stoppingToken))
                    {
                        if (this.ioTransport.IsConnected)
                        {
                            this.writeEnableEvent.Reset();

                            var isWriteSuccessful = false;

                            try
                            {
                                switch (shdMessage.CodeOperation)
                                {
                                    case ShdCodeOperation.Data:
                                        {
                                            var telegram = shdMessage.BuildSendTelegram(this.ioStatus.FwRelease);
                                            isWriteSuccessful = await this.ioTransport.WriteAsync(telegram, this.stoppingToken) == telegram.Length;

                                            this.logger.LogTrace($"3:message={shdMessage}: index {this.deviceIndex}");

                                            break;
                                        }

                                    case ShdCodeOperation.Configuration:
                                        {
                                            var telegram = shdMessage.BuildSendTelegram(this.ioStatus.FwRelease);
                                            isWriteSuccessful = await this.ioTransport.WriteAsync(telegram, this.stoppingToken) == telegram.Length;

                                            this.logger.LogTrace($"4:message={shdMessage}: index {this.deviceIndex}");

                                            break;
                                        }

                                    case ShdCodeOperation.SetIP:
                                        throw new NotImplementedException();

                                    default:
                                        if (Debugger.IsAttached)
                                        {
                                            Debugger.Break();
                                        }

                                        break;
                                }
                            }
                            catch (IoDriverException ex)
                            {
                                // connection error
                                this.logger.LogError(ex, $"Exception {ex.Message}, IoDriverExceptionCode={ex.IoDriverExceptionCode}");
                                using (var scope = this.serviceScopeFactory.CreateScope())
                                {
                                    var errorsProvider = scope.ServiceProvider.GetRequiredService<IErrorsProvider>();

                                    errorsProvider.RecordNew(DataModels.MachineErrorCode.IoDeviceConnectionError, this.bayNumber, ex.Message);
                                }
                                this.SendOperationErrorMessage(new IoExceptionFieldMessageData(ex, "IO Driver Connection Error", (int)IoDriverExceptionCode.DeviceNotConnected));
                                continue;
                            }

                            if (isWriteSuccessful)
                            {
                                this.ioCommandQueue.Dequeue(out _);
                            }
                            else
                            {
                                this.writeEnableEvent.Set();
                            }
                        }
                        else
                        {
                            Thread.Sleep(5);
                        }
                    }
                }
                catch (Exception ex) when (ex is OperationCanceledException || ex is ThreadAbortException)
                {
                    this.logger.LogDebug("Terminating I/O device write thread.");

                    return;
                }
                catch (Exception ex)
                {
                    this.SendOperationErrorMessage(
                        new IoExceptionFieldMessageData(ex, "IO Driver Fatal Error", (int)IoDriverExceptionCode.DeviceNotConnected),
                        isFatalError: true);

                    return;
                }
            }
            while (!this.stoppingToken.IsCancellationRequested && !this.isDisposed);
        }

        public void SendIoMessageData(object state)
        {
            if (!this.ioCommandQueue.Any(x => x.CodeOperation == ShdCodeOperation.Data))
            {
                var message = new IoWriteMessage(this.ioStatus.OutputData);

                this.ioCommandQueue.Enqueue(message);
            }
        }

        public void SendIoPublish(object state)
        {
            this.forceIoStatusPublish = true;
        }

        public void SendOperationErrorMessage(IIoExceptionFieldMessageData messageData, bool isFatalError = false)
        {
            var inverterUpdateStatusErrorNotification = new FieldNotificationMessage(
                messageData,
                "Io Driver Error",
                FieldMessageActor.Any,
                FieldMessageActor.IoDriver,
                FieldMessageType.IoDriverException,
                MessageStatus.OperationError,
                (byte)this.deviceIndex,
                isFatalError ? ErrorLevel.Fatal : ErrorLevel.Error);

            this.eventAggregator
                .GetEvent<FieldNotificationEvent>()
                .Publish(inverterUpdateStatusErrorNotification);
        }

        public async Task StartHardwareCommunicationsAsync()
        {
            this.logger.LogInformation($"1:Configure I/O device {this.deviceIndex}, tcp-endpoint={this.ipAddress}:{this.port}");

            try
            {
                await this.ioTransport.ConnectAsync(this.ipAddress, this.port);
            }
            catch (IoDriverException ex)
            {
                this.logger.LogError($"2:Exception: {ex.Message} while connecting to Modbus I/O master - ExceptionCode: {IoDriverExceptionCode.DeviceNotConnected};\nInner exception: {ex.InnerException.Message}");
                using (var scope = this.serviceScopeFactory.CreateScope())
                {
                    var errorsProvider = scope.ServiceProvider.GetRequiredService<IErrorsProvider>();

                    errorsProvider.RecordNew(DataModels.MachineErrorCode.IoDeviceConnectionError, this.bayNumber, ex.Message);
                }

                this.SendOperationErrorMessage(new IoExceptionFieldMessageData(ex, "IO Driver Exception", (int)IoDriverExceptionCode.DeviceNotConnected));
            }
            catch (Exception ex)
            {
                this.logger.LogCritical($"Fatal error while connecting to Modbus I/O master: {ex.Message} - ExceptionCode: {IoDriverExceptionCode.CreationFailure}");

                this.SendOperationErrorMessage(new IoExceptionFieldMessageData(ex, "IO Driver Exception", 0), isFatalError: true);

                return;
            }

            if (!this.ioTransport.IsConnected)
            {
                this.logger.LogError("3:Failed to connect to Modbus I/O master");
                using (var scope = this.serviceScopeFactory.CreateScope())
                {
                    var errorsProvider = scope.ServiceProvider.GetRequiredService<IErrorsProvider>();

                    errorsProvider.RecordNew(DataModels.MachineErrorCode.IoDeviceConnectionError, this.bayNumber);
                }

                this.SendOperationErrorMessage(new IoExceptionFieldMessageData(null, "Socket Transport failed to connect", (int)IoDriverExceptionCode.DeviceNotConnected));
            }
            else
            {
                this.logger.LogInformation($"Connection OK to I/O device {this.deviceIndex} on TCP address {this.ipAddress}:{this.port}");
            }

            try
            {
                this.ioReceiveTask.Start();

                this.ioSendTask.Start();
            }
            catch (Exception ex)
            {
                this.logger.LogCritical($"4:Exception: {ex.Message} while starting service hardware threads - ExceptionCode: {IoDriverExceptionCode.CreationFailure}");

                this.SendOperationErrorMessage(new IoExceptionFieldMessageData(ex, "IO Driver Exception", 0), isFatalError: true);

                return;
            }

            this.StartPollingIoMessage();
        }

        public void StartPollingIoMessage()
        {
            this.logger.LogTrace($"1:Create Timer to poll data");

            this.pollIoTimer?.Dispose();

            try
            {
                this.pollIoTimer = new Timer(this.SendIoMessageData, null, TimeSpan.Zero, TimeSpan.FromMilliseconds(IoPollingInterval));

                // why is this line commented?
                // this.publishIoTimer = new Timer(this.SendIoPublish, null, TimeSpan.Zero, TimeSpan.FromMilliseconds(IO_PUBLISH_INTERVAL));
            }
            catch (Exception ex)
            {
                this.logger.LogCritical($"2:Exception: {ex.Message} Timer Creation Failed");

                throw new IOException($"Exception: {ex.Message} Timer Creation Failed", ex);
            }
        }

        private void Dispose(bool disposing)
        {
            if (this.isDisposed)
            {
                return;
            }

            if (disposing)
            {
                this.DestroyStateMachine();

                this.ioTransport?.Dispose();
                this.pollIoTimer?.Dispose();
                this.writeEnableEvent?.Dispose();

                this.ioCommandQueue.Dispose();
            }

            this.isDisposed = true;
        }

        #endregion
    }
}
