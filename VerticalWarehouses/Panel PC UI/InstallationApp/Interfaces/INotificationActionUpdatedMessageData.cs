namespace Ferretto.VW.InstallationApp.Interfaces
{
    public interface INotificationActionUpdatedMessageData : INotificationMessageData
    {
        #region Properties

        decimal? CurrentEncoderPosition { get; set; }

        int? CurrentShutterPosition { get; set; }

        #endregion
    }
}
