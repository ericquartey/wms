using Ferretto.VW.InstallationApp.Interfaces;

namespace Ferretto.VW.InstallationApp.Resources
{
    public class NotificationActionUpdatedMessageData : INotificationActionUpdatedMessageData
    {
        #region Constructors

        public NotificationActionUpdatedMessageData(decimal? currentEncoderPosition = null, int? currentShutterPosition = null)
        {
            this.CurrentEncoderPosition = currentEncoderPosition;
            this.CurrentShutterPosition = currentShutterPosition;
        }

        #endregion

        #region Properties

        public decimal? CurrentEncoderPosition { get; set; }

        public int? CurrentShutterPosition { get; set; }

        #endregion
    }
}
