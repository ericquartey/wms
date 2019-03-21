using System;
using System.Threading;
using System.Threading.Tasks;
using Ferretto.VW.Common_Utils.Enumerations;
using Ferretto.VW.Common_Utils.Events;
using Ferretto.VW.Common_Utils.Exceptions;
using Ferretto.VW.Common_Utils.Messages;
using Ferretto.VW.Common_Utils.Messages.Interfaces;
using Ferretto.VW.Common_Utils.Utilities;
using Ferretto.VW.InverterDriver.StateMachines.CalibrateAxis;
using Ferretto.VW.InverterDriver.StateMachines.Stop;
using Ferretto.VW.MAS_DataLayer;
using Ferretto.VW.MAS_InverterDriver;
using Ferretto.VW.MAS_InverterDriver.Interface;
using Ferretto.VW.MAS_InverterDriver.Interface.StateMachines;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Prism.Events;

namespace Ferretto.VW.InverterDriver
{
    public class HostedInverterDriver : BackgroundService
    {
        #region Fields

        private const int HeartbeatTimeout = 300;

        private readonly Task commandReceiveTask;

        private readonly IDataLayerValueManagment dataLayerValueManagment;

        private readonly IEventAggregator eventAggregator;

        private readonly BlockingConcurrentQueue<InverterMessage> heartbeatQueue;

        private readonly BlockingConcurrentQueue<InverterMessage> inverterCommandQueue;

        private readonly Task inverterReceiveTask;

        private readonly Task inverterSendTask;

        private readonly ILogger logger;

        private readonly BlockingConcurrentQueue<CommandMessage> messageQueue;

        private readonly BlockingConcurrentQueue<NotificationMessage> notificationQueue;

        private readonly Task notificationReceiveTask;

        private readonly ISocketTransport socketTransport;

        private Timer controlWordCheckTimer;

        private IInverterStateMachine currentStateMachine;

        private bool disposed;

        private bool heartbeatCheck;

        private bool heartbeatSet;

        private Timer heartBeatTimer;

        private InverterMessage lastControlMessage;

        private InverterMessage lastHeatbeatMessage;

        private CancellationToken stoppingToken;

        #endregion

        #region Constructors

        public HostedInverterDriver(IEventAggregator eventAggregator, ISocketTransport socketTransport, IDataLayerValueManagment dataLayerValueManagment, ILogger<HostedInverterDriver> logger)
        {
            this.socketTransport = socketTransport;
            this.eventAggregator = eventAggregator;
            this.dataLayerValueManagment = dataLayerValueManagment;
            this.logger = logger;

            this.heartbeatQueue = new BlockingConcurrentQueue<InverterMessage>();
            this.inverterCommandQueue = new BlockingConcurrentQueue<InverterMessage>();

            this.messageQueue = new BlockingConcurrentQueue<CommandMessage>();
            this.notificationQueue = new BlockingConcurrentQueue<NotificationMessage>();

            this.commandReceiveTask = new Task(() => this.CommandReceiveTaskFunction());
            this.notificationReceiveTask = new Task(() => this.NotificationReceiveTaskFunction());
            this.inverterReceiveTask = new Task(async () => await this.ReceiveInverterData());
            this.inverterSendTask = new Task(async () => await this.SendInverterCommand());

            this.lastControlMessage = new InverterMessage(0x00, (short)InverterParameterId.ControlWordParam, (ushort)0x0000);

            var commandEvent = this.eventAggregator.GetEvent<CommandEvent>();
            commandEvent.Subscribe(message =>
                {
                    this.messageQueue.Enqueue(message);
                },
                ThreadOption.PublisherThread,
                false,
                message => message.Destination == MessageActor.InverterDriver || message.Destination == MessageActor.Any);

            var notificationEvent = this.eventAggregator.GetEvent<NotificationEvent>();
            notificationEvent.Subscribe(message =>
                {
                    this.notificationQueue.Enqueue(message);
                },
                ThreadOption.PublisherThread,
                false,
                message => message.Destination == MessageActor.InverterDriver || message.Destination == MessageActor.Any);

            this.logger?.LogInformation("Hosted Inverter Driver Constructor");
        }

        #endregion

        #region Destructors

        ~HostedInverterDriver()
        {
            this.Dispose(false);
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
                this.heartBeatTimer?.Dispose();
                this.controlWordCheckTimer?.Dispose();
            }
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            this.stoppingToken = stoppingToken;

