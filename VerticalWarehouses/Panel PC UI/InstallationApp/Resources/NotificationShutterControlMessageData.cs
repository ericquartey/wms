using Ferretto.VW.InstallationApp.Interfaces;

namespace Ferretto.VW.InstallationApp.Resources
{
    public class NotificationShutterControlMessageData : INotificationShutterControlMessageData
    {
        #region Constructors

        public NotificationShutterControlMessageData(bool shutterControlEnd)
        {
            this.ShutterControlEnd = shutterControlEnd;
        }

        #endregion

        #region Properties

        public bool ShutterControlEnd { get; set; }

        #endregion
    }
}
