using System.Threading.Tasks;

namespace Ferretto.WMS.Scheduler.WebAPI.Contracts
{
    public interface IWakeupHub
    {
        #region Methods

        Task WakeUp(string user, string message);

        #endregion Methods
    }
}
