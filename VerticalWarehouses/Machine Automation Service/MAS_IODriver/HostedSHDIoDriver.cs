using System;
using System.Collections;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Ferretto.VW.Common_Utils.Messages.Enumerations;
using Ferretto.VW.MAS_DataLayer.Enumerations;
using Ferretto.VW.MAS_DataLayer.Interfaces;
using Ferretto.VW.MAS_IODriver.Enumerations;
using Ferretto.VW.MAS_IODriver.Interface;
using Ferretto.VW.MAS_IODriver.StateMachines.PowerUp;
using Ferretto.VW.MAS_Utils.Enumerations;
using Ferretto.VW.MAS_Utils.Events;
using Ferretto.VW.MAS_Utils.Exceptions;
using Ferretto.VW.MAS_Utils.Messages;
using Ferretto.VW.MAS_Utils.Utilities;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Prism.Events;

namespace Ferretto.VW.MAS_IODriver
{
    public partial class HostedSHDIoDriver : BackgroundService
    {
        #region Fields

        private const int IO_POLLING_INTERVAL = 100;  // 50

        private readonly BlockingConcurrentQueue<FieldCommandMessage> commandQueue;

        private readonly Task commandReceiveTask;

        private readonly IDataLayerConfigurationValueManagment dataLayerConfigurationValueManagement;

        private readonly IEventAggregator eventAggregator;

        //x private readonly BlockingConcurrentQueue<IoMessage> ioCommandQueue;
        //private readonly BlockingConcurrentQueue<IoSHDMessage> ioCommandQueue;
        private readonly BlockingConcurrentQueue<IoSHDWriteMessage> ioCommandQueue;

        private readonly Task ioReceiveTask;

        private readonly Task ioSendTask;

        private readonly IoSHDStatus ioSHDStatus;  // <--

        private readonly IoStatus ioStatus;

        private readonly ILogger logger;

        //x private readonly IModbusTransport modbusTransport;

        private readonly BlockingConcurrentQueue<FieldNotificationMessage> notificationQueue;

        private readonly Task notificationReceiveTask;

        // private readonly ManualResetEventSlim pollIoEvent;

        private readonly ISHDTransport shdTransport;  // <--

        private IIoStateMachine currentStateMachine;

        private bool disposed;

        // used only in ReceiveIoDataTaskFunction
        private bool[] inputData;

        private int nMessages;

        // used only in ReceiveIoDataTaskFunction
        private bool[] outputData;

        private Timer pollIoTimer;

        private CancellationToken stoppingToken;

        #endregion

        #region Constructors

        public HostedSHDIoDriver(IEventAggregator eventAggregator,
            ISHDTransport shdTransport,
            IDataLayerConfigurationValueManagment dataLayerConfigurationValueManagement,
            ILogger<HostedSHDIoDriver> logger)
        {
            logger.LogDebug("1:Method Start");

            this.logger = logger;
            this.eventAggregator = eventAggregator;
            this.dataLayerConfigurationValueManagement = dataLayerConfigurationValueManagement;
            this.shdTransport = shdTransport;

            this.outputData = new bool[8];
            this.inputData = new bool[16];

            this.ioStatus = new IoStatus();  // remove

            this.ioSHDStatus = new IoSHDStatus();
            this.nMessages = 0;

            //this.ioCommandQueue = new BlockingConcurrentQueue<IoSHDMessage>();
            this.ioCommandQueue = new BlockingConcurrentQueue<IoSHDWriteMessage>();

            this.commandQueue = new BlockingConcurrentQueue<FieldCommandMessage>();

            this.notificationQueue = new BlockingConcurrentQueue<FieldNotificationMessage>();

            this.commandReceiveTask = new Task(() => this.CommandReceiveTaskFunction());
            this.notificationReceiveTask = new Task(async () => await this.NotificationReceiveTaskFunction());
            this.ioReceiveTask = new Task(async () => await this.ReceiveIoDataTaskFunction());
            this.ioSendTask = new Task(async () => await this.SendIoCommandTaskFunction());

            this.InitializeMethodSubscriptions();

            this.logger.LogDebug("2:Method End");
        }

        #endregion

        #region Destructors

