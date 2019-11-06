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
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS.Utils.Enumerations;
using Ferretto.VW.MAS.Utils.Events;
using Ferretto.VW.MAS.Utils.Exceptions;
using Ferretto.VW.MAS.Utils.Messages;
using Ferretto.VW.MAS.Utils.Messages.FieldData;
using Ferretto.VW.MAS.Utils.Messages.FieldInterfaces;
using Ferretto.VW.MAS.Utils.Utilities;
using Microsoft.Extensions.Logging;
using Prism.Events;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS.IODriver
{
    internal sealed partial class IoDevice : IIoDevice, IDisposable
    {
        #region Fields

        private const int IoPollingInterval = 50;

        private readonly IoIndex deviceIndex;

        private readonly IEventAggregator eventAggregator;

        private readonly BlockingConcurrentQueue<IoWriteMessage> ioCommandQueue = new BlockingConcurrentQueue<IoWriteMessage>();

        private readonly Task ioReceiveTask;

        private readonly Task ioSendTask;

        private readonly IoStatus ioStatus;

        private readonly IIoTransport ioTransport;

        private readonly IPAddress ipAddress;

        private readonly bool isCarousel;

        private readonly ILogger logger;

        private readonly IoStatus mainIoDevice;

        private readonly int port;

        private readonly CancellationToken stoppingToken;

        private readonly ManualResetEventSlim writeEnableEvent;

        private IIoStateMachine currentStateMachine;

        private bool disposed;

        private bool forceIoStatusPublish;

        private Timer pollIoTimer;

        private byte[] receiveBuffer;

        #endregion

        #region Constructors

        public IoDevice(
            IEventAggregator eventAggregator,
            IIoDevicesProvider ioDeviceService,
            IIoTransport shdTransport,
            IPAddress ipAddress,
            int port,
            IoIndex index,
            bool isCarousel,
            ILogger logger,
            CancellationToken cancellationToken)
        {
            this.eventAggregator = eventAggregator;
            this.ipAddress = ipAddress;
            this.port = port;
            this.deviceIndex = index;
            this.logger = logger;
            this.ioTransport = shdTransport;
            this.stoppingToken = cancellationToken;
            this.isCarousel = isCarousel;

            this.writeEnableEvent = new ManualResetEventSlim(true);

            this.ioReceiveTask = new Task(async () => await this.ReceiveIoDataTaskFunction());
            this.ioSendTask = new Task(async () => await this.SendIoCommandTaskFunction());

            this.mainIoDevice = ioDeviceService.Devices.SingleOrDefault(s => s.IoIndex == IoIndex.IoDevice1);
            this.ioStatus = ioDeviceService.Devices.SingleOrDefault(s => s.IoIndex == index) ?? throw new ArgumentNullException(nameof(index));
        }

        #endregion

        #region Properties

        private IIoStateMachine CurrentStateMachine
        {
            get => this.currentStateMachine;
            set
            {
                if (this.currentStateMachine != value)
                {
                    this.currentStateMachine = value;
                }

                var objectName = string.Empty;
                if (this.currentStateMachine != null)
                {
                    objectName = this.currentStateMachine.GetType().Name;
                }

                var notificationMessageData = new MachineStatusActiveMessageData(MessageActor.IoDriver, objectName, MessageVerbosity.Info);
                var notificationMessage = new NotificationMessage(
                    notificationMessageData,
                    !string.IsNullOrEmpty(objectName) ? $"IoDriver current machine state {objectName}" : $"IoDriver current machine is null",
                    MessageActor.Any,
                    MessageActor.IoDriver,
                    MessageType.MachineStatusActive,
                    BayNumber.None,
                    BayNumber.None,
                    MessageStatus.OperationStart);

                this.eventAggregator.GetEvent<NotificationEvent>().Publish(notificationMessage);
            }
        }

        #endregion

        #region Methods

        public void DestroyStateMachine()
        {
            if (this.CurrentStateMachine is IDisposable stateMachine)
            {
                stateMachine.Dispose();
            }

            this.CurrentStateMachine = null;
        }

        public void Dispose()
        {
            this.Dispose(true);
        }

        public async Task ReceiveIoDataTaskFunction()
        {
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
                        await this.ioTransport.ConnectAsync();
                    }
                    catch (IoDriverException ex)
                    {
                        this.logger.LogError($"2:Exception: {ex.Message} while connecting to Modbus I/O master - ExceptionCode: {IoDriverExceptionCode.DeviceNotConnected};\nInner exception: {ex.InnerException.Message}");

                        this.SendMessage(new IoExceptionFieldMessageData(ex, "IO Driver Exception", (int)IoDriverExceptionCode.DeviceNotConnected));
                    }
                    catch (Exception ex)
                    {
                        this.logger.LogCritical($"2:Exception: {ex.Message} while connecting to Modbus I/O master - ExceptionCode: {IoDriverExceptionCode.CreationFailure}");

                        this.SendMessage(new IoExceptionFieldMessageData(ex, "IO Driver Exception", (int)IoDriverExceptionCode.CreationFailure));
                        throw new IOException($"Exception: {ex.Message} ReceiveIoDataTaskFunction Failed 1", ex);
                    }

                    if (!this.ioTransport.IsConnected)
                    {
                        this.logger.LogError("3:Socket Transport failed to connect");

                        var ex = new Exception();
                        this.SendMessage(new IoExceptionFieldMessageData(ex, "IO Driver Connection Error", (int)IoDriverExceptionCode.DeviceNotConnected));
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
                        var ex = new Exception();
                        this.SendMessage(new IoExceptionFieldMessageData(ex, "IO Driver Connection Error", (int)IoDriverExceptionCode.DeviceNotConnected));
                        continue;
                    }
                }
                catch (OperationCanceledException)
                {
                    this.logger.LogDebug("3:Method End - operation cancelled");
                    return;
                }
                catch (IoDriverException ex)
                {
                    // connection error
                    this.logger.LogError(ex, $"3:Exception: {ex.Message} while connecting to Modbus I/O master - ExceptionCode: {IoDriverExceptionCode.DeviceNotConnected}; Inner exception: {ex.InnerException?.Message ?? string.Empty}");
                    this.SendMessage(new IoExceptionFieldMessageData(ex, "IO Driver Connection Error", (int)IoDriverExceptionCode.DeviceNotConnected));
                    continue;
                }
                catch (Exception ex)
                {
                    this.logger.LogCritical($"3:Exception: {ex.Message} while reading async error - ExceptionCode: {IoDriverExceptionCode.CreationFailure}");

                    throw new IoDriverException($"Exception: {ex.Message} while reading async error", IoDriverExceptionCode.CreationFailure, ex);
                }

                this.receiveBuffer = this.receiveBuffer.AppendArrays(telegram, telegram.Length);

                // INFO: Byte 0 of read data contains packet length
                if (!this.IsHeaderValid(this.receiveBuffer[0]))
                {
                    // message error
                    this.logger.LogError($"5:IO Driver message length error: received {BitConverter.ToString(telegram)}: message {BitConverter.ToString(this.receiveBuffer)}");
                    var ex = new Exception();
                    this.SendMessage(new IoExceptionFieldMessageData(ex, "IO Driver Connection Error", (int)IoDriverExceptionCode.DeviceNotConnected));
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
                    this.logger.LogWarning($" extracted: count {extractedMessages.Count}: left bytes {this.receiveBuffer.Length}");
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
                        var ex = new Exception();
                        this.SendMessage(new IoExceptionFieldMessageData(ex, "IO Driver Connection Error", (int)IoDriverExceptionCode.DeviceNotConnected));
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
                        this.SendMessage(new IoExceptionFieldMessageData(ex, "IO Driver Connection Error", (int)IoDriverExceptionCode.DeviceNotConnected));
                        this.ioTransport.Disconnect();
                        break;
                    }

                    switch (formatDataOperation)
                    {
                        case ShdFormatDataOperation.Data:

                            inputData[(int)IoPorts.MicroCarterLeftSideBay] = !inputData[(int)IoPorts.MicroCarterLeftSideBay];

                            inputData[(int)IoPorts.MicroCarterRightSideBay] = !inputData[(int)IoPorts.MicroCarterRightSideBay];

                            inputData[(int)IoPorts.AntiIntrusionBarrierBay] = !inputData[(int)IoPorts.AntiIntrusionBarrierBay];

                            // INFO The mushroom signal must be inverted
                            inputData[(int)IoPorts.MushroomEmergency] = !inputData[(int)IoPorts.MushroomEmergency];

                            // INFO The sensor presence in bay must be inverted
                            inputData[(int)IoPorts.LoadingUnitInBay] = !inputData[(int)IoPorts.LoadingUnitInBay];

                            // INFO The sensor presence in lower bay must be inverted (NOT for carousel)
                            inputData[(int)IoPorts.LoadingUnitInLowerBay] = this.isCarousel ? inputData[(int)IoPorts.LoadingUnitInLowerBay] : !inputData[(int)IoPorts.LoadingUnitInLowerBay];

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
            while (!this.stoppingToken.IsCancellationRequested);
        }

        public async Task SendIoCommandTaskFunction()
        {
            do
            {
                IoWriteMessage shdMessage;
                try
                {
                    this.ioCommandQueue.TryPeek(Timeout.Infinite, this.stoppingToken, out shdMessage);

                    this.logger.LogTrace($"1:message={shdMessage}: index {this.deviceIndex}");
                }
                catch (OperationCanceledException)
                {
                    this.logger.LogDebug("2:Method End operation cancelled");

                    return;
                }

                if (this.writeEnableEvent.Wait(Timeout.Infinite, this.stoppingToken))
                {
                    if (this.ioTransport.IsConnected)
                    {
                        this.writeEnableEvent.Reset();

                        var result = false;

                        try
                        {
                            switch (shdMessage.CodeOperation)
                            {
                                case ShdCodeOperation.Data:
                                    {
                                        var telegram = shdMessage.BuildSendTelegram(this.ioStatus.FwRelease);
                                        result = await this.ioTransport.WriteAsync(telegram, this.stoppingToken) == telegram.Length;

                                        this.logger.LogTrace($"3:message={shdMessage}: index {this.deviceIndex}");

                                        break;
                                    }

                                case ShdCodeOperation.Configuration:
                                    {
                                        var telegram = shdMessage.BuildSendTelegram(this.ioStatus.FwRelease);
                                        result = await this.ioTransport.WriteAsync(telegram, this.stoppingToken) == telegram.Length;

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
                            this.SendMessage(new IoExceptionFieldMessageData(ex, "IO Driver Connection Error", (int)IoDriverExceptionCode.DeviceNotConnected));
                            continue;
                        }

                        if (result)
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
            while (!this.stoppingToken.IsCancellationRequested);
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

        public void SendMessage(IFieldMessageData messageData)
        {
            var inverterUpdateStatusErrorNotification = new FieldNotificationMessage(
                messageData,
                "Io Driver Error",
                FieldMessageActor.Any,
                FieldMessageActor.IoDriver,
                FieldMessageType.IoDriverException,
                MessageStatus.OperationError,
                (byte)this.deviceIndex,
                ErrorLevel.Critical);

            this.eventAggregator.GetEvent<FieldNotificationEvent>().Publish(inverterUpdateStatusErrorNotification);
        }

        public async Task StartHardwareCommunications()
        {
            this.logger.LogInformation($"1:Configure I/O device {this.deviceIndex}, tcp-endpoint={this.ipAddress}:{this.port}");

            this.ioTransport.Configure(this.ipAddress, this.port);

            try
            {
                await this.ioTransport.ConnectAsync();
            }
            catch (IoDriverException ex)
            {
                this.logger.LogError($"2:Exception: {ex.Message} while connecting to Modbus I/O master - ExceptionCode: {IoDriverExceptionCode.DeviceNotConnected};\nInner exception: {ex.InnerException.Message}");

                this.SendMessage(new IoExceptionFieldMessageData(ex, "IO Driver Exception", (int)IoDriverExceptionCode.DeviceNotConnected));
            }
            catch (Exception ex)
            {
                this.logger.LogCritical($"2:Exception: {ex.Message} while connecting to Modbus I/O master - ExceptionCode: {IoDriverExceptionCode.CreationFailure}");

                this.SendMessage(new IoExceptionFieldMessageData(ex, "IO Driver Exception", 0));
                throw new IOException($"Exception: {ex.Message} StartHardwareCommunications Failed 1", ex);
            }

            if (!this.ioTransport.IsConnected)
            {
                this.logger.LogError("3:Failed to connect to Modbus I/O master");

                var ex = new Exception();
                this.SendMessage(new IoExceptionFieldMessageData(ex, "Socket Transport failed to connect", (int)IoDriverExceptionCode.DeviceNotConnected));
            }
            else
            {
                this.logger.LogInformation($"3:Connection OK ipAddress={this.ipAddress}:Port={this.port}");
            }

            try
            {
                this.ioReceiveTask.Start();

                this.ioSendTask.Start();
            }
            catch (Exception ex)
            {
                this.logger.LogCritical($"4:Exception: {ex.Message} while starting service hardware threads - ExceptionCode: {IoDriverExceptionCode.CreationFailure}");

                this.SendMessage(new IoExceptionFieldMessageData(ex, "IO Driver Exception", 0));
                throw new IOException($"Exception: {ex.Message} StartHardwareCommunications Failed 2", ex);
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
            if (this.disposed)
            {
                return;
            }

            if (disposing)
            {
                this.DestroyStateMachine();

                this.pollIoTimer?.Dispose();
                this.writeEnableEvent?.Dispose();
            }

            this.disposed = true;
        }

        #endregion
    }
}
