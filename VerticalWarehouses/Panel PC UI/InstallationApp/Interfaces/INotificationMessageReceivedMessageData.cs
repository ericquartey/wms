namespace Ferretto.VW.InstallationApp.Interfaces
{
    public interface INotificationMessageReceivedMessageData : INotificationMessageData
    {
        #region Properties

        string Message { get; set; }

        #endregion
    }
}
