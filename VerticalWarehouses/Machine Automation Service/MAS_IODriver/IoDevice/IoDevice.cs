using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.IODriver.Enumerations;
using Ferretto.VW.MAS.IODriver.Interface;
using Ferretto.VW.MAS.IODriver.IoDevice.Interfaces;
using Ferretto.VW.MAS.Utils.Enumerations;
using Ferretto.VW.MAS.Utils.Events;
using Ferretto.VW.MAS.Utils.Exceptions;
using Ferretto.VW.MAS.Utils.Messages;
using Ferretto.VW.MAS.Utils.Messages.FieldData;
using Ferretto.VW.MAS.Utils.Messages.FieldInterfaces;
using Ferretto.VW.MAS.Utils.Utilities;
using static Ferretto.VW.MAS.Utils.Utilities.BufferUtility;
using Microsoft.Extensions.Logging;
using Prism.Events;
using System.Collections.Generic;

namespace Ferretto.VW.MAS.IODriver.IoDevice
{
    public partial class IoDevice : IIoDevice
    {
        #region Fields

        private const int IO_POLLING_INTERVAL = 50;

        private readonly IEventAggregator eventAggregator;

        private readonly BlockingConcurrentQueue<IoSHDWriteMessage> ioCommandQueue;

        private readonly Task ioReceiveTask;

        private readonly Task ioSendTask;

        private readonly IoSHDStatus ioSHDStatus;

        private readonly ILogger logger;

        private readonly ISHDTransport shdTransport;

        private IIoStateMachine currentStateMachine;

        private bool disposed;

        private bool forceIoStatusPublish;

        private IoIndex index;

        private IPAddress ipAddress;

        private Timer pollIoTimer;

        private int port;

        private CancellationToken stoppingToken;

        private byte[] ReceiveBuffer;

        #endregion

        #region Constructors

        public IoDevice(IEventAggregator eventAggregator, ISHDTransport shdTransport, IPAddress ipAddress, int port, IoIndex index, ILogger logger)
        {
            logger.LogTrace("1:Method Start");

            this.eventAggregator = eventAggregator;
            this.ipAddress = ipAddress;
            this.port = port;
            this.index = index;
            this.logger = logger;
            this.shdTransport = shdTransport;

            this.ioSHDStatus = new IoSHDStatus();

            this.ioCommandQueue = new BlockingConcurrentQueue<IoSHDWriteMessage>();

            this.ioReceiveTask = new Task(async () => await this.ReceiveIoDataTaskFunction());
            this.ioSendTask = new Task(async () => await this.SendIoCommandTaskFunction());
        }

        #endregion

        #region Destructors

        ~IoDevice()
        {
            this.Dispose(false);
        }

        #endregion

        #region Methods

        public void DestroyStateMachine()
        {
            this.currentStateMachine?.Dispose();
            this.currentStateMachine = null;
        }