            var inverterAddress = this.dataLayerValueManagment.GetIPAddressConfigurationValue(ConfigurationValueEnum.InverterAddress);
            var inverterPort = this.dataLayerValueManagment.GetIntegerConfigurationValue(ConfigurationValueEnum.InverterPort);

            this.socketTransport.Configure(inverterAddress, inverterPort);

            try
            {
                await this.socketTransport.ConnectAsync();
            }
            catch (Exception ex)
            {
                throw new InverterDriverException($"Exception {ex.Message} while Connecting Receiver Socket Transport",
                    ex);
            }

            if (!this.socketTransport.IsConnected)
            {
                throw new InverterDriverException("Socket Transport failed to connect");
            }

            try
            {
                this.commandReceiveTask.Start();
                this.notificationReceiveTask.Start();
                this.inverterReceiveTask.Start();
                this.inverterSendTask.Start();
            }
            catch (Exception ex)
            {
                throw new InverterDriverException($"Exception: {ex.Message} while starting service threads", ex);
            }
        }

        private Task CommandReceiveTaskFunction()
        {
            this.controlWordCheckTimer?.Dispose();
            this.controlWordCheckTimer = new Timer(this.ControlWordCheckTimeout, null, -1, Timeout.Infinite);

            this.heartBeatTimer?.Dispose();
            this.heartBeatTimer = new Timer(this.SendHeartBeat, null, TimeSpan.Zero, TimeSpan.FromMilliseconds(HeartbeatTimeout));

            do
            {
                CommandMessage receivedMessage;
                try
                {
                    this.messageQueue.TryDequeue(Timeout.Infinite, this.stoppingToken, out receivedMessage);
                }
                catch (OperationCanceledException)
                {
                    return Task.CompletedTask;
                }

                if (this.currentStateMachine != null)
                {
                    var errorNotification = new NotificationMessage(null, "Inverter operation already in progress", MessageActor.Any,
                        MessageActor.InverterDriver, receivedMessage.Type, MessageStatus.OperationError, ErrorLevel.Error);
                    this.eventAggregator?.GetEvent<NotificationEvent>().Publish(errorNotification);
                    continue;
                }

                this.logger?.LogTrace($"{DateTime.Now}: Thread:{Thread.CurrentThread.ManagedThreadId} - HostedInverterDriver:CommandReceiveTaskFunction");

                switch (receivedMessage.Type)
                {
                    case MessageType.CalibrateAxis:
                        if (receivedMessage.Data is ICalibrateAxisMessageData calibrateData)
                        {
                            this.currentStateMachine = new CalibrateAxisStateMachine(calibrateData.AxisToCalibrate, this.inverterCommandQueue, this.eventAggregator, this.logger);
                        }

                        break;

                    case MessageType.Stop:
                        if (receivedMessage.Data is IStopAxisMessageData stopData)
                        {
                            this.currentStateMachine = new StopStateMachine(stopData.AxisToStop, this.inverterCommandQueue, this.eventAggregator, this.logger);
                        }
                        break;
                }
                this.currentStateMachine?.Start();
            } while (!this.stoppingToken.IsCancellationRequested);

            return Task.CompletedTask;
        }

