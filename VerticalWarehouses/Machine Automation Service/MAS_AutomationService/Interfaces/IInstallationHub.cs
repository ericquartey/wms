using System.Threading.Tasks;
using Ferretto.VW.MAS_Utils.Messages;
using Ferretto.VW.MAS_Utils.Messages.Data;

namespace Ferretto.VW.MAS_AutomationService.Interfaces
{
    public interface IInstallationHub
    {
        #region Methods

        Task CalibrateAxisNotify(NotificationMessageUI<CalibrateAxisMessageData> message);

        Task SensorsChanged(NotificationMessageUI<SensorsChangedMessageData> message);

        Task SwitchAxisNotify(NotificationMessageUI<SwitchAxisMessageData> message);

        #endregion

        // -
        // TODO: Add here methods for each notification message to be sent via SignalR related to a specific type of operation
        // -
    }
}
