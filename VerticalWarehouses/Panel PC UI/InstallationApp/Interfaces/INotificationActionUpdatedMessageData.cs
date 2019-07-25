using Ferretto.VW.App.Services.Interfaces;

namespace Ferretto.VW.App.Installation.Interfaces
{
    public interface INotificationActionUpdatedMessageData : INotificationMessageData
    {
        #region Properties

        decimal? CurrentEncoderPosition { get; set; }

        int? CurrentShutterPosition { get; set; }

        #endregion
    }
}
