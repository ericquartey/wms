using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Ferretto.VW.Common_Utils.Enumerations;
using Ferretto.VW.MAS_DataLayer.Enumerations;
using Ferretto.VW.MAS_DataLayer.Interfaces;
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
// ReSharper disable ParameterHidesMember

namespace Ferretto.VW.MAS_IODriver
{
    public partial class HostedIoDriver : BackgroundService
    {
        #region Fields

        private const int IO_POLLING_INTERVAL = 50;

        private readonly BlockingConcurrentQueue<FieldCommandMessage> commandQueue;

        private readonly Task commandReceiveTask;

        private readonly IDataLayerValueManagment dataLayerValueManagement;

        private readonly IEventAggregator eventAggregator;

        private readonly BlockingConcurrentQueue<IoMessage> ioCommandQueue;

        private readonly Task ioReceiveTask;

        private readonly Task ioSendTask;

        private readonly IoStatus ioStatus;

        private readonly ILogger logger;

        private readonly IModbusTransport modbusTransport;

        private readonly BlockingConcurrentQueue<FieldNotificationMessage> notificationQueue;

        private readonly Task notificationReceiveTask;

        private readonly ManualResetEventSlim pollIoEvent;

        private IIoStateMachine currentStateMachine;

        private bool disposed;

        private bool[] inputData;

        private Timer pollIoTimer;

        private CancellationToken stoppingToken;

        #endregion

        #region Constructors

        public HostedIoDriver(IEventAggregator eventAggregator, IModbusTransport modbusTransport, IDataLayerValueManagment dataLayerValueManagement, ILogger<HostedIoDriver> logger)
        {
            logger.LogDebug("1:Method Start");

            this.logger = logger;
            this.eventAggregator = eventAggregator;
            this.dataLayerValueManagement = dataLayerValueManagement;
            this.modbusTransport = modbusTransport;

            this.ioStatus = new IoStatus();
            this.pollIoEvent = new ManualResetEventSlim(false);

            this.ioCommandQueue = new BlockingConcurrentQueue<IoMessage>();

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

        ~HostedIoDriver()
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

            this.pollIoTimer?.Dispose();
            try
            {
                this.pollIoTimer = new Timer(this.ReadIoData, null, TimeSpan.Zero, TimeSpan.FromMilliseconds(IO_POLLING_INTERVAL));
            }
            catch (Exception ex)
            {
                this.logger.LogCritical($"2:Exception: {ex.Message} Timer Creation Failed");

                throw new IOException($"Exception: {ex.Message} Timer Creation Failed", ex);
            }
            this.currentStateMachine = new PowerUpStateMachine(this.ioCommandQueue, this.eventAggregator, this.logger);
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

        private void ReadIoData(object state)
        {
            this.pollIoEvent.Set();
        }

        private async Task ReceiveIoDataTaskFunction()
        {
            this.logger.LogDebug("1:Method Start");

            do
            {
                try
                {
                    this.pollIoEvent.Wait(Timeout.Infinite, this.stoppingToken);
                    this.pollIoEvent.Reset();
                }
                catch (OperationCanceledException)
                {
                    this.logger.LogDebug("2:Method End operation cancelled");

                    return;
                }

                try
                {
                    this.inputData = await this.modbusTransport.ReadAsync();
                }
                catch (Exception ex)
                {
                    this.logger.LogCritical($"3:Exception: {ex.Message} while reading async error - ExceptionCode: {IoDriverExceptionCode.CreationFailure}");

                    throw new IoDriverException($"Exception: {ex.Message} while reading async error", IoDriverExceptionCode.CreationFailure, ex);
                }

                if (this.inputData == null)
                {
                    continue;
                }

                if (this.ioStatus.UpdateInputStates(this.inputData))
                {
                    var message = new IoMessage(this.inputData, true);

                    this.logger.LogTrace($"4:{message}");

                    this.currentStateMachine?.ProcessMessage(message);
                }
            } while (!this.stoppingToken.IsCancellationRequested);

            this.logger.LogDebug("5:Method End");
        }

        private async Task SendIoCommandTaskFunction()
        {
            this.logger.LogDebug("1:Method Start");

            do
            {
                IoMessage message;
                try
                {
                    this.ioCommandQueue.TryDequeue(Timeout.Infinite, this.stoppingToken, out message);

                    this.logger.LogTrace($"2:message={message}");
                }
                catch (OperationCanceledException)
                {
                    this.logger.LogDebug("3:Method End operation cancelled");

                    return;
                }

                if (message.ValidOutputs)
                {
                    if (this.ioStatus.UpdateOutputStates(message.Outputs) || message.Force)
                    {
                        await this.modbusTransport.WriteAsync(message.Outputs);
                    }

                    this.logger.LogTrace($"4:message={message}");

                    this.currentStateMachine.ProcessMessage(message);
                }
            } while (!this.stoppingToken.IsCancellationRequested);

            this.logger.LogDebug("5:Method End");
        }

        private async Task StartHardwareCommunications()
        {
            this.logger.LogDebug("1:Method Start");

            var ioAddress = await
                this.dataLayerValueManagement.GetIPAddressConfigurationValueAsync((long)SetupNetwork.IOExpansion1, (long)ConfigurationCategory.SetupNetwork);
            var ioPort = await
                this.dataLayerValueManagement.GetIntegerConfigurationValueAsync((long)SetupNetwork.IOExpansion1Port, (long)ConfigurationCategory.SetupNetwork);

            this.logger.LogTrace($"2:ioAddress={ioAddress}:ioPort={ioPort}");

            this.modbusTransport.Configure(ioAddress, ioPort);

            bool connectionResult;
            try
            {
                connectionResult = this.modbusTransport.Connect();
            }
            catch (Exception ex)
            {
                this.logger.LogCritical($"3:Exception: {ex.Message} while connecting to Modbus I/O master - ExceptionCode: {IoDriverExceptionCode.CreationFailure}");

                throw new IoDriverException($"Exception: {ex.Message} while connecting to Modbus I/O master", IoDriverExceptionCode.CreationFailure, ex);
            }

            if (!connectionResult)
            {
                this.logger.LogCritical("4:Failed to connect to Modbus I/O master");

                throw new IoDriverException("Failed to connect to Modbus I/O master");
            }

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

        #endregion
    }
}
