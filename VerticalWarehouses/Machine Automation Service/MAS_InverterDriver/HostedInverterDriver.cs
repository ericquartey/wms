using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Ferretto.VW.Common_Utils.Enumerations;
using Ferretto.VW.Common_Utils.Events;
using Ferretto.VW.Common_Utils.Exceptions;
using Ferretto.VW.Common_Utils.Messages;
using Ferretto.VW.Common_Utils.Messages.Interfaces;
using Ferretto.VW.Common_Utils.Utilities;
using Ferretto.VW.InverterDriver.Interface;
using Ferretto.VW.InverterDriver.StateMachines;
using Ferretto.VW.MAS_DataLayer;
using Microsoft.Extensions.Hosting;
using Prism.Events;

namespace Ferretto.VW.InverterDriver
{
    public class HostedInverterDriver : BackgroundService
    {
        #region Fields

        private const Int32 HEARTBEAT_TIMEOUT = 300;

        private const Int32 InverterPortNumber = 17221;

        private readonly IDataLayer dataLayer;

        private readonly IEventAggregator eventAggregator;

        private readonly BlockingConcurrentQueue<InverterMessage> heartbeatQueue;

        private readonly BlockingConcurrentQueue<InverterMessage> inverterCommandQueue;

        private readonly BlockingConcurrentQueue<CommandMessage> messageQueue;

        private readonly ISocketTransport socketTransport;

        private Timer controlWordCheckTimer;

        private IInverterStateMachine currentStateMachine;

        private Timer heartBeatTimer;

        private Task inverterReceiveTask;

        private Task inverterSendTask;

        private InverterMessage lastControlMessage;

        #endregion

        #region Constructors

        public HostedInverterDriver(IEventAggregator eventAggregator, ISocketTransport socketTransport,
            IDataLayer dataLayer)
        {
            this.socketTransport = socketTransport;
            this.eventAggregator = eventAggregator;
            this.dataLayer = dataLayer;

            this.heartbeatQueue = new BlockingConcurrentQueue<InverterMessage>();
            this.inverterCommandQueue = new BlockingConcurrentQueue<InverterMessage>();

            this.messageQueue = new BlockingConcurrentQueue<CommandMessage>();

            var webApiMessagEvent = this.eventAggregator.GetEvent<CommandEvent>();
            webApiMessagEvent.Subscribe(message => { this.messageQueue.Enqueue(message); },
                ThreadOption.PublisherThread,
                false,
                message => message.Source == MessageActor.FiniteStateMachines);
        }

        #endregion

        #region Methods

        public new void Dispose()
        {
            base.Dispose();

            this.heartBeatTimer?.Dispose();
            this.controlWordCheckTimer?.Dispose();
        }

        public override Task StopAsync(CancellationToken stoppingToken)
        {
            var returnValue = base.StopAsync(stoppingToken);

            return returnValue;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Task.Run(() => this.HostedInverterDriverTaskFunction(stoppingToken), stoppingToken);
        }

        private void ControlWordCheckTimeout(Object state)
        {
            this.controlWordCheckTimer.Change(-1, Timeout.Infinite);
            //TODO notify control word change error
        }

        private Task HostedInverterDriverTaskFunction(CancellationToken stoppingToken)
        {
            //=== Create control word check timer but not start it
            this.controlWordCheckTimer?.Dispose();
            this.controlWordCheckTimer = new Timer(this.ControlWordCheckTimeout, null, -1, Timeout.Infinite);

            //=== create the heartbeat timer
            this.heartBeatTimer?.Dispose();
            this.heartBeatTimer = new Timer(this.SendHeartBeat, null, TimeSpan.Zero,
                TimeSpan.FromMilliseconds(HEARTBEAT_TIMEOUT));

            //=== create and start the sending Task
            this.inverterSendTask?.Dispose();
            this.inverterSendTask = Task.Run(() => this.SendInverterCommand(stoppingToken), stoppingToken);

            //=== create and start the receiving Task
            this.inverterReceiveTask?.Dispose();
            this.inverterReceiveTask = Task.Run(() => this.ReceiveInverterData(stoppingToken), stoppingToken);

            //=== This will be the command receiving Task from state machine
            do
            {
                CommandMessage receivedMessage;
                try
                {
                    this.messageQueue.TryDequeue(Timeout.Infinite, stoppingToken, out receivedMessage);
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
                            this.currentStateMachine = new CalibrateStateMachine(data.AxisToCalibrate,
                                this.inverterCommandQueue, this.heartbeatQueue);
                            this.currentStateMachine.Start();
                        }

                        break;
                }
            } while (stoppingToken.IsCancellationRequested);