        ~HostedSHDIoDriver()
        {
            this.Dispose(false);
        }

        #endregion

        #region Methods

        protected void Dispose(bool disposing)
        {
            if (this.disposed)
            {
                return;
            }

            if (disposing)
            {
                this.pollIoTimer?.Dispose();
                base.Dispose();
            }

            this.disposed = true;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            this.logger.LogDebug("1:Method Start");

            this.stoppingToken = stoppingToken;

            this.logger.LogDebug("2:Starting Tasks");
            try
            {
                this.commandReceiveTask.Start();
                this.notificationReceiveTask.Start();
            }
            catch (Exception ex)
            {
                this.logger.LogCritical($"3:Exception: {ex.Message} while starting service threads");

                throw new IOException($"Exception: {ex.Message} while starting service threads", ex);
            }

            this.logger.LogDebug("4:Method End");

            return Task.CompletedTask;
        }

        private bool[] ByteArrayToBoolArray(byte b)
        {
            const int N_BITS8 = 8;
            var t = new BitArray(new byte[] { b });
            var bits = new bool[N_BITS8];
            t.CopyTo(bits, 0);
            return bits;
        }

        private void CommandReceiveTaskFunction()
        {
            this.logger.LogDebug("1:Method Start");

            // PowerUp
            this.currentStateMachine = new PowerUpStateMachine(this.ioCommandQueue, this.ioSHDStatus, this.eventAggregator, this.logger);
            this.currentStateMachine.Start();

            do
            {
                FieldCommandMessage receivedMessage;
                try
                {
                    this.commandQueue.TryDequeue(Timeout.Infinite, this.stoppingToken, out receivedMessage);

                    this.logger.LogTrace($"3:Type={receivedMessage.Type}:Destination={receivedMessage.Destination}:receivedMessage={receivedMessage}");
                }
                catch (OperationCanceledException)
                {
                    this.logger.LogDebug("4:Method End - Operation Canceled");

                    return;
                }
                this.logger.LogTrace($"4:Filed Command received: {receivedMessage.Type}, destination: {receivedMessage.Destination}");
                if (this.currentStateMachine != null)
                {
                    var errorNotification = new FieldNotificationMessage(null, "I/O operation already in progress", FieldMessageActor.Any,
                        FieldMessageActor.IoDriver, receivedMessage.Type, MessageStatus.OperationError, ErrorLevel.Error);

                    this.logger.LogTrace($"6:Type={errorNotification.Type}:Destination={errorNotification.Destination}:Status={errorNotification.Status}");

                    this.eventAggregator?.GetEvent<FieldNotificationEvent>().Publish(errorNotification);
                    continue;
                }
                switch (receivedMessage.Type)
                {
                    case FieldMessageType.SwitchAxis:
                        this.ExecuteSwitchAxis(receivedMessage);
                        break;

                    case FieldMessageType.IoReset:
                        this.ExecuteIoReset();
                        break;
                }
            } while (!this.stoppingToken.IsCancellationRequested);

            this.logger.LogDebug("6:Method End");
        }

        private void InitializeMethodSubscriptions()
        {
            this.logger.LogTrace("1:Commands Subscription");

            var commandEvent = this.eventAggregator.GetEvent<FieldCommandEvent>();
            commandEvent.Subscribe(commandMessage => { this.commandQueue.Enqueue(commandMessage); },
                ThreadOption.PublisherThread,
                false,
                commandMessage => commandMessage.Destination == FieldMessageActor.IoDriver || commandMessage.Destination == FieldMessageActor.Any);

            this.logger.LogTrace("1:Notifications Subscription");

            var notificationEvent = this.eventAggregator.GetEvent<FieldNotificationEvent>();
            notificationEvent.Subscribe(notificationMessage => { this.notificationQueue.Enqueue(notificationMessage); },
                ThreadOption.PublisherThread,
                false,
                notificationMessage => notificationMessage.Destination == FieldMessageActor.IoDriver || notificationMessage.Destination == FieldMessageActor.Any);
        }

