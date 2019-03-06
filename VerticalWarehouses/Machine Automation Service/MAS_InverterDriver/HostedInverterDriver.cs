using System;
using System.Threading;
using System.Threading.Tasks;
using Ferretto.VW.Common_Utils.Enumerations;
using Ferretto.VW.Common_Utils.Events;
using Ferretto.VW.Common_Utils.Exceptions;
using Ferretto.VW.Common_Utils.Messages;
using Ferretto.VW.Common_Utils.Messages.Interfaces;
using Ferretto.VW.Common_Utils.Utilities;
using Ferretto.VW.MAS_InverterDriver.Interface;
using Ferretto.VW.MAS_InverterDriver;
using Ferretto.VW.MAS_InverterDriver.StateMachines;
using Ferretto.VW.MAS_DataLayer;
using Microsoft.Extensions.Hosting;
using Prism.Events;

namespace Ferretto.VW.InverterDriver
{
    public class HostedInverterDriver : BackgroundService
    {
        #region Fields

        private const int HeartbeatTimeout = 300;

        private const int InverterPortNumber = 17221;

        private readonly Task commandReceiveTask;

        private readonly IDataLayer dataLayer;

        private readonly IEventAggregator eventAggregator;

        private readonly BlockingConcurrentQueue<InverterMessage> heartbeatQueue;

        private readonly BlockingConcurrentQueue<InverterMessage> inverterCommandQueue;

        private readonly Task inverterReceiveTask;

        private readonly Task inverterSendTask;

        private readonly BlockingConcurrentQueue<CommandMessage> messageQueue;

        private readonly BlockingConcurrentQueue<NotificationMessage> notificationQueue;

        private readonly Task notificationReceiveTask;

        private readonly ISocketTransport socketTransport;

        private Timer controlWordCheckTimer;

        private IInverterStateMachine currentStateMachine;

        private bool disposed;

        private Timer heartBeatTimer;

        private InverterMessage lastControlMessage;

        private CancellationToken stoppingToken;

        #endregion

        #region Constructors

        public HostedInverterDriver(IEventAggregator eventAggregator, ISocketTransport socketTransport, IDataLayer dataLayer)
        {
            this.socketTransport = socketTransport;
            this.eventAggregator = eventAggregator;
            this.dataLayer = dataLayer;

            this.heartbeatQueue = new BlockingConcurrentQueue<InverterMessage>();
            this.inverterCommandQueue = new BlockingConcurrentQueue<InverterMessage>();

            this.messageQueue = new BlockingConcurrentQueue<CommandMessage>();
            this.notificationQueue = new BlockingConcurrentQueue<NotificationMessage>();

            this.commandReceiveTask = new Task(() => this.CommandReceiveTaskFunction());
            this.notificationReceiveTask = new Task(() => this.NotificationReceiveTaskFunction());
            this.inverterReceiveTask = new Task(async () => await ReceiveInverterData());
            this.inverterSendTask = new Task(async () => await SendInverterCommand());

            var commandEvent = this.eventAggregator.GetEvent<CommandEvent>();
            commandEvent.Subscribe(message => { this.messageQueue.Enqueue(message); },
                ThreadOption.PublisherThread,
                false,
                message => message.Source == MessageActor.FiniteStateMachines);

            var notificationEvent = this.eventAggregator.GetEvent<NotificationEvent>();
            notificationEvent.Subscribe(message => { this.notificationQueue.Enqueue(message); },
                ThreadOption.PublisherThread,
                false,
                message => message.Destination == MessageActor.InverterDriver || message.Destination == MessageActor.Any);
        }

        #endregion

        #region Destructors

