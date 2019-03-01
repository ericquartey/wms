using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Ferretto.VW.Common_Utils.Enumerations;
using Ferretto.VW.Common_Utils.Events;
using Ferretto.VW.Common_Utils.Messages;
using Ferretto.VW.Common_Utils.Messages.Data;
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

        private readonly IEventAggregator eventAggregator;

        private readonly IHubContext<InstallationHub, IInstallationHub> hub;

        private readonly ConcurrentQueue<CommandMessage> messageQueue;

        private readonly ManualResetEventSlim messageReceived;

        #endregion

        #region Constructors

        public AutomationService(IEventAggregator eventAggregator, IHubContext<InstallationHub, IInstallationHub> hub)
        {
            this.eventAggregator = eventAggregator;
            this.hub = hub;

            this.messageReceived = new ManualResetEventSlim(false);
            this.messageQueue = new ConcurrentQueue<CommandMessage>();

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
            this.TESTStartCycle();
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

        public async void TESTStartCycle()
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
            await Task.Run(() => this.AutomationServiceTaskFunction(stoppingToken), stoppingToken);
        }

        private Task AutomationServiceTaskFunction(CancellationToken stoppingToken)
        {
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

                while (this.messageQueue.TryDequeue(out var receivedMessage))
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

        private void ProcessAddMissionMessage(CommandMessage message)
        {
            //TODO apply Automation Service Business Logic to the message

            message.Source = MessageActor.AutomationService;
            message.Destination = MessageActor.MissionsManager;
            this.eventAggregator.GetEvent<CommandEvent>().Publish(message);
        }

        #endregion
    }
}