        public async Task ReceiveIoDataTaskFunction()
        {
            this.logger.LogTrace("1:Method Start");

            var formatDataOperation = SHDFormatDataOperation.Data;
            byte fwRelease = 0x00;
            byte errorCode = 0x00;
            var inputData = new bool[16];
            var outputData = new bool[8];
            var configurationData = new byte[25];

            do
            {
                if(!this.shdTransport.IsConnected)
                {
                    try
                    {
                        this.ReceiveBuffer = null;
                        await this.shdTransport.ConnectAsync();
                    }
                    catch (IoDriverException ex)
                    {
                        this.logger.LogError($"2:Exception: {ex.Message} while connecting to Modbus I/O master - ExceptionCode: {IoDriverExceptionCode.DeviceNotConnected}");

                        this.SendMessage(new IoExceptionFieldMessageData(ex, "IO Driver Exception", (int)IoDriverExceptionCode.DeviceNotConnected));
                    }
                    catch (Exception ex)
                    {
                        this.logger.LogCritical($"2:Exception: {ex.Message} while connecting to Modbus I/O master - ExceptionCode: {IoDriverExceptionCode.CreationFailure}");

                        this.SendMessage(new IoExceptionFieldMessageData(ex, "IO Driver Exception", (int)IoDriverExceptionCode.CreationFailure));
                        throw new IOException($"Exception: {ex.Message} ReceiveIoDataTaskFunction Failed 1", ex);
                    }
                    if (!this.shdTransport.IsConnected)
                    {
                        this.logger.LogError("3:Socket Transport failed to connect");

                        var ex = new Exception();
                        this.SendMessage(new IoExceptionFieldMessageData(ex, "IO Driver Connection Error", (int)IoDriverExceptionCode.DeviceNotConnected));
                        continue;
                    }
                }
                // socket connected
                var nBytesReceived = 0;
                byte[] telegram;
                try
                {
                    telegram = await this.shdTransport.ReadAsync(this.stoppingToken);

                    if (telegram == null || telegram.Length == 0)
                    {
                        // connection error
                        this.logger.LogError($"4:IO Driver message is null");
                        var ex = new Exception();
                        this.SendMessage(new IoExceptionFieldMessageData(ex, "IO Driver Connection Error", (int)IoDriverExceptionCode.DeviceNotConnected));
                        this.shdTransport.Disconnect();
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
                    this.SendMessage(new IoExceptionFieldMessageData(ex, "IO Driver Connection Error", (int)IoDriverExceptionCode.DeviceNotConnected));
                    continue;
                }
                catch (Exception ex)
                {
                    this.logger.LogCritical($"3:Exception: {ex.Message} while reading async error - ExceptionCode: {IoDriverExceptionCode.CreationFailure}");

                    throw new IoDriverException($"Exception: {ex.Message} while reading async error", IoDriverExceptionCode.CreationFailure, ex);
                }
                this.ReceiveBuffer = this.ReceiveBuffer.AppendArrays(telegram, telegram.Length);

                //INFO: Byte 0 of read data contains packet length
                if (!(this.ReceiveBuffer[0] == 3 || this.ReceiveBuffer[0] == 15 || this.ReceiveBuffer[0] == 26))
                {
                    // message error
                    this.logger.LogError($"5:IO Driver message length error: received {BitConverter.ToString(telegram)}: message {BitConverter.ToString(this.ReceiveBuffer)}");
                    var ex = new Exception();
                    this.SendMessage(new IoExceptionFieldMessageData(ex, "IO Driver Connection Error", (int)IoDriverExceptionCode.DeviceNotConnected));
                    this.shdTransport.Disconnect();
                    continue;
                }
                if (this.ReceiveBuffer.Length < this.ReceiveBuffer[0])
                {
                    // this is not an error: we try to recover from messages received in more pieces
                    this.logger.LogWarning($"5:IO Driver message is not complete: received {BitConverter.ToString(telegram)}: message {BitConverter.ToString(this.ReceiveBuffer)}");
                    continue;
                }
                var extractedMessages = GetMessagesWithHeaderLengthToEnqueue(ref this.ReceiveBuffer, 3, 0, 0);
                if(this.ReceiveBuffer.Length > 0)
                {
                    this.logger.LogWarning($" extracted: count {extractedMessages.Count}: left bytes {this.ReceiveBuffer.Length}");
                }

                foreach (var extractedMessage in extractedMessages)
                {
                    if ((extractedMessage[1] == 0x10 && !(extractedMessage[0] == 15 || extractedMessage[0] == 3))    // length is not valid for old release   
                        || (extractedMessage[1] == 0x11 && !(extractedMessage[0] == 26 || extractedMessage[0] == 3))    // length is not valid  for new release 
                        )
                    {
                        // message error
                        this.logger.LogError($"5:IO Driver message error: received {BitConverter.ToString(telegram)}: message {BitConverter.ToString(this.ReceiveBuffer)}");
                        var ex = new Exception();
                        this.SendMessage(new IoExceptionFieldMessageData(ex, "IO Driver Connection Error", (int)IoDriverExceptionCode.DeviceNotConnected));
                        this.shdTransport.Disconnect();
                        break;
                    }
                    try
                    {

                        this.ParsingDataBytes(
                            extractedMessage,
                            out nBytesReceived,
                            out formatDataOperation,
                            out fwRelease,
                            ref inputData,
                            ref outputData,
                            out configurationData,
                            out errorCode);
                    }
                    catch (Exception ex)
                    {
                        // message error
                        this.logger.LogError($"6:IO Driver message error: received {BitConverter.ToString(telegram)}: message {BitConverter.ToString(extractedMessage)}");
                        this.SendMessage(new IoExceptionFieldMessageData(ex, "IO Driver Connection Error", (int)IoDriverExceptionCode.DeviceNotConnected));
                        this.shdTransport.Disconnect();
                        break;
                    }

                    // message ok
                    lock (this.ioSHDStatus)
                    {
                        this.ioSHDStatus.FwRelease = fwRelease;
                    }

                    switch (formatDataOperation)
                    {
                        case SHDFormatDataOperation.Data:

                            if (this.ioSHDStatus.UpdateInputStates(inputData) || this.forceIoStatusPublish)
                            {
                                var data = new SensorsChangedFieldMessageData();
                                data.SensorsStates = inputData;
                                var notificationMessage = new FieldNotificationMessage(
                                    data,
                                    "Update IO sensors",
                                    FieldMessageActor.FiniteStateMachines,
                                    FieldMessageActor.IoDriver,
                                    FieldMessageType.SensorsChanged,
                                    MessageStatus.OperationExecuting,
                                    ErrorLevel.NoError,
                                    (byte)this.index);
                                this.eventAggregator.GetEvent<FieldNotificationEvent>().Publish(notificationMessage);

                                this.forceIoStatusPublish = false;
                            }

                            var messageData = new IoSHDReadMessage(
                                formatDataOperation,
                                fwRelease,
                                inputData,
                                outputData,
                                configurationData,
                                errorCode);
                            this.logger.LogTrace($"4:{messageData}");

                            this.currentStateMachine?.ProcessResponseMessage(messageData);

                            break;

                        case SHDFormatDataOperation.Ack:

                            var messageConfig = new IoSHDReadMessage(
                                formatDataOperation,
                                fwRelease,
                                inputData,
                                outputData,
                                configurationData,
                                errorCode);
                            this.logger.LogTrace($"4: Configuration message={messageConfig}");

                            this.currentStateMachine?.ProcessResponseMessage(messageConfig);

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
                IoSHDWriteMessage shdMessage;
                try
                {
                    this.ioCommandQueue.TryPeek(Timeout.Infinite, this.stoppingToken, out shdMessage);

                    this.logger.LogTrace($"1:message={shdMessage}");
                }
                catch (OperationCanceledException)
                {
                    this.logger.LogDebug("2:Method End operation cancelled");

                    return;
                }
                try
                {
                    byte[] telegram;
                    switch (shdMessage.CodeOperation)
                    {
                        case SHDCodeOperation.Data:
                            if (shdMessage.ValidOutputs)
                            {
                                telegram = shdMessage.BuildSendTelegram(this.ioSHDStatus.FwRelease);
                                await this.shdTransport.WriteAsync(telegram, this.stoppingToken);

                                this.logger.LogTrace($"3:message={shdMessage}");
                            }
                            break;

                        case SHDCodeOperation.Configuration:
                            {
                                telegram = shdMessage.BuildSendTelegram(this.ioSHDStatus.FwRelease);
                                await this.shdTransport.WriteAsync(telegram, this.stoppingToken);

                                this.logger.LogTrace($"4:message={shdMessage}");
                            }
                            break;

                        case SHDCodeOperation.SetIP:
                            {
                                // TODO
                            }
                            break;

                        default:
                            break;
                    }
                }
                catch(IoDriverException ex)
                {
                    // connection error
                    this.logger.LogError($"Exception {ex.Message}, IoDriverExceptionCode={ex.IoDriverExceptionCode}");
                    this.SendMessage(new IoExceptionFieldMessageData(ex, "IO Driver Connection Error", (int)IoDriverExceptionCode.DeviceNotConnected));
                    continue;
                }
                this.ioCommandQueue.Dequeue(out var consumedMessage);
            }
            while (!this.stoppingToken.IsCancellationRequested);
        }

        public void SendIoMessageData(object state)
        {
            var message = new IoSHDWriteMessage(this.ioSHDStatus.OutputData);

            this.ioCommandQueue.Enqueue(message);
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
            ErrorLevel.Critical,
            (byte)this.index);

            this.eventAggregator?.GetEvent<FieldNotificationEvent>().Publish(inverterUpdateStatusErrorNotification);
        }

        public async Task StartHardwareCommunications()
        {
            this.logger.LogTrace($"1:ioAddress={this.ipAddress}:ioPort={this.port}");

            this.shdTransport.Configure(this.ipAddress, this.port);

            try
            {
                await this.shdTransport.ConnectAsync();
            }
            catch(IoDriverException ex)
            {
                this.logger.LogError($"2:Exception: {ex.Message} while connecting to Modbus I/O master - ExceptionCode: {IoDriverExceptionCode.DeviceNotConnected}");

                this.SendMessage(new IoExceptionFieldMessageData(ex, "IO Driver Exception", (int)IoDriverExceptionCode.DeviceNotConnected));
            }
            catch (Exception ex)
            {
                this.logger.LogCritical($"2:Exception: {ex.Message} while connecting to Modbus I/O master - ExceptionCode: {IoDriverExceptionCode.CreationFailure}");

                this.SendMessage(new IoExceptionFieldMessageData(ex, "IO Driver Exception", 0));
                throw new IOException($"Exception: {ex.Message} StartHardwareCommunications Failed 1", ex);
            }

            if (!this.shdTransport.IsConnected)
            {
                this.logger.LogError("3:Failed to connect to Modbus I/O master");

                var ex = new Exception();
                this.SendMessage(new IoExceptionFieldMessageData(ex, "Socket Transport failed to connect", (int)IoDriverExceptionCode.DeviceNotConnected));
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
            this.logger.LogDebug($"1:Create Timer to poll data");

            this.pollIoTimer?.Dispose();

            try
            {
                this.pollIoTimer = new Timer(this.SendIoMessageData, null, TimeSpan.Zero, TimeSpan.FromMilliseconds(IO_POLLING_INTERVAL));
            }
            catch (Exception ex)
            {
                this.logger.LogCritical($"2:Exception: {ex.Message} Timer Creation Failed");

                throw new IOException($"Exception: {ex.Message} Timer Creation Failed", ex);
            }
        }

        protected void Dispose(bool disposing)
        {
            if (this.disposed)
            {
                return;
            }

            if (disposing)
            {
                this.pollIoTimer?.Dispose();
            }

            this.disposed = true;
        }

#endregion
    }
}
