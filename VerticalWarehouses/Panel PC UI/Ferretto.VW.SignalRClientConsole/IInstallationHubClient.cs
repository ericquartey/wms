using System;
using System.Threading.Tasks;

namespace Ferretto.VW.SignalRClientConsole
{
    public interface IInstallationHubClient
    {
        #region Events

        event EventHandler<MessageNotifiedEventArgs> MessageNotified;

        #endregion

        #region Methods

        Task ConnectAsync();

        #endregion
    }
}
