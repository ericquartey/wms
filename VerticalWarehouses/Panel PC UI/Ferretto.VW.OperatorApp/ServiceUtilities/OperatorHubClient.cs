using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ferretto.VW.OperatorApp.ServiceUtilities.Interfaces;
using Microsoft.AspNetCore.SignalR.Client;

namespace Ferretto.VW.OperatorApp.ServiceUtilities
{
    internal class OperatorHubClient : IOperatorHubClient
    {
        #region Fields

        private readonly HubConnection hubConnection;

        #endregion

        #region Constructors

        public OperatorHubClient(string url, string installationHubPath)
        {
            this.hubConnection = new HubConnectionBuilder()
              .WithUrl(new Uri(new Uri(url), installationHubPath).AbsoluteUri)
              .Build();

            this.hubConnection.Closed += async (error) =>
            {
                await Task.Delay(new Random().Next(0, 5) * 1000);
                await this.hubConnection.StartAsync();
            };
        }

        #endregion

        #region Events

        public event EventHandler<MessageNotifiedEventArgs> MessageNotified;

        #endregion

        #region Methods

        public async Task ConnectAsync()
        {
            await this.hubConnection.StartAsync();
            Console.WriteLine("Client connection to operator hub status: " + this.hubConnection.State);
        }

        #endregion
    }
}
