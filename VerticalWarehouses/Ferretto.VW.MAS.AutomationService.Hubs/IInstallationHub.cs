using System.Threading.Tasks;
using Ferretto.VW.CommonUtils.Messages.Interfaces;

namespace Ferretto.VW.MAS.AutomationService.Hubs
{
    public interface IInstallationHub
    {
        #region Methods

        Task CalibrateAxisNotify(IBaseNotificationMessageUI message);

        Task ExceptionNotify(IBaseNotificationMessageUI message);

        Task HomingNotify(IBaseNotificationMessageUI message);

        Task PositioningNotify(IBaseNotificationMessageUI message);

        Task ResolutionCalibrationNotify(IBaseNotificationMessageUI message);

        Task SensorsChangedNotify(IBaseNotificationMessageUI message);

        Task ShutterControlNotify(IBaseNotificationMessageUI message);

        Task ShutterPositioningNotify(IBaseNotificationMessageUI message);

        Task SwitchAxisNotify(IBaseNotificationMessageUI message);

        #endregion

        //Task UpDownRepetitiveNotify(IBaseNotificationMessageUI message);
        // -
        // TODO: Add here methods for each notification message to be sent via SignalR related to a specific type of operation
        // -
    }
}
