using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Ferretto.VW.Common_Utils.Enumerations;
using Ferretto.VW.Common_Utils.Events;
using Ferretto.VW.Common_Utils.Exceptions;
using Ferretto.VW.Common_Utils.Messages;
using Ferretto.VW.Common_Utils.Utilities;
using Ferretto.VW.MAS_DataLayer;
using Ferretto.VW.MAS_DataLayer.Enumerations;
using Ferretto.VW.MAS_IODriver.Interface;
using Ferretto.VW.MAS_IODriver.StateMachines;
using Ferretto.VW.MAS_IODriver.StateMachines.PowerUp;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Prism.Events;

namespace Ferretto.VW.MAS_IODriver
{
    public partial class HostedIoDriver : BackgroundService
    {
        #region Fields

        private const int IoPollingInterval = 50;

        private readonly BlockingConcurrentQueue<CommandMessage> commandQueue;

        private readonly Task commandReceiveTask;

        private readonly IDataLayerValueManagment dataLayerValueManagment;

        private readonly IEventAggregator eventAggregator;

        private readonly BlockingConcurrentQueue<IoMessage> ioCommandQueue;

        private readonly Task ioReceiveTask;

        private readonly Task ioSendTask;

        private readonly IoStatus ioStatus;

        private readonly ILogger logger;

        private readonly IModbusTransport modbusTransport;

        private readonly BlockingConcurrentQueue<NotificationMessage> notificationQueue;

        private readonly Task notificationReceiveTask;

        private readonly ManualResetEventSlim pollIoEvent;

        private IIoStateMachine currentStateMachine;

        private bool disposed;

        private bool[] inputData;

        private Timer pollIoTimer;

        private CancellationToken stoppingToken;

        #endregion

        #region Constructors

        public HostedIoDriver(IEventAggregator eventAggregator, IModbusTransport modbusTransport, IDataLayerValueManagment dataLayerValueManagment, ILogger<HostedIoDriver> logger)
        {
            this.eventAggregator = eventAggregator;
            this.dataLayerValueManagment = dataLayerValueManagment;
            this.modbusTransport = modbusTransport;

            this.logger = logger;

            this.ioStatus = new IoStatus();
            this.pollIoEvent = new ManualResetEventSlim(false);

            this.ioCommandQueue = new BlockingConcurrentQueue<IoMessage>();

            this.commandQueue = new BlockingConcurrentQueue<CommandMessage>();

            this.notificationQueue = new BlockingConcurrentQueue<NotificationMessage>();

            this.commandReceiveTask = new Task(() => this.CommandReceiveTaskFunction());
            this.notificationReceiveTask = new Task(() => this.NotificationReceiveTaskFunction());
            this.ioReceiveTask = new Task(async () => await this.ReceiveIoDataTaskFunction());
            this.ioSendTask = new Task(async () => await this.SendIoCommandTaskFunction());

            var commandEvent = this.eventAggregator.GetEvent<CommandEvent>();
            commandEvent.Subscribe(message => { this.commandQueue.Enqueue(message); },
                ThreadOption.PublisherThread,
                false,
                message => message.Destination == MessageActor.IODriver || message.Destination == MessageActor.Any);

            var notificationEvent = this.eventAggregator.GetEvent<NotificationEvent>();
            notificationEvent.Subscribe(message => { this.notificationQueue.Enqueue(message); },
                ThreadOption.PublisherThread,
                false,
                message => message.Destination == MessageActor.IODriver || message.Destination == MessageActor.Any);

            this.logger?.LogInformation("Hosted I/O Driver Constructor");
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
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            this.stoppingToken = stoppingToken;

            var ioAddress =
                this.dataLayerValueManagment.GetIPAddressConfigurationValue((long)SetupNetwork.IOExpansion1, (long)ConfigurationCategory.SetupNetwork);
            var ioPort =
                this.dataLayerValueManagment.GetIntegerConfigurationValue((long)SetupNetwork.IOExpansion1Port, (long)ConfigurationCategory.SetupNetwork);

            this.modbusTransport.Configure(ioAddress, ioPort);

            bool connectionResult;
            try
            {
                connectionResult = this.modbusTransport.Connect();
            }
            catch (Exception ex)
            {
                throw new IoDriverException($"Exception: {ex.Message} while connecting to Modbus I/O master", IoDriverExceptionCode.CreationFailure, ex);
            }

            if (!connectionResult)
            {
                throw new IoDriverException("Failed to connect to Modbus I/O master");
            }

            try
            {
                this.commandReceiveTask.Start();
                this.notificationReceiveTask.Start();
                this.ioReceiveTask.Start();
                this.ioSendTask.Start();
            }
            catch (Exception ex)
            {
                throw new IOException($"Exception: {ex.Message} while starting service threads", ex);
            }
        }

