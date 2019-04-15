using System.Threading.Tasks;
using Ferretto.VW.MAS_Utils.Messages.Interfaces;

namespace Ferretto.VW.MAS_AutomationService.Interfaces
{
    public interface IInstallationHub
    {
        #region Methods

        Task CalibrateAxisNotify(IBaseNotificationMessageUI message);

        Task SensorsChangedNotify(IBaseNotificationMessageUI message);

        Task ShutterPositioning(IBaseNotificationMessageUI message);

        Task SwitchAxisNotify(IBaseNotificationMessageUI message);

        #endregion

        // -
        // TODO: Add here methods for each notification message to be sent via SignalR related to a specific type of operation
        // -
    }
}
