using Ferretto.VW.InstallationApp.Interfaces;

namespace Ferretto.VW.InstallationApp.Resources
{
    public class NotificationActionUpdatedMessageData : INotificationActionUpdatedMessageData
    {
        #region Constructors

        public NotificationActionUpdatedMessageData(decimal? currentPosition)
        {
            this.CurrentPosition = currentPosition;
        }

        #endregion

        #region Properties

        public decimal? CurrentPosition { get; set; }

        #endregion
    }
}
