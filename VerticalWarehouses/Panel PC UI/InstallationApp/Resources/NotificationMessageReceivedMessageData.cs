using Ferretto.VW.InstallationApp.Interfaces;

namespace Ferretto.VW.InstallationApp.Resources
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
