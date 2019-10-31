using Ferretto.VW.CommonUtils.Messages.Interfaces;
using Ferretto.VW.MAS.AutomationService.Hubs;

namespace Ferretto.VW.MAS.AutomationService.Contracts.Hubs
{
    public class MessageNotifiedEventArgs : System.EventArgs
    {
        #region Constructors

        public MessageNotifiedEventArgs(IBaseNotificationMessageUI notificationMessage)
        {
            this.NotificationMessage = notificationMessage ?? throw new System.ArgumentNullException(nameof(notificationMessage));
        }

        #endregion

        #region Properties

        public IBaseNotificationMessageUI NotificationMessage { get; }

        #endregion
    }
}
