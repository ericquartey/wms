using System;
using System.Threading.Tasks;
using Ferretto.WMS.Scheduler.WebAPI.Contracts;

namespace Ferretto.WMS.Scheduler.WebAPI
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
