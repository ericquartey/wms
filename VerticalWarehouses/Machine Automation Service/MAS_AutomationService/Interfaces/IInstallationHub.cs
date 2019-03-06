using System;
using System.Threading.Tasks;

namespace Ferretto.VW.MAS_AutomationService.Interfaces
{
    public interface IInstallationHub
    {
        #region Methods

        Task OnSendMessageToAllConnectedClients(string message);

        Task OnSensorsChangedToAllConnectedClients(bool[] sensors);

        #endregion
    }
}
