using System.Threading.Tasks;
using Ferretto.WMS.Scheduler.Core;

namespace Ferretto.WMS.Scheduler.WebAPI.Hubs
{
    public interface IWakeupHub
    {
        #region Methods

        Task NotifyNewMission(Mission mission);

        Task WakeUp(string user, string message);

        #endregion Methods
    }
}