        private async Task NotificationReceiveTaskFunction()
        {
            this.logger.LogDebug("1:Method Start");

            do
            {
                FieldNotificationMessage receivedMessage;
                try
                {
                    this.notificationQueue.TryDequeue(Timeout.Infinite, this.stoppingToken, out receivedMessage);
                    this.logger.LogTrace($"2:Notification received: {receivedMessage.Type}, destination: {receivedMessage.Destination}, source: {receivedMessage.Source}, status: {receivedMessage.Status}");
                }
                catch (OperationCanceledException)
                {
                    this.logger.LogDebug("3:Method End operation cancelled");

                    return;
                }
                this.logger.LogTrace($"Notification received: {receivedMessage.Type}, {receivedMessage.Status}, destination: {receivedMessage.Destination}");
                switch (receivedMessage.Type)
                {
                    case FieldMessageType.DataLayerReady:
                        await this.StartHardwareCommunications();
                        this.StartPollingIoMessage();
                        break;

                    case FieldMessageType.IoPowerUp:
                    case FieldMessageType.SwitchAxis:
                        if (receivedMessage.Status == MessageStatus.OperationEnd &&
                            receivedMessage.ErrorLevel == ErrorLevel.NoError)
                        {
                            this.currentStateMachine?.Dispose();
                            this.currentStateMachine = null;
                        }
                        break;
                }
            } while (!this.stoppingToken.IsCancellationRequested);

            this.logger.LogDebug("4:Method End");
        }

        /// <summary>
        /// Parsing the incoming telegram from the RemoteIO device.
        /// </summary>
        private void parsingDataBytes(byte[] telegram, out int nBytesReceived, out SHDFormatDataOperation formatDataOperation, out byte fwRelease, ref bool[] inputs, ref bool[] outputs, out byte[] configurationData, out byte errorCode)
        {
            const int N_BYTES8 = 8;
            const int N_BITS8 = 8;
            const int N_BITS16 = 16;

            const int NBYTES_TELEGRAM_DATA = 15;
            const int NBYTES_TELEGRAM_ACK = 3;

            Array.Clear(inputs, 0, inputs.Length);
            Array.Clear(outputs, 0, outputs.Length);

            configurationData = null;
            formatDataOperation = SHDFormatDataOperation.Data;
            fwRelease = 0x00;
            errorCode = 0x00;
            nBytesReceived = 0;

            if (telegram == null)
                return;

            byte codeOp = 0x00;

            // Parsing incoming telegram
            try
            {
                // N Bytes
                nBytesReceived = telegram[0];

                // Get the fw release
                fwRelease = telegram[1];
                switch (fwRelease)
                {
                    case 0x10: // old release
                        switch (nBytesReceived)
                        {
                            case NBYTES_TELEGRAM_DATA:
                                // Fw release
                                fwRelease = telegram[1];
                                // Code op
                                codeOp = telegram[2];
                                // Error code
                                errorCode = telegram[3];

                                // Payload output
                                var payloadOutput = telegram[4];

                                this.logger.LogDebug($"telegram[4] = {telegram[4]}");

                                outputs = new bool[N_BITS8];
                                Array.Copy(this.ByteArrayToBoolArray(payloadOutput), outputs, N_BITS8);

                                // Payload input (Low byte)
                                var payloadInputLow = telegram[5];
                                // Payload input (High byte)
                                var payloadInputHigh = telegram[6];

                                inputs = new bool[N_BITS16];
                                Array.Copy(this.ByteArrayToBoolArray(payloadInputLow), inputs, N_BITS8);
                                Array.Copy(this.ByteArrayToBoolArray(payloadInputHigh), 0, inputs, N_BITS8, N_BITS8);

                                // Configuration data
                                configurationData = new byte[N_BYTES8];
                                Array.Copy(telegram, 7, configurationData, 0, N_BYTES8);

                                // Format data operation
                                formatDataOperation = SHDFormatDataOperation.Data;

                                break;

                            case NBYTES_TELEGRAM_ACK:
                                // Fw release
                                fwRelease = telegram[1];
                                // Code op
                                codeOp = telegram[2];

                                // Format data operation
                                formatDataOperation = SHDFormatDataOperation.Ack;

                                break;

                            default:
                                //TODO throw an exception for the invalid telegram
                                break;
                        }

                        break;

                    case 0x11: // new release
                        switch (nBytesReceived)
                        {
                            case NBYTES_TELEGRAM_DATA + 10:  // 25
                                                             // Fw release
                                fwRelease = telegram[1];
                                // Code op
                                codeOp = telegram[2];

                                // Alignment
                                var alignment = telegram[3];

                                // Error code
                                errorCode = telegram[4];

                                // Payload output
                                var payloadOutput = telegram[5];
                                outputs = new bool[N_BITS8];
                                Array.Copy(this.ByteArrayToBoolArray(payloadOutput), outputs, N_BITS8);

                                // Payload input (Low byte)
                                var payloadInputLow = telegram[6];
                                // Payload input (High byte)
                                var payloadInputHigh = telegram[7];

                                inputs = new bool[N_BITS16];
                                Array.Copy(this.ByteArrayToBoolArray(payloadInputLow), inputs, N_BITS8);
                                Array.Copy(this.ByteArrayToBoolArray(payloadInputHigh), 0, inputs, N_BITS8, N_BITS8);

                                // Configuration data
                                configurationData = new byte[17];
                                Array.Copy(telegram, 8, configurationData, 0, 17);

                                // Format data operation
                                formatDataOperation = SHDFormatDataOperation.Data;

                                break;

                            case NBYTES_TELEGRAM_ACK:
                                // Fw release
                                fwRelease = telegram[1];
                                // Code op
                                codeOp = telegram[2];

                                // Format data operation
                                formatDataOperation = SHDFormatDataOperation.Ack;

                                break;

                            default:
                                //TODO throw an exception for the invalid telegram
                                break;
                        }

                        break;

                    default:
                        break;
                }
            }
            catch (Exception exc)
            {
                throw new IOException($"Exception: {exc.Message} while parsing the received telegram", exc);
            }
        }

