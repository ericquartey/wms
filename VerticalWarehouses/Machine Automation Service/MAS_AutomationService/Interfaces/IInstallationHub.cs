using System;
using System.Threading.Tasks;

namespace Ferretto.VW.MAS_AutomationService.Interfaces
{
    public interface IInstallationHub
    {
        #region Methods

        Task OnSendMessageToAllConnectedClients(String message);

        #endregion
    }
}
