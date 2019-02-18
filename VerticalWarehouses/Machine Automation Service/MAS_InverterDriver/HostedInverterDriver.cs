using System;
using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Ferretto.VW.Common_Utils.Events;
using Ferretto.VW.Common_Utils.Messages;
using Microsoft.Extensions.Hosting;
using Prism.Events;

namespace Ferretto.VW.InverterDriver
{
    public class HostedInverterDriver : BackgroundService
    {
        #region Fields

        private const int HEARTBEAT_TIMEOUT = 300;

        private readonly ConcurrentQueue<Event_Message> commandQueue;

        private readonly IEventAggregator eventAggregator;

        private readonly ManualResetEventSlim inverterCommandReceived;

        private readonly IInverterDriver inverterDriver;

        private readonly ConcurrentQueue<Event_Message> messageQueue;

        private readonly ManualResetEventSlim messageReceived;

        private readonly ManualResetEventSlim priorityInverterCommandReceived;

        private readonly ConcurrentQueue<Event_Message> priorityQueue;

        private Timer heartBeatTimer;

        private Task inverterReceiveTask;

        private Task inverterSendTask;

        private Socket receiveSocket;

        #endregion

        #region Constructors

        public HostedInverterDriver(IEventAggregator eventAggregator, IInverterDriver inverterDriver)
        {
            this.inverterDriver = inverterDriver;
            this.eventAggregator = eventAggregator;
            this.inverterDriver.Initialize();

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
            this.heartBeatTimer = new Timer(this.SendHeartBeat, null, TimeSpan.Zero, TimeSpan.FromMilliseconds(HEARTBEAT_TIMEOUT));

            //=== create and start the sending Task
            this.inverterSendTask?.Dispose();
            this.inverterSendTask = Task.Run(() => this.SendInverterCommand(stoppingToken), stoppingToken);

            //=== create and start the receiving Task
            this.inverterReceiveTask?.Dispose();
            this.inverterReceiveTask = Task.Run(() => this.ReceiveInverterData(stoppingToken), stoppingToken);

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
            //var inverterAddress = IPAddress.Parse( "169.254.231.248" );
            //var inverterPortNumber = 17221;
            //this.receiveSocket = new Socket( inverterAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp );
            //this.receiveSocket.SetSocketOption( SocketOptionLevel.IP, SocketOptionName.ReuseAddress, true);
            //var inverterEndPoint = new IPEndPoint( inverterAddress, inverterPortNumber );

            //this.receiveSocket.Connect( inverterEndPoint );

            //Memory<byte> receiveBuffer = new Memory<byte>(new byte[1024]);

            do
            {
                try
                {
                    //await this.receiveSocket.ReceiveAsync( receiveBuffer, SocketFlags.None, stoppingToken );
                    await Task.Delay(2000);
                    //=== Parse Buffer
                }
                catch (OperationCanceledException ex)
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
            ManualResetEventSlim cancellationEventSlim = new ManualResetEventSlim(false);

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
