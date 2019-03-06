using System.Threading.Tasks;
using Ferretto.WMS.Data.Core.Models;

namespace Ferretto.WMS.Data.WebAPI.Hubs
{
    public interface IWakeupHub
    {
        #region Methods

        Task NotifyNewMissionAsync(Mission mission);

        Task WakeUpAsync(string user, string message);

        #endregion
    }
}
