using Ferretto.VW.App.Installation.Interfaces;

namespace Ferretto.VW.App.Installation.Resources
{
    public class NotificationMessageReceivedMessageData : INotificationMessageReceivedMessageData
    {
        #region Constructors

        public NotificationMessageReceivedMessageData(string s)
        {
            this.Message = s;
        }

        #endregion

        #region Properties

        public string Message { get; set; }

        #endregion
    }
}
