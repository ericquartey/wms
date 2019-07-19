namespace Ferretto.VW.App.Services
{
    public interface IStatusMessageService
    {
        #region Events

        event System.EventHandler<StatusMessageEventArgs> StatusMessageNotified;

        #endregion

        #region Methods

        void Clear();

        void Notify(string message, StatusMessageLevel level = StatusMessageLevel.Info);

        void Notify(System.Exception exception, string detailedMessage = null);

        #endregion
    }
}
