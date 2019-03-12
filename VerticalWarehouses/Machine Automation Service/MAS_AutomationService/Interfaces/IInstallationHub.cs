using System;
using System.Threading.Tasks;
using Ferretto.VW.Common_Utils.Messages.MAStoUIMessages;
using Ferretto.VW.Common_Utils.Messages.MAStoUIMessages.Interfaces;

namespace Ferretto.VW.MAS_AutomationService.Interfaces
{
    public interface IInstallationHub
    {
        #region Methods

        Task OnActionUpdateToAllConnectedClients(ActionUpdateData data);

        Task OnSendMessageToAllConnectedClients(string message);

        Task OnSensorsChangedToAllConnectedClients(bool[] sensors);

        #endregion
    }
}
