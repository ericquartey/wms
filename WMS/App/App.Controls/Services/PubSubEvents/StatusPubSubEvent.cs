using CommonServiceLocator;
using Ferretto.Common.BLL.Interfaces;

namespace Ferretto.WMS.App.Controls.Services
{
    public enum StatusType
    {
        None,

        Info,

        Error,

        Warning,

        Success,
    }

    public class StatusPubSubEvent : Prism.Events.PubSubEvent, IPubSubEvent
    {
        #region Fields

        private readonly INotificationDialogService notificationDialogService = ServiceLocator.Current.GetInstance<INotificationDialogService>();

        #endregion

        #region Constructors

        public StatusPubSubEvent(
            string message = null,
            StatusType type = StatusType.Info,
            NotificationFlowDirection flowDirection = NotificationFlowDirection.RightBottom)
        {
            this.Type = type;
            this.Message = message?
                .Split(new[] { '\r', '\n' }, System.StringSplitOptions.RemoveEmptyEntries)[0];
            if (!string.IsNullOrEmpty(message))
            {
                this.ShowNotification(type, message, flowDirection);
            }
        }

        public StatusPubSubEvent(System.Exception exception, StatusType type = StatusType.Error)
        {
            if (exception == null)
            {
                throw new System.ArgumentNullException(nameof(exception));
            }

            this.Exception = exception;
            this.Type = type;
            this.Message = exception.Message
                .Split(new[] { '\r', '\n' }, System.StringSplitOptions.RemoveEmptyEntries)[0];
        }

        #endregion

        #region Properties

        public System.Exception Exception { get; }

        public bool? IsSchedulerOnline { get; set; }

        public string Message { get; set; }

        public string Token { get; }

        public StatusType Type { get; set; }

        #endregion

        #region Methods

        private void ShowNotification(StatusType statusType, string message, NotificationFlowDirection notificationFlowDirection)
        {
            var notificationConfiguration = NotificationConfiguration.DefaultConfiguration;
            notificationConfiguration.NotificationFlowDirection = notificationFlowDirection;
            var newNotification = new Notification
            {
                Message = message,
                Mode = statusType,
            };

            this.notificationDialogService.ShowNotificationWindow(newNotification, notificationConfiguration);
        }

        #endregion
    }
}
