using Ferretto.VW.MAS_Utils.Messages.Interfaces;

namespace Ferretto.VW.InstallationApp.ServiceUtilities
{
    public class MessageNotifiedEventArgs
    {
        #region Constructors

        public MessageNotifiedEventArgs(IBaseNotificationMessage notificationMessage)
        {
            this.NotificationMessage = notificationMessage;
        }

        #endregion

        #region Properties

        public IBaseNotificationMessage NotificationMessage { get; }

        #endregion
    }
}
