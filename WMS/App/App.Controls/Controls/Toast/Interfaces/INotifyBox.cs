namespace Ferretto.Common.BLL.Interfaces.Models
{
    public interface INotifyBox
    {
        #region Methods

        void ClearNotifications();

        void Show(object content, INotificationConfiguration configuration);

        void Show(object content);

        #endregion
    }
}
