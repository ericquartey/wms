using System;
using System.Threading.Tasks;

namespace Ferretto.WMS.Scheduler.WebAPI.Contracts
{
    public interface IWakeupHubClient
    {
        #region Events

        event EventHandler<WakeUpEventArgs> WakeupReceived;

        #endregion Events

        #region Methods

        Task ConnectAsync();

        #endregion Methods
    }
}
