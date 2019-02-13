using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Ferretto.Common.Common_Utils;
using Ferretto.VW.Common_Utils.EventParameters;
using Ferretto.VW.Common_Utils.Events;
using Ferretto.VW.Common_Utils.Messages;
using Ferretto.VW.MAS_AutomationService.Hubs;
using Ferretto.VW.MAS_AutomationService.Interfaces;
using Ferretto.VW.MAS_MissionScheduler;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Hosting;
using Prism.Events;

namespace Ferretto.VW.MAS_AutomationService
{
    public class AutomationService : BackgroundService, IAutomationService
    {
        #region Fields

        private readonly ManualResetEventSlim actionRequest;

        private readonly ConcurrentQueue<AutomationService_CommandMessage> actionQueue;

        private readonly IEventAggregator eventAggregator;

        private readonly IHubContext<InstallationHub, IInstallationHub> hub;

        private readonly IMissionsScheduler missionScheduler;

        #endregion

        #region Constructors

        public AutomationService(IMissionsScheduler missionScheduler, IEventAggregator eventAggregator, IHubContext<InstallationHub, IInstallationHub> hub)
        {
            this.missionScheduler = missionScheduler;
            this.eventAggregator = eventAggregator;
            this.hub = hub;

            this.actionRequest = new ManualResetEventSlim( false );

            this.actionQueue = new ConcurrentQueue<AutomationService_CommandMessage>();
        }

        #endregion

        #region Methods

        public bool AddMission(Mission mission)
        {
            if (mission == null) throw new ArgumentNullException();
            this.missionScheduler.AddMission(mission);
            return true;
        }

        public void SendMessageToAllConnectedClients(Notification_EventParameter eventParameter)
        {
            this.hub.Clients.All.OnSendMessageToAllConnectedClients(eventParameter.Description);
        }

        public async void TESTStartCycle()
        {
            while (true)
            {
                var message = new string[] { "pippo", "topolino", "pluto", "paperino", "minnie", "qui", "quo", "qua" };
                var randomInt = new Random().Next(message.Length);
                Console.WriteLine(message[randomInt]);
                await this.hub.Clients.All.OnSendMessageToAllConnectedClients(message[randomInt]);
                await Task.Delay(1000);
            }
        }

        #endregion

        protected override Task ExecuteAsync( CancellationToken stoppingToken )
        {
            var inverterNotificationEvent = this.eventAggregator.GetEvent<InverterDriver_NotificationEvent>();
            inverterNotificationEvent.Subscribe( this.SendMessageToAllConnectedClients, ThreadOption.BackgroundThread, false, message => message.OperationStatus == OperationStatus.End );

            do
            {
                try
                {
                    this.actionRequest.Wait( Timeout.Infinite, stoppingToken );
                }
                catch (OperationCanceledException ex)
                {
                    return Task.CompletedTask;
                }

                this.actionRequest.Reset();

                AutomationService_CommandMessage receivedAction;

                while (this.actionQueue.TryDequeue( out receivedAction ))
                {
                    switch (receivedAction.AutomationCommand)
                    {
                        case AutomationCommandType.HorizontalHoming:
                            break;
                    }
                }

            } while (!stoppingToken.IsCancellationRequested);

            return StopAsync( stoppingToken );
        }

        public new Task StopAsync( CancellationToken stoppingToken )
        {
            var returnValue = base.StopAsync( stoppingToken );

            return returnValue;
        }

    }
}
