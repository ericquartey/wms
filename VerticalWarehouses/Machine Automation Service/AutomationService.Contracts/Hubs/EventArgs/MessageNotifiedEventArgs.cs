using Ferretto.VW.CommonUtils.Messages.Interfaces;

namespace Ferretto.VW.MAS.AutomationService.Contracts.Hubs
{
    public class MessageNotifiedEventArgs : System.EventArgs
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