            return Task.CompletedTask;
        }

        private async Task ProcessCommand(CancellationToken cancellationToken)
        {
            while (this.inverterCommandQueue.TryDequeue(Timeout.Infinite, cancellationToken, out var message))
            {
                if (message.ParameterId == InverterParameterId.ControlWordParam)
                    this.lastControlMessage = new InverterMessage(message);

                await this.socketTransport.WriteAsync(message.GetWriteMessage(), cancellationToken);
            }
        }

        private async Task ProcessHeartbeat(CancellationToken cancellationToken)
        {
            while (this.heartbeatQueue.TryDequeue(Timeout.Infinite, cancellationToken, out var message))
                await this.socketTransport.WriteAsync(message.GetWriteMessage(), cancellationToken);
        }

        private async void ReceiveInverterData(CancellationToken stoppingToken)
        {
            var inverterAddress =
                IPAddress.Any; //this.dataLayer.GetIPAddressConfigurationValue( ConfigurationValueEnum.InverterAddress );
            var inverterPort = this.dataLayer.GetIntegerConfigurationValue(ConfigurationValueEnum.InverterPort);

            this.socketTransport.Configure(inverterAddress, inverterPort);

            Boolean connectionCompleted;
            try
            {
                connectionCompleted = await this.socketTransport.ConnectAsync();
            }
            catch (Exception ex)
            {
                throw new InverterDriverException($"Exception {ex.Message} while Connecting Receiver Socket Transport",
                    ex);
            }

            if (!connectionCompleted) throw new InverterDriverException("Socket Transport failed to connect");

            do
            {
                Byte[] inverterData;
                try
                {
                    inverterData = await this.socketTransport.ReadAsync(stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    return;
                }

                var currentMessage = new InverterMessage(inverterData);

                if (currentMessage.IsError) continue;

                if (currentMessage.IsWriteMessage && currentMessage.ParameterId == InverterParameterId.ControlWordParam)
                {
                    var readStatusWordMessage = new InverterMessage(0x00, (Int16) InverterParameterId.StatusWordParam);
                    this.inverterCommandQueue.Enqueue(readStatusWordMessage);
                    this.controlWordCheckTimer.Change(5000, Timeout.Infinite);
                    continue;
                }

                if (!currentMessage.IsWriteMessage && currentMessage.ParameterId == InverterParameterId.StatusWordParam)
                {
                    if (currentMessage.ShortPayload != this.lastControlMessage.ShortPayload)
                    {
                        var readStatusWordMessage =
                            new InverterMessage(0x00, (Int16) InverterParameterId.StatusWordParam);
                        this.inverterCommandQueue.Enqueue(readStatusWordMessage);
                        continue;
                    }

                    this.controlWordCheckTimer.Change(-1, Timeout.Infinite);
                }

                this.currentStateMachine.NotifyMessage(currentMessage);
            } while (stoppingToken.IsCancellationRequested);
        }

        private void SendHeartBeat(Object state)
        {
            this.heartbeatQueue.Enqueue(this.lastControlMessage);
        }

        private async Task SendInverterCommand(CancellationToken cancellationToken)
        {
            var cancellationEventSlim = new ManualResetEventSlim(false);

            cancellationToken.Register(() => cancellationEventSlim.Set());

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
                        await this.ProcessHeartbeat(cancellationToken);
                        break;

                    case 1:
                        await this.ProcessCommand(cancellationToken);
                        break;

                    case 2:
                        return;
                }
            } while (!cancellationToken.IsCancellationRequested);
        }

        #endregion
    }
}
