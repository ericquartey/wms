using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using Ferretto.VW.Utils.Source;

namespace Ferretto.VW.InstallationApp.ServiceUtilities
{
    public class SensorsStatesHubClient
    {
        #region Fields

        private readonly HubConnection connection;

        #endregion Fields

        #region Constructors

        public SensorsStatesHubClient(string url, string sensorStatePath)
        {
            this.connection = new HubConnectionBuilder()
              .WithUrl(new Uri(new Uri(url), sensorStatePath).AbsoluteUri)
              .Build();

            this.connection.On<SensorsStates>("SensorsChanged", this.SensorsChanged);

            //this.connection.Closed += async (error) =>
            //{
            //    await Task.Delay(new Random().Next(0, 5) * 1000);
            //    await this.connection.StartAsync();
            //};
        }

        #endregion Constructors

        #region Events

        public event EventHandler<SensorsStatesEventArgs> SensorsStatesChanged;

        #endregion Events

        #region Methods

        public async Task ConnectAsync()
        {
            await this.connection.StartAsync();
        }

        private void SensorsChanged(SensorsStates sensors)
        {
            this.SensorsStatesChanged?.Invoke(this, new SensorsStatesEventArgs(sensors));
        }

        #endregion Methods
    }
}