        private async Task ReceiveIoDataTaskFunction()
        {
            this.logger.LogDebug("1:Method Start");

            var formatDataOperation = SHDFormatDataOperation.Data;
            byte fwRelease = 0x00;
            byte errorCode = 0x00;
            var configurationData = new byte[25];

            do
            {
                //TODO Attention: handle the message fragmentation

                var nBytesReceived = 0;
                try
                {
                    var telegram = await this.shdTransport.ReadAsync(this.stoppingToken);

                    if (telegram.Length == 0)
                    {
                        continue;
                    }

                    this.logger.LogDebug($" <--- Read message    Parsing telegram...");

                    this.parsingDataBytes(
                        telegram,
                        out nBytesReceived,
                        out formatDataOperation,
                        out fwRelease,
                        ref this.inputData,
                        ref this.outputData,
                        out configurationData,
                        out errorCode);

                    this.logger.LogDebug($" <--- Read message    outputData[0]={this.outputData[0]}");
                }
                catch (Exception ex)
                {
                    this.logger.LogCritical($"3:Exception: {ex.Message} while reading async error - ExceptionCode: {IoDriverExceptionCode.CreationFailure}");

                    throw new IoDriverException($"Exception: {ex.Message} while reading async error", IoDriverExceptionCode.CreationFailure, ex);
                }

                this.ioSHDStatus.FwRelease = fwRelease;

                switch (formatDataOperation)
                {
                    case SHDFormatDataOperation.Data:

                        // TODO Check the fault output lines status

                        // update IoStatus
                        this.ioSHDStatus.UpdateStates(this.inputData, this.outputData);

                        //var messageData = new IoSHDMessage(this.inputData, this.outputData);
                        var messageData = new IoSHDReadMessage(
                            formatDataOperation,
                            fwRelease,
                            this.inputData,
                            this.outputData,
                            configurationData,
                            errorCode);
                        this.logger.LogTrace($"4:{messageData}");

                        //this.currentStateMachine?.ProcessMessage(messageData);
                        this.currentStateMachine?.ProcessResponseMessage(messageData);

                        break;

                    case SHDFormatDataOperation.Ack:

                        var messageConfig = new IoSHDReadMessage(
                            formatDataOperation,
                            fwRelease,
                            this.inputData,
                            this.outputData,
                            configurationData,
                            errorCode);
                        this.logger.LogTrace($"4: Configuration message > {messageConfig}");

                        //this.currentStateMachine?.ProcessMessage(messageData);
                        this.currentStateMachine?.ProcessResponseMessage(messageConfig);

                        break;

                    default:
                        break;
                }
            } while (!this.stoppingToken.IsCancellationRequested);

            this.logger.LogDebug("5:Method End");
        }

