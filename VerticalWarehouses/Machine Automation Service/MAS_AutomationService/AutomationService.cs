using System;
using System.Threading;
using System.Threading.Tasks;
using Ferretto.VW.Common_Utils.Enumerations;
using Ferretto.VW.Common_Utils.Events;
using Ferretto.VW.Common_Utils.Messages;
using Ferretto.VW.Common_Utils.Messages.Data;
using Ferretto.VW.Common_Utils.Utilities;
using Ferretto.VW.MAS_AutomationService.Hubs;
using Ferretto.VW.MAS_AutomationService.Interfaces;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Hosting;
using Prism.Events;
using Ferretto.VW.Common_Utils.Messages.Interfaces;

namespace Ferretto.VW.MAS_AutomationService
{
    public class AutomationService : BackgroundService
    {
        #region Fields

        private readonly Task commadReceiveTask;

        private readonly IEventAggregator eventAggregator;

        private readonly IHubContext<InstallationHub, IInstallationHub> hub;

        private readonly BlockingConcurrentQueue<CommandMessage> messageQueue;

        private ManualResetEventSlim messageReceived;

        private CancellationToken stoppingToken;

        #endregion

        #region Constructors

        public AutomationService(IEventAggregator eventAggregator, IHubContext<InstallationHub, IInstallationHub> hub)
        {
            this.eventAggregator = eventAggregator;
            this.hub = hub;

            this.messageReceived = new ManualResetEventSlim(false);
            this.messageQueue = new BlockingConcurrentQueue<CommandMessage>();
            this.messageQueue = new BlockingConcurrentQueue<CommandMessage>();

            this.commadReceiveTask = new Task(() => CommandReceiveTaskFunction());

            this.InitializeMethodSubscription();
            //this.StartTestCycles();
        }

        #endregion

        #region Methods

        public void SendMessageToAllConnectedClients(NotificationMessage notificationMessage)
        {
            this.hub.Clients.All.OnSendMessageToAllConnectedClients(notificationMessage.Description);
        }

        public new Task StopAsync(CancellationToken stoppingToken)
        {
            var returnValue = base.StopAsync(stoppingToken);

            return returnValue;
        }

        public async void TESTStartBoolSensorsCycle()
        {
            while (true)
            {
                var message = new bool[] { (new Random().Next(10) % 2 == 0), (new Random().Next(10) % 2 == 0), (new Random().Next(10) % 2 == 0), (new Random().Next(10) % 2 == 0), };
                Console.WriteLine(message[0].ToString() + " " + message[1].ToString() + " " + message[2].ToString() + " " + message[3].ToString());
                await this.hub.Clients.All.OnSensorsChangedToAllConnectedClients(message);
                await Task.Delay(1000);
            }
        }

        public async void TESTStartStringMessageCycle()
        {
            while (true)
            {
                var message = new[] { "pippo", "topolino", "pluto", "paperino", "minnie", "qui", "quo", "qua" };
                var randomInt = new Random().Next(message.Length);
                Console.WriteLine(message[randomInt]);
                await this.hub.Clients.All.OnSendMessageToAllConnectedClients(message[randomInt]);
                await Task.Delay(1000);
            }
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            this.stoppingToken = stoppingToken;

            try
            {
                this.commadReceiveTask.Start();
            }
            catch (Exception ex)
            {
                //TODO define custom Exception
                throw new Exception($"Exception: {ex.Message} while starting service threads", ex);
            }
        }

        private Task CommandReceiveTaskFunction()
        {
            do
            {
                CommandMessage receivedMessage;
                try
                {
                    this.messageQueue.TryDequeue(Timeout.Infinite, this.stoppingToken, out receivedMessage);
                }
                catch (OperationCanceledException ex)
                {
                    return Task.FromException(ex);
                }
                switch (receivedMessage.Type)
                {
                    case MessageType.AddMission:
                        this.ProcessAddMissionMessage(receivedMessage);
                        break;

                    case MessageType.HorizontalHoming:
                        break;
                }
            } while (!stoppingToken.IsCancellationRequested);
            return Task.CompletedTask;
        }

        private void InitializeMethodSubscription()
        {
            var webApiMessagEvent = this.eventAggregator.GetEvent<CommandEvent>();
            webApiMessagEvent.Subscribe(message =>
            {
                this.messageQueue.Enqueue(message);
                this.messageReceived.Set();
            },
                ThreadOption.PublisherThread,
                false,
                message => message.Destination == MessageActor.AutomationService);

            var finiteStateMachineMessageEvent = this.eventAggregator.GetEvent<NotificationEvent>();
            finiteStateMachineMessageEvent.Subscribe(message =>
            {
                if (message.Data is ISensorsChangedMessageData)
                {
                    this.hub.Clients.All.OnSensorsChangedToAllConnectedClients(((ISensorsChangedMessageData)message.Data).SensorsStates);
                }
            }, ThreadOption.PublisherThread,
            false,
            (message) => message.Source == MessageActor.FiniteStateMachines && message.Type == MessageType.SensorsChanged);
        }

        private void ProcessAddMissionMessage(CommandMessage message)
        {
            //TODO apply Automation Service Business Logic to the message

            message.Source = MessageActor.AutomationService;
            message.Destination = MessageActor.MissionsManager;
            this.eventAggregator.GetEvent<CommandEvent>().Publish(message);
        }

        private async void StartTestCycles()
        {
            this.TESTStartBoolSensorsCycle();
            this.TESTStartStringMessageCycle();
        }

        #endregion
    }
}
