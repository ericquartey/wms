using System;
using System.Threading;
using System.Threading.Tasks;
using Ferretto.VW.Common_Utils.Enumerations;
using Ferretto.VW.Common_Utils.Events;
using Ferretto.VW.Common_Utils.Messages;
using Ferretto.VW.Common_Utils.Utilities;
using Ferretto.VW.MAS_AutomationService.Hubs;
using Ferretto.VW.MAS_AutomationService.Interfaces;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Hosting;
using Prism.Events;

namespace Ferretto.VW.MAS_AutomationService
{
    public class AutomationService : BackgroundService
    {
        #region Fields

        private readonly Task commadReceiveTask;

        private readonly IEventAggregator eventAggregator;

        private readonly IHubContext<InstallationHub, IInstallationHub> hub;

        private readonly BlockingConcurrentQueue<CommandMessage> messageQueue;

        private CancellationToken stoppingToken;

        #endregion

        #region Constructors

        public AutomationService(IEventAggregator eventAggregator, IHubContext<InstallationHub, IInstallationHub> hub)
        {
            this.eventAggregator = eventAggregator;
            this.hub = hub;

            this.messageQueue = new BlockingConcurrentQueue<CommandMessage>();

            this.commadReceiveTask = new Task(() => CommandReceiveTaskFunction());

            var webApiMessagEvent = this.eventAggregator.GetEvent<CommandEvent>();
            webApiMessagEvent.Subscribe(message =>
                {
                    this.messageQueue.Enqueue(message);
                },
                ThreadOption.PublisherThread,
                false,
                message => message.Destination == MessageActor.AutomationService);
            this.TESTStartCycle();
        }

        #endregion

        #region Methods

        public void SendMessageToAllConnectedClients(NotificationMessage notificationMessage)
        {
            this.hub.Clients.All.OnSendMessageToAllConnectedClients(notificationMessage.Description);
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
            } while (!this.stoppingToken.IsCancellationRequested);

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
