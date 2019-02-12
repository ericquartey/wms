using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;

namespace Ferretto.WMS.Scheduler.WebAPI.Contracts
{
    public class WakeupHubClient : IWakeupHubClient
    {
        #region Fields

        private const string NotifyNewMissionMessageName = "NotifyNewMission";
        private const string WakeUpMessageName = "WakeUp";
        private readonly HubConnection connection;

        #endregion

        #region Constructors

        public WakeupHubClient(Uri url, string wakeUpPath)
        {
            this.connection = new HubConnectionBuilder()
              .WithUrl(new Uri(url, wakeUpPath).AbsoluteUri)
              .Build();

            this.connection.On<string, string>(WakeUpMessageName, this.OnWakeUp_MessageReceived);
            this.connection.On<Mission>(NotifyNewMissionMessageName, this.OnNotifyNewMission_MessageReceived);

            this.connection.Closed += async (error) =>
            {
                await Task.Delay(new Random().Next(0, 5) * 1000);

                await this.connection.StartAsync();
            };
        }

        #endregion

        #region Events

        public event EventHandler<MissionEventArgs> NewMissionReceived;

        public event EventHandler<WakeUpEventArgs> WakeupReceived;

        #endregion

        #region Methods

        public async Task ConnectAsync()
        {
            await this.connection.StartAsync();
        }

        private void OnNotifyNewMission_MessageReceived(Mission mission)
        {
            this.NewMissionReceived?.Invoke(this, new MissionEventArgs(mission));
        }

        private void OnWakeUp_MessageReceived(string user, string message)
        {
            this.WakeupReceived?.Invoke(this, new WakeUpEventArgs(user, message));
        }

        #endregion
    }
}
