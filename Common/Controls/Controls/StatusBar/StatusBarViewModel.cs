using System;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.Controls.Interfaces;
using Ferretto.Common.Controls.Services;
using Ferretto.Common.Resources;

namespace Ferretto.Common.Controls
{
    public sealed class StatusBarViewModel : Prism.Mvvm.BindableBase, IDisposable
    {
        #region Fields

        private const int timeToKeepText = 15;

        private readonly IEventService eventService;

        private readonly System.Windows.Threading.DispatcherTimer keepInfoTimer =
            new System.Windows.Threading.DispatcherTimer();

        private readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        private string messageIconName;

        private string message;

        private string schedulerStatusIconName;

        private object statusbarEventSubscription;

        #endregion

        #region Constructors

        public StatusBarViewModel(INotificationService notificationService, IEventService eventService)
        {
            this.eventService = eventService;
            this.SchedulerStatusIconName = this.GetConnectionStatusIcon(notificationService.IsServiceHubConnected);
            this.keepInfoTimer.Tick += this.KeepInfoTimer_Tick;
            this.keepInfoTimer.Interval = new TimeSpan(0, 0, timeToKeepText);
            this.statusbarEventSubscription = this.eventService.Subscribe<StatusPubSubEvent>(this.OnStatusbarInfoChanged);
        }

        #endregion

        #region Properties

        public string MessageIconName
        {
            get => this.messageIconName;
            set => this.SetProperty(ref this.messageIconName, value);
        }

        public string Message
        {
            get => this.message;
            set => this.SetProperty(ref this.message, value);
        }

        public string SchedulerStatusIconName
        {
            get => this.schedulerStatusIconName;
            set => this.SetProperty(ref this.schedulerStatusIconName, value);
        }

        #endregion

        #region Methods

        public void Dispose()
        {
            this.eventService.Unsubscribe<StatusPubSubEvent>(this.statusbarEventSubscription);
        }

        private string GetConnectionStatusIcon(bool isServiceHubConnected)
        {
            return isServiceHubConnected ? nameof(Icons.SchedulerOnLine) : nameof(Icons.SchedulerOffLine);
        }

        private string GetIconNameFromStatusType(StatusType status)
        {
            return $"{nameof(StatusType)}{status}";
        }

        private void KeepInfoTimer_Tick(object sender, EventArgs e)
        {
            this.Message = string.Empty;
            this.MessageIconName = string.Empty;
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
            this.Message = eventArgs.Message;
            this.MessageIconName = this.GetIconNameFromStatusType(eventArgs.Type);
            if (eventArgs.IsSchedulerOnline.HasValue)
            {
                this.SchedulerStatusIconName = this.GetConnectionStatusIcon(eventArgs.IsSchedulerOnline.Value);
            }

            this.keepInfoTimer.Start();
        }

        #endregion
    }
}
