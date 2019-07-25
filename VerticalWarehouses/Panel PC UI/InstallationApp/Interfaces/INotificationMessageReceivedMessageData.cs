using Ferretto.VW.App.Services.Interfaces;

namespace Ferretto.VW.App.Installation.Interfaces
{
    public interface INotificationMessageReceivedMessageData : INotificationMessageData
    {
        #region Properties

        string Message { get; set; }

        #endregion
    }
}
