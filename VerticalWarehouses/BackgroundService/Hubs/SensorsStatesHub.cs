using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Ferretto.VW.Utils.Source;

namespace BackgroundService
{
    public class SensorsStatesHub : Hub<ISensorsStatesHub>
    {
        #region Methods

        public override Task OnConnectedAsync()
        {
            return base.OnConnectedAsync();
        }

        public async Task SensorChangedMethod(SensorsStates sensor)
        {
            await this.Clients.All.OnSensorsChanged(sensor);
        }

        #endregion Methods
    }
}
