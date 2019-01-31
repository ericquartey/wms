using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;

namespace ZZ_AutomationServiceTESTPROJECT
{
    internal class AutomationServiceHubClient
    {
        #region Fields

        private readonly HubConnection connection;

        #endregion Fields

        #region Constructors

        public AutomationServiceHubClient(string url, string sensorStatePath)
        {
            this.connection = new HubConnectionBuilder()
              .WithUrl(new Uri(new Uri(url), sensorStatePath).AbsoluteUri)
              .Build();

            this.connection.Closed += async (error) =>
            {
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

        #endregion Methods
    }
}
