using System;
using System.Threading.Tasks;
using Ferretto.WMS.Scheduler.WebAPI.Contracts;
using Microsoft.AspNetCore.SignalR.Client;

namespace Ferretto.WMS.Scheduler.WebAPI
{
    public class WakeupHubClient
    {
        #region Fields

        private readonly HubConnection connection;

        #endregion Fields

        #region Constructors

        public WakeupHubClient(string url)
        {
            this.connection = new HubConnectionBuilder()
              .WithUrl(url)
              .Build();

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

        public event EventHandler<WakeUpEventArgs> WakeupReceived;

        #endregion Events

        #region Methods

        public async Task ConnectAsync()
        {
            this.connection.On<string, string>(nameof(IWakeupHub.WakeUp), (user, message) =>
            {
                this.WakeupReceived?.Invoke(this, new WakeUpEventArgs { User = user, Message = message });
            });

            await this.connection.StartAsync();
        }

        #endregion Methods

        /*   public async Task SendMessageAsync(string userName, string message)
           {
               await this.connection.InvokeAsync("SendMessage", userName, message);
           }*/
    }
}
