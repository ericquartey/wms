using Ferretto.VW.App.Services.Models;

namespace Ferretto.VW.App.Services
{
    public class PresentationNotificationMessage
    {
        #region Constructors

        public PresentationNotificationMessage(string notificationMessage, NotificationSeverity notificationSeverity, string iconName = null)
        {
            this.Msg = string.IsNullOrEmpty(notificationMessage) ? null : notificationMessage;
            this.NotificationSeverity = notificationSeverity;
            this.IconName = iconName;
        }

        public PresentationNotificationMessage(string notificationMessage)
        {
            this.Msg = notificationMessage;
        }

        public PresentationNotificationMessage(bool clearMessage)
        {
            this.ClearMessage = clearMessage;
        }

        public PresentationNotificationMessage(System.Exception exception)
        {
            this.Exception = exception;
            this.NotificationSeverity = NotificationSeverity.Error;
        }

        #endregion

        #region Properties

        public bool ClearMessage { get; }

        public System.Exception Exception { get; }

        public string IconName { get; }

        public string Msg { get; }

        public NotificationSeverity NotificationSeverity { get; }

        #endregion
    }
}
