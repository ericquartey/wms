using System.Threading.Tasks;

namespace Ferretto.WMS.Scheduler.WebAPI.Contracts
{
    public interface IWakeupHub
    {
        #region Methods

        Task WakeUpAsync(string user, string message);

        #endregion Methods
    }
}
