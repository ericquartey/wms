using System;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.WMS.App.Controls.Interfaces;
using Ferretto.WMS.App.Controls.Services;
using Ferretto.WMS.App.Resources;

namespace Ferretto.WMS.App.Controls
{
    public sealed class StatusBarViewModel : Prism.Mvvm.BindableBase, IDisposable
    {
        #region Fields

        private const int timeToKeepText = 15;

        private readonly IEventService eventService;

        private readonly System.Windows.Threading.DispatcherTimer keepInfoTimer =
            new System.Windows.Threading.DispatcherTimer();

        private readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly object statusbarEventSubscription;

        private string message;

        private string schedulerStatusDescription;

        private string schedulerStatusIconName;

        private StatusType statusType;

        #endregion

        #region Constructors

        public StatusBarViewModel(INotificationService notificationService, IEventService eventService)
        {
            if (notificationService == null)
            {
                throw new ArgumentNullException(nameof(notificationService));
            }

            this.eventService = eventService;

            this.StatusType = StatusType.None;
            this.SchedulerStatusIconName = GetConnectionStatusIcon(notificationService.IsServiceHubConnected);
            this.SchedulerStatusDescription = GetConnectionStatusDescription(notificationService.IsServiceHubConnected);
            this.statusbarEventSubscription = this.eventService.Subscribe<StatusPubSubEvent>(this.OnStatusbarInfoChanged);

            this.keepInfoTimer.Tick += this.KeepInfoTimer_Tick;
            this.keepInfoTimer.Interval = new TimeSpan(0, 0, timeToKeepText);
        }

        #endregion

        #region Properties

        public string Message
        {
            get => this.message;
            set => this.SetProperty(ref this.message, value);
        }

        public string MessageIconName => this.StatusType != StatusType.None ? $"{nameof(Services.StatusType)}{this.StatusType}" : string.Empty;

        public string SchedulerStatusDescription
        {
            get => this.schedulerStatusDescription;
            set => this.SetProperty(ref this.schedulerStatusDescription, value);
        }

        public string SchedulerStatusIconName
        {
            get => this.schedulerStatusIconName;
            set => this.SetProperty(ref this.schedulerStatusIconName, value);
        }

        public StatusType StatusType
        {
            get => this.statusType;
            set
            {
                if (this.SetProperty(ref this.statusType, value))
                {
                    this.RaisePropertyChanged(nameof(this.MessageIconName));
                }
            }
        }

        #endregion

        #region Methods

        public void Dispose()
        {
            this.eventService.Unsubscribe<StatusPubSubEvent>(this.statusbarEventSubscription);
        }

        private static string GetConnectionStatusDescription(bool isServiceHubConnected)
        {
            return isServiceHubConnected ? DesktopApp.ServicesOnline : DesktopApp.ServicesOffline;
        }

        private static string GetConnectionStatusIcon(bool isServiceHubConnected)
        {
            return isServiceHubConnected ? nameof(Icons.ServicesOnline) : nameof(Icons.ServicesOffline);
        }

        private void KeepInfoTimer_Tick(object sender, EventArgs e)
        {
            this.StatusType = StatusType.None;
            this.Message = string.Empty;
            this.keepInfoTimer.Stop();
        }

        private void OnStatusbarInfoChanged(StatusPubSubEvent eventArgs)
        {
            if (eventArgs.Exception != null)
            {
                this.logger.Error(eventArgs.Exception, "Displaying status error message.");
            }
            else
            {
                this.logger.Trace($"Displaying status message '{eventArgs.Message}'.");
            }

            this.keepInfoTimer.Stop();
            if (eventArgs.IsSchedulerOnline.HasValue)
            {
                this.SchedulerStatusIconName = GetConnectionStatusIcon(eventArgs.IsSchedulerOnline.Value);
                this.SchedulerStatusDescription = GetConnectionStatusDescription(eventArgs.IsSchedulerOnline.Value);
            }
            else
            {
                this.StatusType = eventArgs.Type;
                this.Message = eventArgs.Message;
                this.keepInfoTimer.Start();
            }
        }

        #endregion
    }
}