        private Task CommandReceiveTaskFunction()
        {
            this.pollIoTimer?.Dispose();
            try
            {
                this.pollIoTimer = new Timer(this.ReadIoData, null, TimeSpan.Zero, TimeSpan.FromMilliseconds(IoPollingInterval));
            }
            catch (Exception ex)
            {
                throw new IOException($"Exception: {ex.Message} Timer Creation Failed", ex);
            }

            this.currentStateMachine = new PowerUpStateMachine(this.ioCommandQueue, this.eventAggregator, this.logger);
            this.currentStateMachine.Start();

            do
            {
                CommandMessage receivedMessage;
                try
                {
                    this.commandQueue.TryDequeue(Timeout.Infinite, this.stoppingToken, out receivedMessage);
                }
                catch (OperationCanceledException)
                {
                    return Task.CompletedTask;
                }

                if (this.currentStateMachine != null)
                {
                    var errorNotification = new NotificationMessage(null, "I/O operation already in progress", MessageActor.Any,
                        MessageActor.IODriver, receivedMessage.Type, MessageStatus.OperationError, ErrorLevel.Error);
                    this.eventAggregator?.GetEvent<NotificationEvent>().Publish(errorNotification);
                    continue;
                }

                switch (receivedMessage.Type)
                {
                    case MessageType.SwitchAxis:
                        this.ExecuteSwitchAxis(receivedMessage);
                        break;
                }
            } while (!this.stoppingToken.IsCancellationRequested);

            return Task.CompletedTask;
        }

        private Task NotificationReceiveTaskFunction()
        {
            do
            {
                NotificationMessage receivedMessage;
                try
                {
                    this.notificationQueue.TryDequeue(Timeout.Infinite, this.stoppingToken, out receivedMessage);
                }
                catch (OperationCanceledException)
                {
                    return Task.CompletedTask;
                }

                switch (receivedMessage.Type)
                {
                    case MessageType.IOPowerUp:
                    case MessageType.SwitchAxis:
                        if (receivedMessage.Status == MessageStatus.OperationEnd &&
                            receivedMessage.ErrorLevel == ErrorLevel.NoError)
                        {
                            this.currentStateMachine.Dispose();
                            this.currentStateMachine = null;
                        }
                        break;
                }
            } while (!this.stoppingToken.IsCancellationRequested);
            return Task.CompletedTask;
        }

        private void ReadIoData(object state)
        {
            this.pollIoEvent.Set();
        }

        private async Task ReceiveIoDataTaskFunction()
        {
            do
            {
                try
                {
                    this.pollIoEvent.Wait(Timeout.Infinite, this.stoppingToken);
                    this.pollIoEvent.Reset();
                }
                catch (OperationCanceledException)
                {
                    return;
                }

                try
                {
                    this.inputData = await this.modbusTransport.ReadAsync();
                }
                catch (Exception ex)
                {
                    throw new IoDriverException($"Exception: {ex.Message} while reading async error", IoDriverExceptionCode.CreationFailure, ex);
                }

                if (this.inputData == null)
                {
                    continue;
                }

                if (this.ioStatus.UpdateInputStates(this.inputData))
                {
                    this.currentStateMachine?.ProcessMessage(new IoMessage(this.inputData, true));
                }
            } while (!this.stoppingToken.IsCancellationRequested);
        }

        private async Task SendIoCommandTaskFunction()
        {
            do
            {
                IoMessage message;
                try
                {
                    this.ioCommandQueue.TryDequeue(Timeout.Infinite, this.stoppingToken, out message);
                }
                catch (OperationCanceledException)
                {
                    return;
                }

                if (message.ValidOutputs)
                {
                    if (this.ioStatus.UpdateOutputStates(message.Outputs) || message.Force)
                    {
                        await this.modbusTransport.WriteAsync(message.Outputs);
                    }

                    this.currentStateMachine.ProcessMessage(message);
                }
            } while (!this.stoppingToken.IsCancellationRequested);
        }

        #endregion
    }
}
