using System;
using System.Threading;
using System.Threading.Tasks;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.MAS.Utils.Events;
using Ferretto.VW.MAS.Utils.Messages;
using Ferretto.VW.MAS.Utils.Utilities;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Prism.Events;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS.Utils
{
    public abstract class AutomationBackgroundService : BackgroundService
    {
        #region Fields

        private readonly BlockingConcurrentQueue<CommandMessage> commandQueue = new BlockingConcurrentQueue<CommandMessage>();

        private readonly Task commandReceiveTask;

        private readonly BlockingConcurrentQueue<NotificationMessage> notificationQueue = new BlockingConcurrentQueue<NotificationMessage>();

        private readonly Task notificationReceiveTask;

        private SubscriptionToken commandEventSubscriptionToken;

        private SubscriptionToken notificationEventSubscriptionToken;

        #endregion

        #region Constructors

        public AutomationBackgroundService(
            IEventAggregator eventAggregator,
            ILogger logger)
        {
            if (eventAggregator == null)
            {
                throw new ArgumentNullException(nameof(eventAggregator));
            }

            if (logger == null)
            {
                throw new ArgumentNullException(nameof(logger));
            }

            this.EventAggregator = eventAggregator;
            this.Logger = logger;

            this.commandReceiveTask = new Task(async () => await this.DequeueCommandsAsync());
            this.notificationReceiveTask = new Task(async () => await this.DequeueNotificationsAsync());

            this.InitializeSubscriptions();
        }

        #endregion

        #region Properties

        protected CancellationToken CancellationToken { get; private set; }

        protected IEventAggregator EventAggregator { get; }

        protected ILogger Logger { get; }

        #endregion

        #region Methods

        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            await base.StartAsync(cancellationToken);
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            this.EventAggregator
                .GetEvent<CommandEvent>()
                .Unsubscribe(this.commandEventSubscriptionToken);
            this.commandEventSubscriptionToken = null;

            this.EventAggregator
                .GetEvent<NotificationEvent>()
                .Unsubscribe(this.notificationEventSubscriptionToken);
            this.notificationEventSubscriptionToken = null;

            await base.StopAsync(cancellationToken);
        }

        protected override Task ExecuteAsync(CancellationToken cancellationToken)
        {
            this.CancellationToken = cancellationToken;

            try
            {
                this.commandReceiveTask.Start();
                this.notificationReceiveTask.Start();
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while starting service threads.", ex);
            }

            return Task.CompletedTask;
        }

        protected abstract bool FilterCommand(CommandMessage command);

        protected abstract bool FilterNotification(NotificationMessage notification);

        protected abstract void NotifyCommandError(CommandMessage notificationData);

        protected abstract void NotifyError(NotificationMessage notificationData);

        protected abstract Task OnCommandReceivedAsync(CommandMessage command);

        protected abstract Task OnNotificationReceivedAsync(NotificationMessage message);

        private async Task DequeueCommandsAsync()
        {
            do
            {
                try
                {
                    this.commandQueue.TryDequeue(Timeout.Infinite, this.CancellationToken, out var command);
                    this.Logger.LogTrace(
                        $"Dequeued command '{command.Type}' from '{command.Source}' to '{command.Destination}').");

                    await this.OnCommandReceivedAsync(command);
                }
                catch (OperationCanceledException)
                {
                    this.Logger.LogTrace("Operation Canceled.");
                    return;
                }
            }
            while (!this.CancellationToken.IsCancellationRequested);
        }

        private async Task DequeueNotificationsAsync()
        {
            do
            {
                try
                {
                    this.notificationQueue.TryDequeue(Timeout.Infinite, this.CancellationToken, out var notification);
                    this.Logger.LogTrace(
                        $"Dequeued notification '{notification.Type}', status {notification.Status} (from '{notification.Source}' to '{notification.Destination}').");

                    await this.OnNotificationReceivedAsync(notification);
                }
                catch (OperationCanceledException)
                {
                    this.Logger.LogDebug("Operation canceled.");

                    return;
                }
                catch (Exception ex)
                {
                    this.Logger.LogError(ex, "Error while processing the notification.");
                }
            }
            while (!this.CancellationToken.IsCancellationRequested);
        }

        private void InitializeSubscriptions()
        {
            this.commandEventSubscriptionToken = this.EventAggregator
                .GetEvent<CommandEvent>()
                .Subscribe(
                    command => this.commandQueue.Enqueue(command),
                    ThreadOption.PublisherThread,
                    false,
                    command => this.FilterCommand(command));

            this.Logger.LogTrace("Subscribed to command events.");

            this.notificationEventSubscriptionToken = this.EventAggregator
                .GetEvent<NotificationEvent>()
                .Subscribe(
                    notification => this.notificationQueue.Enqueue(notification),
                    ThreadOption.PublisherThread,
                    false,
                    notification => this.FilterNotification(notification));

            this.Logger.LogTrace("Subscribed to notification events.");
        }

        #endregion
    }
}