        ~HostedInverterDriver()
        {
            Dispose(false);
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

            var inverterAddress = this.dataLayer.GetIPAddressConfigurationValue(ConfigurationValueEnum.InverterAddress);
            var inverterPort = this.dataLayer.GetIntegerConfigurationValue(ConfigurationValueEnum.InverterPort);

            this.socketTransport.Configure(inverterAddress, inverterPort);

            bool connectionCompleted;
            try
            {
                connectionCompleted = await this.socketTransport.ConnectAsync();
            }
            catch (Exception ex)
            {
                throw new InverterDriverException($"Exception {ex.Message} while Connecting Receiver Socket Transport",
                    ex);
            }

            if (!connectionCompleted)
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

                switch (receivedMessage.Type)
                {
                    case MessageType.Calibrate:
                        if (receivedMessage.Data is ICalibrateMessageData data)
                        {
                            this.currentStateMachine = new CalibrateStateMachine(data.AxisToCalibrate, this.inverterCommandQueue);
                            this.currentStateMachine.Start();
                        }

                        break;
                }
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

                switch (receivedMessage.Type)
                {
                    case MessageType.SwitchAxis:
                        if (receivedMessage.Status == MessageStatus.OperationEnd)
                        {
                            if (this.currentStateMachine is CalibrateStateMachine)
                            {
                                this.currentStateMachine.ChangeState();
                            }
                        }
                        break;
                }
            } while (!this.stoppingToken.IsCancellationRequested);
            return Task.CompletedTask;
        }

        private async Task ProcessCommand()
        {
            while (this.inverterCommandQueue.TryDequeue(Timeout.Infinite, this.stoppingToken, out var message))
            {
                if (message.ParameterId == InverterParameterId.ControlWordParam)
                    this.lastControlMessage = new InverterMessage(message);

                await this.socketTransport.WriteAsync(message.GetWriteMessage(), this.stoppingToken);
            }
        }

        private async Task ProcessHeartbeat()
        {
            while (this.heartbeatQueue.TryDequeue(Timeout.Infinite, this.stoppingToken, out var message))
                await this.socketTransport.WriteAsync(message.GetWriteMessage(), this.stoppingToken);
        }

        private async Task ReceiveInverterData()
        {
            do
            {
                byte[] inverterData;
                try
                {
                    inverterData = await this.socketTransport.ReadAsync(this.stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    return;
                }

                var currentMessage = new InverterMessage(inverterData);

                if (currentMessage.IsWriteMessage && currentMessage.ParameterId == InverterParameterId.ControlWordParam)
                {
                    var readStatusWordMessage = new InverterMessage(0x00, (short)InverterParameterId.StatusWordParam);
                    this.inverterCommandQueue.Enqueue(readStatusWordMessage);
                    this.controlWordCheckTimer.Change(5000, Timeout.Infinite);
                    continue;
                }

                if (!currentMessage.IsWriteMessage && currentMessage.ParameterId == InverterParameterId.StatusWordParam)
                {
                    if (currentMessage.ShortPayload != this.lastControlMessage.ShortPayload)
                    {
                        var readStatusWordMessage =
                            new InverterMessage(0x00, (short)InverterParameterId.StatusWordParam);
                        this.inverterCommandQueue.Enqueue(readStatusWordMessage);
                        continue;
                    }

                    this.controlWordCheckTimer.Change(-1, Timeout.Infinite);
                }

                this.currentStateMachine.ProcessMessage(currentMessage);
            } while (!this.stoppingToken.IsCancellationRequested);
        }

        private void SendHeartBeat(object state)
        {
            this.heartbeatQueue.Enqueue(this.lastControlMessage);
        }

        private async Task SendInverterCommand()
        {
            var cancellationEventSlim = new ManualResetEventSlim(false);

            this.stoppingToken.Register(() => cancellationEventSlim.Set());

            //INFO Create WaitHandle array to wait for multiple events
            var commandHandles = new[]
            {
                this.heartbeatQueue.WaitHandle,
                this.inverterCommandQueue.WaitHandle,
                cancellationEventSlim.WaitHandle
            };

            do
            {
                var handleIndex = WaitHandle.WaitAny(commandHandles, Timeout.Infinite);
                switch (handleIndex)
                {
                    case 0:
                        await this.ProcessHeartbeat();
                        break;

                    case 1:
                        await this.ProcessCommand();
                        break;

                    case 2:
                        return;
                }
            } while (!this.stoppingToken.IsCancellationRequested);
        }

        #endregion
    }
}
