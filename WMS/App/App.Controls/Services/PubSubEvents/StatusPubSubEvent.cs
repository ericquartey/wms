using System;
using CommonServiceLocator;
using DevExpress.Mvvm.Native;
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
                .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                .ConcatStringsWithDelimiter(" ");
            if (!string.IsNullOrEmpty(message))
            {
                this.ShowNotification(type, message, flowDirection);
            }
        }

        public StatusPubSubEvent(
            string title,
            string description,
            StatusType type = StatusType.Info,
            NotificationFlowDirection flowDirection = NotificationFlowDirection.RightBottom)
                : this(
                    description != null ? $"{title}{Environment.NewLine}{description}" : title,
                    type,
                    flowDirection)
        {
        }

        public StatusPubSubEvent(Exception exception, StatusType type = StatusType.Error)
        {
            this.Exception = exception ?? throw new ArgumentNullException(nameof(exception));
            this.Type = type;
            this.Message = exception.Message
                .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)[0];
        }

        #endregion

        #region Properties

        public Exception Exception { get; }

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
