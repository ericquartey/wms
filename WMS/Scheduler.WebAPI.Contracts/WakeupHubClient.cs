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

        #endregion Fields

        #region Constructors

        public WakeupHubClient(string url, string wakeUpPath)
        {
            this.connection = new HubConnectionBuilder()
              .WithUrl(new Uri(new Uri(url), wakeUpPath).AbsoluteUri)
              .Build();

            this.connection.On<string, string>(WakeUpMessageName, this.OnWakeUp_MessageReceived);
            this.connection.On<Mission>(NotifyNewMissionMessageName, this.OnNotifyNewMission_MessageReceived);

            this.connection.Closed += async (error) =>
            {
                System.Diagnostics.Debug.WriteLine("Connection to hub closed!");
                await Task.Delay(new Random().Next(0, 5) * 1000);

                System.Diagnostics.Debug.WriteLine("Retrying connection to hub ...");
                await this.connection.StartAsync();
            };
        }

        #endregion Constructors

        #region Events

        public event EventHandler<MissionEventArgs> NewMissionReceived;

        public event EventHandler<WakeUpEventArgs> WakeupReceived;

        #endregion Events

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

        #endregion Methods
    }
}
