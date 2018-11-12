using System;
using System.Threading.Tasks;
#if NET4
using Microsoft.AspNet.SignalR.Client;
#else
using Microsoft.AspNetCore.SignalR.Client;
#endif

namespace Ferretto.WMS.Scheduler.WebAPI.Contracts
{
    public class WakeupHubClient : IWakeupHubClient
    {
        #region Fields

        private const string WakeUpMessageName = "WakeUp";
        private const string NotifyNewMissionMessageName = "NotifyNewMission";
        public const string WakeUpEndpoint = "wakeup-hub";

        private readonly HubConnection connection;
#if NET4
        private IHubProxy proxy;
#endif

        #endregion Fields

        #region Constructors

        public WakeupHubClient(string url)
        {
#if NET4
            this.connection = new HubConnection(url);
            this.proxy = this.connection.CreateHubProxy(WakeUpEndpoint);

            this.proxy.On<string, string>(WakeUpMessageName, (a, b) => this.OnWakeUp_MessageReceived(a, b));

            this.connection.StateChanged += this.OnConnectionStateChanged;
#else
            this.connection = new HubConnectionBuilder()
              .WithUrl(new Uri(new Uri(url), WakeUpEndpoint).AbsoluteUri)
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
#endif
        }

        #endregion Constructors

        #region Events

        public event EventHandler<WakeUpEventArgs> WakeupReceived;

        public event EventHandler<MissionEventArgs> NewMissionReceived;

        #endregion Events

        #region Methods

#if NET4
        private void OnConnectionStateChanged(StateChange state)
        {
            if (state.NewState == ConnectionState.Disconnected)
            {
                this.ConnectAsync();
            }
        }
#endif

        private void OnWakeUp_MessageReceived(string user, string message)
        {
            this.WakeupReceived?.Invoke(this, new WakeUpEventArgs(user, message));
        }

        private void OnNotifyNewMission_MessageReceived(Mission mission)
        {
            this.NewMissionReceived?.Invoke(this, new MissionEventArgs(mission));
        }

        public async Task ConnectAsync()
        {
#if NET4
            await this.connection.Start();
#else
            await this.connection.StartAsync();
#endif
        }

        #endregion Methods
    }
}
