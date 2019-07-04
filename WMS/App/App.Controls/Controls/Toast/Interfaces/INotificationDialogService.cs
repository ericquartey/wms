using Ferretto.Common.BLL.Interfaces.Models;

namespace Ferretto.Common.BLL.Interfaces
{
    public interface INotificationDialogService
    {
        #region Methods

        /// <summary>
        /// Remove all notifications from notification list and buffer list.
        /// </summary>
        void ClearNotifications();

        /// <summary>
        /// Show notification window.
        /// </summary>
        /// <param name="content">The notification object.</param>
        void ShowNotificationWindow(object content);

        /// <summary>
        /// Show notification window.
        /// </summary>
        /// <param name="content">The notification object.</param>
        /// <param name="configuration">The notification configuration object.</param>
        void ShowNotificationWindow(object content, INotificationConfiguration configuration);

        #endregion
    }
}
