using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.BLL.Interfaces.Models;

namespace Ferretto.WMS.App.Controls
{
    public class NotificationDialogService : INotificationDialogService
    {
        #region Constructors

        public NotificationDialogService()
        {
            this.NotifyBox = new NotifyBox();
        }

        #endregion

        #region Properties

        public INotifyBox NotifyBox { get; set; }

        #endregion

        #region Methods

        /// <summary>
        ///  Remove all notifications from notification list and buffer list.
        /// </summary>
        public void ClearNotifications()
        {
            this.NotifyBox.ClearNotifications();
        }

        /// <summary>
        /// Show notification window.
        /// </summary>
        /// <param name="content">The notification object.</param>
        public void ShowNotificationWindow(object content)
        {
            this.NotifyBox.Show(content);
        }

        /// <summary>
        /// Show notification window.
        /// </summary>
        /// <param name="content">The notification object.</param>
        /// <param name="configuration">The notification configuration object.</param>
        public void ShowNotificationWindow(object content, INotificationConfiguration configuration)
        {
            this.NotifyBox.Show(content, configuration);
        }

        #endregion
    }
}
