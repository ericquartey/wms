using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Ferretto.VW.Common_Utils.Events;
using Ferretto.VW.Common_Utils.Exceptions;
using Ferretto.VW.Common_Utils.Messages;
using Ferretto.VW.InverterDriver.Interface;
using Microsoft.Extensions.Hosting;
using Prism.Events;

namespace Ferretto.VW.InverterDriver
{
    public class HostedInverterDriver : BackgroundService
    {
        #region Fields

        private const int HEARTBEAT_TIMEOUT = 300;

        private const int InverterPortNumber = 17221;

        private readonly ConcurrentQueue<Event_Message> commandQueue;

        private readonly IEventAggregator eventAggregator;

        private readonly ManualResetEventSlim inverterCommandReceived;

        private readonly ConcurrentQueue<Event_Message> messageQueue;

        private readonly ManualResetEventSlim messageReceived;

        private readonly ManualResetEventSlim priorityInverterCommandReceived;

        private readonly ConcurrentQueue<Event_Message> priorityQueue;

        private readonly ISocketTransport socketTransport;

        private Timer heartBeatTimer;

        private Task inverterReceiveTask;

        private Task inverterSendTask;

        private Socket receiveSocket;

        #endregion

        #region Constructors

        public HostedInverterDriver(IEventAggregator eventAggregator, ISocketTransport socketTransport)
        {
            this.socketTransport = socketTransport;
            this.eventAggregator = eventAggregator;

            this.priorityInverterCommandReceived = new ManualResetEventSlim(false);
            this.inverterCommandReceived = new ManualResetEventSlim(false);
            this.messageReceived = new ManualResetEventSlim(false);

            this.priorityQueue = new ConcurrentQueue<Event_Message>();
            this.commandQueue = new ConcurrentQueue<Event_Message>();
            this.messageQueue = new ConcurrentQueue<Event_Message>();

            var webApiMessagEvent = this.eventAggregator.GetEvent<MachineAutomationService_Event>();
            webApiMessagEvent.Subscribe((message) =>
               {
                   this.messageQueue.Enqueue(message);
                   this.messageReceived.Set();
               },
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

        private Task HostedInverterDriverTaskFunction(CancellationToken stoppingToken)
        {
            //=== create the heartbeat timer
            this.heartBeatTimer?.Dispose();
            this.heartBeatTimer = new Timer(SendHeartBeat, null, TimeSpan.Zero, TimeSpan.FromMilliseconds(HEARTBEAT_TIMEOUT));

            //=== create and start the sending Task
            this.inverterSendTask?.Dispose();
            this.inverterSendTask = Task.Run(() => SendInverterCommand(stoppingToken), stoppingToken);

            //=== create and start the receiving Task
            this.inverterReceiveTask?.Dispose();
            this.inverterReceiveTask = Task.Run(() => ReceiveInverterData(stoppingToken), stoppingToken);

            //=== This will be the command receiving Task from state machine
            do
            {
                try
                {
                    this.messageReceived.Wait(Timeout.Infinite, stoppingToken);
                }
                catch (OperationCanceledException ex)
                {
                    return Task.FromException(ex);
                }

                this.messageReceived.Reset();

                //=== Identify message and start relevant state machine
                Event_Message receivedMessage;

                while (this.messageQueue.TryDequeue(out receivedMessage))
                {
                    switch (receivedMessage.Type)
                    {
                        case MessageType.StartAction:
                            this.commandQueue.Enqueue(receivedMessage);
                            this.inverterCommandReceived.Set();
                            //=== Create and Run Horizontal Homing State Machine
                            break;
                    }
                }
            } while (stoppingToken.IsCancellationRequested);

            return Task.CompletedTask;
        }

        private void ProcessCommand()
        {
            //TODO Create relevant state machine to send commands to the inverter
        }

        private void ProcessPriorityCommand()
        {
            //TODO Send single message high priority messages to the inverter
        }

        private async void ReceiveInverterData(CancellationToken stoppingToken)
        {
            this.socketTransport.Configure(IPAddress.Parse("169.254.231.248"), InverterPortNumber);

            bool connectionCompleted;
            try
            {
                connectionCompleted = await this.socketTransport.ConnectAsync();
            }
            catch (Exception Ex)
            {
                throw new InverterDriverException($"Exception {Ex.Message} while Connecting Receiver Socket Transport", Ex);
            }

            if (!connectionCompleted)
            {
                throw new InverterDriverException("Socket Transport failed to connect");
            }

            byte[] inverterData;
            do
            {
                try
                {
                    inverterData = await this.socketTransport.ReadAsync(stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    return;
                }

                this.eventAggregator.GetEvent<MachineAutomationService_Event>().Publish(new Event_Message());
            } while (stoppingToken.IsCancellationRequested);
        }

        private void SendHeartBeat(object state)
        {
            this.priorityQueue.Enqueue(new Event_Message());
            this.priorityInverterCommandReceived.Set();
        }

        private void SendInverterCommand(CancellationToken cancellationToken)
        {
            var cancellationEventSlim = new ManualResetEventSlim(false);

            cancellationToken.Register(() => cancellationEventSlim.Set());

            //=== Create WaitHandle array
            WaitHandle[] commandHandles = new[]{ this.priorityInverterCommandReceived.WaitHandle,
                                                 this.inverterCommandReceived.WaitHandle,
                                                 cancellationEventSlim.WaitHandle };

            do
            {
                var handleIndex = WaitHandle.WaitAny(commandHandles, Timeout.Infinite);
                switch (handleIndex)
                {
                    case 0:
                        this.priorityInverterCommandReceived.Reset();
                        ProcessPriorityCommand();
                        break;

                    case 1:
                        this.inverterCommandReceived.Reset();
                        ProcessCommand();
                        break;

                    case 2:
                        return;
                }
            } while (!cancellationToken.IsCancellationRequested);
        }

        #endregion
    }
}