        private void ControlWordCheckTimeout(object state)
        {
            this.controlWordCheckTimer.Change(-1, Timeout.Infinite);
            //TODO notify control word change error
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

                this.logger?.LogTrace($"{DateTime.Now}: Thread:{Thread.CurrentThread.ManagedThreadId} - HostedInverterDriver:NotificationReceiveTaskFunction");

                switch (receivedMessage.Type)
                {
                    case MessageType.CalibrateAxis:
                        if (receivedMessage.Status == MessageStatus.OperationEnd)
                        {
                            this.currentStateMachine.Dispose();
                            this.currentStateMachine = null;
                        }
                        break;

                    case MessageType.Stop:
                        if (receivedMessage.Status == MessageStatus.OperationEnd)
                        {
                            this.currentStateMachine.Dispose();
                            this.currentStateMachine = null;
                        }
                        break;
                }
            } while (!this.stoppingToken.IsCancellationRequested);
            return Task.CompletedTask;
        }

        private async Task ProcessCommand()
        {
            while (this.inverterCommandQueue.Dequeue(out var message))
            {
                //TEMP this.logger?.LogTrace($"{DateTime.Now}: Thread:{Thread.CurrentThread.ManagedThreadId} - HostedInverterDriver:ProcessCommand");

                if (message.ParameterId == InverterParameterId.ControlWordParam)
                {
                    this.lastControlMessage = new InverterMessage(message);
                }

                byte[] inverterMessage;
                if (message.IsWriteMessage)
                {
                    inverterMessage = message.GetWriteMessage();
                }
                else
                {
                    inverterMessage = message.GetReadMessage();
                }
                if (message.SendDelay > 0)
                {
                    await this.socketTransport.WriteAsync(inverterMessage, message.SendDelay, this.stoppingToken);
                }
                else
                {
                    await this.socketTransport.WriteAsync(inverterMessage, this.stoppingToken);
                }
            }
        }

        private async Task ProcessHeartbeat()
        {
            while (this.heartbeatQueue.Dequeue(out var message))
            {
                //TEMP this.logger?.LogTrace($"{DateTime.Now}: Thread:{Thread.CurrentThread.ManagedThreadId} - HostedInverterDriver:ProcessHeartbeat");
                await this.socketTransport.WriteAsync(message.GteHeartbeatMessage(this.heartbeatSet), this.stoppingToken);

                this.lastHeatbeatMessage = message;

                this.heartbeatSet = !this.heartbeatSet;
                this.heartbeatCheck = false;
            }
        }

        private async Task ReceiveInverterData()
        {
            do
            {
                byte[] inverterData;
                try
                {
                    inverterData = await this.socketTransport.ReadAsync(this.stoppingToken);
                    //TEMP this.logger?.LogTrace($"{DateTime.Now}: Thread:{Thread.CurrentThread.ManagedThreadId} - HostedInverterDriver:ReceiveInverterData/Read Async");
                }
                catch (OperationCanceledException)
                {
                    return;
                }

                //INFO: Byte 1 of read data contains packet length, zero means invalid packet
                if (inverterData[1] == 0x00)
                {
                    continue;
                }

                InverterMessage currentMessage;
                try
                {
                    currentMessage = new InverterMessage(inverterData);
                }
                catch (InverterDriverException)
                {
                    continue;
                }
                //TODO catch other exceptions

                if (currentMessage.IsWriteMessage && currentMessage.ParameterId == InverterParameterId.ControlWordParam)
                {
                    if (!this.heartbeatCheck)
                    {
                        if (currentMessage.UShortPayload == this.lastHeatbeatMessage.UShortPayload)
                        {
                            this.logger?.LogTrace($"{DateTime.Now}: Thread:{Thread.CurrentThread.ManagedThreadId} - HostedInverterDriver:ReceiveInverterData/Heartbeat Check");

                            this.heartbeatCheck = true;
                            continue;
                        }
                    }
                    else
                    {
                        this.logger?.LogTrace($"{DateTime.Now}: Thread:{Thread.CurrentThread.ManagedThreadId} - HostedInverterDriver:ReceiveInverterData/Request Status");

                        var readStatusWordMessage = new InverterMessage(0x00, (short)InverterParameterId.StatusWordParam);
                        this.inverterCommandQueue.Enqueue(readStatusWordMessage);
                        this.controlWordCheckTimer.Change(5000, Timeout.Infinite);
                        continue;
                    }
                }

                if (this.currentStateMachine != null)
                {
                    if (this.currentStateMachine.ProcessMessage(currentMessage))
                    {
                        try
                        {
                            this.controlWordCheckTimer.Change(-1, Timeout.Infinite);
                        }
                        catch (Exception)
                        {
                            this.controlWordCheckTimer = new Timer(this.ControlWordCheckTimeout, null, -1, Timeout.Infinite);
                        }
                    }
                    else
                    {
                        var readStatusWordMessage =
                            new InverterMessage(0x00, (short)InverterParameterId.StatusWordParam);
                        this.inverterCommandQueue.Enqueue(readStatusWordMessage);
                    }
                }
            } while (!this.stoppingToken.IsCancellationRequested);

            return;
        }

        private void SendHeartBeat(object state)
        {
            this.heartbeatQueue.Enqueue(this.lastControlMessage);
        }

        private async Task SendInverterCommand()
        {
            this.logger?.LogTrace($"{DateTime.Now}: Thread:{Thread.CurrentThread.ManagedThreadId} - HostedInverterDriver:SendInverterCommand/Start");

            //INFO Create WaitHandle array to wait for multiple events
            var commandHandles = new[]
            {
                this.heartbeatQueue.WaitHandle,
                this.inverterCommandQueue.WaitHandle
            };

            do
            {
                var handleIndex = WaitHandle.WaitAny(commandHandles);
                switch (handleIndex)
                {
                    case 0:
                        await this.ProcessHeartbeat();
                        break;

                    case 1:
                        await this.ProcessCommand();
                        break;
                }
            } while (!this.stoppingToken.IsCancellationRequested);
        }

        #endregion
    }
}
