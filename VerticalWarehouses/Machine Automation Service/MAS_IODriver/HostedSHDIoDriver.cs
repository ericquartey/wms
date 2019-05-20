using System;
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
// ReSharper disable ArrangeThisQualifier

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

        private readonly BlockingConcurrentQueue<IoSHDWriteMessage> ioCommandQueue;

        private readonly Task ioReceiveTask;

        private readonly Task ioSendTask;

        private readonly IoSHDStatus ioSHDStatus;

        private readonly ILogger logger;

        private readonly BlockingConcurrentQueue<FieldNotificationMessage> notificationQueue;

        private readonly Task notificationReceiveTask;

        private readonly ISHDTransport shdTransport;

        private IIoStateMachine currentStateMachine;

        private bool disposed;

        // used only in ReceiveIoDataTaskFunction
        private bool[] inputData;

        // used only in ReceiveIoDataTaskFunction
        private bool[] outputData;

        private Timer pollIoTimer;

        private CancellationToken stoppingToken;

        private SHDTransport_Utility utility;

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

            this.utility = new SHDTransport_Utility();

            this.outputData = new bool[8];
            this.inputData = new bool[16];

            this.ioSHDStatus = new IoSHDStatus();

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

        private void CommandReceiveTaskFunction()
        {
            this.logger.LogDebug("1:Method Start");

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

                    this.utility.ParsingDataBytes(
                        telegram,
                        out nBytesReceived,
                        out formatDataOperation,
                        out fwRelease,
                        ref this.inputData,
                        ref this.outputData,
                        out configurationData,
                        out errorCode);
                }
                catch (Exception ex)
                {
                    this.logger.LogCritical($"3:Exception: {ex.Message} while reading async error - ExceptionCode: {IoDriverExceptionCode.CreationFailure}");

                    throw new IoDriverException($"Exception: {ex.Message} while reading async error", IoDriverExceptionCode.CreationFailure, ex);
                }

                lock (this.ioSHDStatus)
                {
                    this.ioSHDStatus.FwRelease = fwRelease;
                }

                switch (formatDataOperation)
                {
                    case SHDFormatDataOperation.Data:

                        // TODO Check the fault output lines status

                        // update IO status
                        this.ioSHDStatus.UpdateInputStates(this.inputData);

                        var messageData = new IoSHDReadMessage(
                            formatDataOperation,
                            fwRelease,
                            this.inputData,
                            this.outputData,
                            configurationData,
                            errorCode);
                        this.logger.LogTrace($"4:{messageData}");

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
                        this.logger.LogTrace($"4: Configuration message={messageConfig}");

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
            var message = new IoSHDWriteMessage(this.ioSHDStatus.OutputData);

            this.logger.LogDebug($"Enqueue message={message.ToString()}");

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

            // PowerUp
            this.currentStateMachine = new PowerUpStateMachine(this.ioCommandQueue, this.ioSHDStatus, this.eventAggregator, this.logger);
            this.currentStateMachine.Start();
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
