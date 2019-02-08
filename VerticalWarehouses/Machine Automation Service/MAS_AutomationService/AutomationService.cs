using System;
using System.Threading.Tasks;
using Ferretto.Common.Common_Utils;
using Ferretto.VW.Common_Utils.EventParameters;
using Ferretto.VW.Common_Utils.Events;
using Ferretto.VW.MAS_AutomationService.Hubs;
using Ferretto.VW.MAS_AutomationService.Interfaces;
using Ferretto.VW.MAS_MissionScheduler;
using Microsoft.AspNetCore.SignalR;
using Prism.Events;

namespace Ferretto.VW.MAS_AutomationService
{
    public class AutomationService : IAutomationService
    {
        #region Fields

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

            var inverterNotificationEvent = this.eventAggregator.GetEvent<InverterDriver_NotificationEvent>();
            inverterNotificationEvent.Subscribe(this.SendMessageToAllConnectedClients, ThreadOption.BackgroundThread, false, message => message.OperationStatus == OperationStatus.End);
            this.TESTStartCycle();
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
    }
}
