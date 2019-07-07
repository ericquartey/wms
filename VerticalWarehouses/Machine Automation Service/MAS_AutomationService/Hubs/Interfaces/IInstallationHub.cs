using System.Threading.Tasks;
using Ferretto.VW.CommonUtils.Messages.Interfaces;

namespace Ferretto.VW.MAS_AutomationService.Interfaces
{
    public interface IInstallationHub
    {
        #region Methods

        Task CalibrateAxisNotify(IBaseNotificationMessageUI message);

        Task ExceptionNotify(IBaseNotificationMessageUI message);

        Task HomingNotify(IBaseNotificationMessageUI message);

        Task ResolutionCalibrationNotify(IBaseNotificationMessageUI message);

        Task SensorsChangedNotify(IBaseNotificationMessageUI message);

        Task ShutterControlNotify(IBaseNotificationMessageUI message);

        Task ShutterPositioningNotify(IBaseNotificationMessageUI message);

        Task SwitchAxisNotify(IBaseNotificationMessageUI message);

        Task UpDownRepetitiveNotify(IBaseNotificationMessageUI message);

        Task VerticalPositioningNotify(IBaseNotificationMessageUI message);

        #endregion

        // -
        // TODO: Add here methods for each notification message to be sent via SignalR related to a specific type of operation
        // -
    }
}
