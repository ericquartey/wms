using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;

namespace Ferretto.VW.InstallationApp
{
    internal class AutomationServiceHubClient
    {
        #region Fields

        private readonly HubConnection connection;

        #endregion Fields

        #region Constructors

        public AutomationServiceHubClient(string url, string automationPath)
        {
            this.connection = new HubConnectionBuilder()
              .WithUrl(new Uri(new Uri(url), automationPath).AbsoluteUri)
              .Build();

            this.connection.Closed += async (error) =>
            {
                Debug.Print("Automation Service Client Reconnecting...\n");
                await Task.Delay(new Random().Next(0, 5) * 1000);
                await this.connection.StartAsync();
            };
        }

        #endregion Constructors

        #region Methods

        public async Task ConnectAsync()
        {
            await this.connection.StartAsync();
        }

        public void ExecuteServerMethod()
        {
            this.connection.InvokeAsync("testing", "hello from client!");
        }

        #endregion Methods
    }
}
