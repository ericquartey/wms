using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace Ferretto.WMS.Data.WebAPI.Hubs
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Major Code Smell",
        "S4261: Add the 'Async' suffix to the name of this method.",
        Justification = "The methods names here will be exposed by SignalR, so we want that names are clean")]
    public class SchedulerHub : Hub<ISchedulerHub>
    {
        #region Methods

        public Task MissionCompleted(int id)
        {
            return this.Clients.All.MissionCompleted(id);
        }

        #endregion
    }
}