        private async Task SendIoCommandTaskFunction()
        {
            this.logger.LogDebug("1:Method Start");

            do
            {
                //var shdMessage = new IoSHDMessage();
                var shdMessage = new IoSHDWriteMessage();
                try
                {
                    this.ioCommandQueue.TryDequeue(Timeout.Infinite, this.stoppingToken, out shdMessage);

                    this.logger.LogDebug($"2:message={shdMessage}");
                }
                catch (OperationCanceledException)
                {
                    this.logger.LogDebug("3:Method End operation cancelled");

                    return;
                }

                byte[] telegram;
                switch (shdMessage.CodeOperation)
                {
                    case SHDCodeOperation.Data:
                        if (shdMessage.ValidOutputs)
                        {
                            telegram = shdMessage.BuildSendTelegram(this.ioSHDStatus.FwRelease);
                            await this.shdTransport.WriteAsync(telegram, this.stoppingToken);

                            this.logger.LogTrace($"4:message={shdMessage}");
                        }
                        break;

                    case SHDCodeOperation.Configuration:
                        {
                            telegram = shdMessage.BuildSendTelegram(this.ioSHDStatus.FwRelease);
                            await this.shdTransport.WriteAsync(telegram, this.stoppingToken);

                            this.logger.LogTrace($"5:message={shdMessage}");
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
            } while (!this.stoppingToken.IsCancellationRequested);

            this.logger.LogDebug("5:Method End");
        }

        private void SendIoMessageData(object state)
        {
            //var message = new IoSHDMessage(this.ioSHDStatus.InputData, this.ioSHDStatus.OutputData);
            var message = new IoSHDWriteMessage(this.ioSHDStatus.InputData, this.ioSHDStatus.OutputData);

            this.logger.LogDebug($"** enqueue message # {this.nMessages}");
            this.nMessages++;

            this.ioCommandQueue.Enqueue(message);
        }

        private async Task StartHardwareCommunications()
        {
            this.logger.LogDebug("1:Method Start");

            var ioAddress = await
                this.dataLayerConfigurationValueManagement.GetIPAddressConfigurationValueAsync((long)SetupNetwork.IOExpansion1, (long)ConfigurationCategory.SetupNetwork);
            var ioPort = await
                this.dataLayerConfigurationValueManagement.GetIntegerConfigurationValueAsync((long)SetupNetwork.IOExpansion1Port, (long)ConfigurationCategory.SetupNetwork);

            this.logger.LogTrace($"2:ioAddress={ioAddress}:ioPort={ioPort}");

            this.shdTransport.Configure(ioAddress, ioPort);

            try
            {
                await this.shdTransport.ConnectAsync();
            }
            catch (Exception ex)
            {
                this.logger.LogCritical($"3:Exception: {ex.Message} while connecting to Modbus I/O master - ExceptionCode: {IoDriverExceptionCode.CreationFailure}");

                throw new IoDriverException($"Exception: {ex.Message} while connecting to Modbus I/O master", IoDriverExceptionCode.CreationFailure, ex);
            }

            if (!this.shdTransport.IsConnected)
            {
                this.logger.LogCritical("4:Failed to connect to Modbus I/O master");

                throw new IoDriverException("Failed to connect to Modbus I/O master");
            }

            this.ioSHDStatus.IpAddress = ioAddress.ToString();

            try
            {
                this.ioReceiveTask.Start();
                this.ioSendTask.Start();
            }
            catch (Exception ex)
            {
                this.logger.LogCritical($"5:Exception: {ex.Message} while starting service hardware threads - ExceptionCode: {IoDriverExceptionCode.CreationFailure}");

                throw new IOException($"Exception: {ex.Message} while starting service hardware threads", ex);
            }

            this.logger.LogDebug("6:Method End");
        }

        private void StartPollingIoMessage()
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

        #endregion
    }
}
