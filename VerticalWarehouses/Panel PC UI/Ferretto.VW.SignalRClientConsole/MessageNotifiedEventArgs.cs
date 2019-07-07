using Ferretto.VW.CommonUtils.Messages.Interfaces;

namespace Ferretto.VW.SignalRClientConsole
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
