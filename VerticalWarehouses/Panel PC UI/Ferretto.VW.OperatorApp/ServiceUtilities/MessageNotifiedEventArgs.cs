using Ferretto.VW.Common_Utils.Messages.Interfaces;

namespace Ferretto.VW.OperatorApp.ServiceUtilities
{
    public class MessageNotifiedEventArgs
    {
        #region Constructors

        public MessageNotifiedEventArgs(IBaseNotificationMessageUI notificationMessage)
        {
            this.NotificationMessage = notificationMessage;
        }

        #endregion

        #region Properties

        public IBaseNotificationMessageUI NotificationMessage { get; }

        #endregion
    }
}
