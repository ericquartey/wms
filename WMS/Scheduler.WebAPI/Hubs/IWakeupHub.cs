using System.Threading.Tasks;

namespace Ferretto.WMS.Scheduler.WebAPI.Hubs
{
    public interface IWakeupHub
    {
        #region Methods

        Task WakeUp(string user, string message);

        #endregion Methods
    }
}
