using System;
using System.Threading;
using System.Threading.Tasks;
using Ferretto.VW.MAS.Utils.Events;
using Ferretto.VW.MAS.Utils.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Prism.Events;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS.Utils
{
    public abstract class AutomationBackgroundService<TCommandMessage, TNotificationMessage, TCommandEvent, TNotificationEvent> : BackgroundService
        where TCommandMessage : class
        where TNotificationMessage : class
        where TCommandEvent : PubSubEvent<TCommandMessage>, new()
        where TNotificationEvent : PubSubEvent<TNotificationMessage>, new()
    {
        #region Fields

        private readonly BlockingConcurrentQueue<TCommandMessage> commandQueue = new BlockingConcurrentQueue<TCommandMessage>();

        private readonly IServiceScope commandQueueScope;

        private readonly Thread commandsDequeuingThread;

        private readonly BlockingConcurrentQueue<TNotificationMessage> notificationQueue = new BlockingConcurrentQueue<TNotificationMessage>();

        private readonly IServiceScope notificationQueueScope;

        private readonly Thread notificationsDequeuingThread;

        private SubscriptionToken commandEventSubscriptionToken;

        private bool isDisposed;

        private SubscriptionToken notificationEventSubscriptionToken;

        #endregion

        #region Constructors

        public AutomationBackgroundService(
            IEventAggregator eventAggregator,
            ILogger logger,
            IServiceScopeFactory serviceScopeFactory)
        {
            this.EventAggregator = eventAggregator ?? throw new ArgumentNullException(nameof(eventAggregator));
            this.Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.ServiceScopeFactory = serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));

            this.commandQueueScope = serviceScopeFactory.CreateScope();
            this.notificationQueueScope = serviceScopeFactory.CreateScope();

            this.commandsDequeuingThread = new Thread(new ParameterizedThreadStart(this.DequeueCommands))
            {
                Name = $"[commands] {this.GetType().Name}",
            };

            this.notificationsDequeuingThread = new Thread(new ParameterizedThreadStart(this.DequeueNotifications))
            {
                Name = $"[notifications] {this.GetType().Name}",
            };

            this.InitializeSubscriptions();
        }

        #endregion

        #region Properties

        protected CancellationToken CancellationToken { get; private set; }

        protected IEventAggregator EventAggregator { get; }

        protected ILogger Logger { get; }

        protected IServiceScopeFactory ServiceScopeFactory { get; }

        #endregion

        #region Methods

        public override void Dispose()
        {
            this.Dispose(true);
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

        protected void EnqueueCommand(TCommandMessage command)
        {
            if (command is null)
            {
                throw new ArgumentNullException(nameof(command));
            }

            this.commandQueue.Enqueue(command);
        }

        protected override Task ExecuteAsync(CancellationToken cancellationToken)
        {
            this.CancellationToken = cancellationToken;

            try
            {
                this.commandsDequeuingThread.Start(cancellationToken);
                this.notificationsDequeuingThread.Start(cancellationToken);
            }
            catch (Exception ex)
            {
                this.Logger.LogError(ex, $"Error while starting queue threads for {this.GetType().Name}");
                throw;
            }

            return Task.CompletedTask;
        }

        protected abstract bool FilterCommand(TCommandMessage command);

        protected abstract bool FilterNotification(TNotificationMessage notification);

        protected virtual void NotifyCommandError(TCommandMessage notificationData)
        {
            // Do nothing.
            // Behaviour can be customized by inheriting class.
        }

        protected virtual void NotifyError(TNotificationMessage notificationData)
        {
            // Do nothing.
            // Behaviour can be customized by inheriting class.
        }

        protected abstract Task OnCommandReceivedAsync(TCommandMessage command, IServiceProvider serviceProvider);

        protected abstract Task OnNotificationReceivedAsync(TNotificationMessage message, IServiceProvider serviceProvider);

        private void DequeueCommands(object cancellationTokenObject)
        {
            var cancellationToken = (CancellationToken)cancellationTokenObject;

            do
            {
                try
                {
                    if (this.commandQueue.TryDequeue(Timeout.Infinite, this.CancellationToken, out var command))
                    {
                        this.Logger.LogTrace($"Dequeued command {command}.");

                        this.OnCommandReceivedAsync(command, this.commandQueueScope.ServiceProvider).Wait();
                    }
                }
                catch (Exception ex) when (ex is ThreadAbortException || ex is OperationCanceledException)
                {
                    this.Logger.LogDebug($"Terminating commands thread for service {this.GetType().Name}.");
                    return;
                }
                catch (Exception ex)
                {
                    this.Logger.LogError(ex, "Error while processing a command.");
                }
            }
            while (!cancellationToken.IsCancellationRequested);
        }

        private void DequeueNotifications(object cancellationTokenObject)
        {
            var cancellationToken = (CancellationToken)cancellationTokenObject;

            do
            {
                try
                {
                    if (this.notificationQueue.TryDequeue(Timeout.Infinite, this.CancellationToken, out var notification))
                    {
                        this.Logger.LogTrace($"Dequeued notification {notification}");

                        this.OnNotificationReceivedAsync(notification, this.notificationQueueScope.ServiceProvider).Wait();
                    }
                }
                catch (Exception ex) when (ex is ThreadAbortException || ex is OperationCanceledException)
                {
                    this.Logger.LogDebug($"Terminating notifications thread for service {this.GetType().Name}.");
                    return;
                }
                catch (Exception ex)
                {
                    this.Logger.LogError(ex, "Error while processing a notification.");
                }
            }
            while (!cancellationToken.IsCancellationRequested);
        }

        private void Dispose(bool disposing)
        {
            if (this.isDisposed)
            {
                return;
            }

            if (disposing)
            {
                this.commandQueueScope.Dispose();
                this.notificationQueueScope.Dispose();

                this.commandEventSubscriptionToken?.Dispose();
                this.notificationEventSubscriptionToken?.Dispose();
            }

            base.Dispose();

            this.isDisposed = true;
        }

        private void InitializeSubscriptions()
        {
            this.commandEventSubscriptionToken = this.EventAggregator
                .GetEvent<TCommandEvent>()
                .Subscribe(
                    command => this.commandQueue.Enqueue(command),
                    ThreadOption.PublisherThread,
                    false,
                    this.FilterCommand);

            this.Logger.LogTrace("Subscribed to command events.");

            this.notificationEventSubscriptionToken = this.EventAggregator
                .GetEvent<TNotificationEvent>()
                .Subscribe(
                    notification => this.notificationQueue.Enqueue(notification),
                    ThreadOption.PublisherThread,
                    false,
                    this.FilterNotification);

            this.Logger.LogTrace("Subscribed to notification events.");
        }

        #endregion
    }
}
