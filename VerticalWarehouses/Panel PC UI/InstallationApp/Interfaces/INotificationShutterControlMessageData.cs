namespace Ferretto.VW.InstallationApp.Interfaces
{
    public interface INotificationShutterControlMessageData : INotificationMessageData
    {
        #region Properties

        bool ShutterControlEnd { get; set; }

        #endregion
    }
}
