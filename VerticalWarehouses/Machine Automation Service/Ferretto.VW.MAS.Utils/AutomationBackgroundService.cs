using System;
using System.Threading;
using System.Threading.Tasks;
using Ferretto.VW.MAS.Utils.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Prism.Events;

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

        private readonly Thread commandsDequeuingThread;

        private readonly BlockingConcurrentQueue<TNotificationMessage> notificationQueue = new BlockingConcurrentQueue<TNotificationMessage>();

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
            GC.SuppressFinalize(this);
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            this.commandEventSubscriptionToken?.Dispose();
            this.commandEventSubscriptionToken = null;

            this.notificationEventSubscriptionToken?.Dispose();
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
                    if (this.commandQueue.TryDequeue(Timeout.Infinite, this.CancellationToken, out var command)
                        &&
                        command != null)
                    {
                        using (var scope = this.ServiceScopeFactory.CreateScope())
                        {
                            this.OnCommandReceivedAsync(command, scope.ServiceProvider).GetAwaiter().GetResult();
                        }
                    }
                }
                catch (Exception ex) when (ex is ThreadAbortException || ex is OperationCanceledException)
                {
                    this.Logger.LogDebug($"Terminating commands thread for {this.GetType().Name}.");
                    return;
                }
                catch (Exception ex)
                {
                    if (ex.Message.Contains("No master"))
                    {
                        break;
                    }
                    this.Logger.LogError("Error while processing a command: '{details}'.", ex);
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
                    if (this.notificationQueue.TryDequeue(Timeout.Infinite, this.CancellationToken, out var notification)
                        &&
                        notification != null)
                    {
                        using (var scope = this.ServiceScopeFactory.CreateScope())
                        {
                            this.OnNotificationReceivedAsync(notification, scope.ServiceProvider).Wait();
                        }
                    }
                }
                catch (Exception ex) when (ex is ThreadAbortException || ex is OperationCanceledException)
                {
                    this.Logger.LogDebug($"Terminating notifications thread for {this.GetType().Name}.");
                    return;
                }
                catch (Exception ex)
                {
                    if (ex.Message.Contains("No master"))
                    {
                        break;
                    }
                    this.Logger.LogError("Error while processing a notification: '{details}'.", ex);
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
                this.commandEventSubscriptionToken?.Dispose();
                this.notificationEventSubscriptionToken?.Dispose();
                this.commandQueue?.Dispose();
                this.notificationQueue?.Dispose();

                base.Dispose();

                this.isDisposed = true;
            }
        }

        private void InitializeSubscriptions()
        {
            this.commandEventSubscriptionToken = this.EventAggregator
                .GetEvent<TCommandEvent>()
                .Subscribe(
                    command => this.commandQueue.Enqueue(command),
                    ThreadOption.PublisherThread,
                    false,
                    m => m != null && this.FilterCommand(m));

            this.Logger.LogTrace("Subscribed to command events.");

            this.notificationEventSubscriptionToken = this.EventAggregator
                .GetEvent<TNotificationEvent>()
                .Subscribe(
                    notification => this.notificationQueue.Enqueue(notification),
                    ThreadOption.PublisherThread,
                    false,
                    m => m != null && this.FilterNotification(m));

            this.Logger.LogTrace("Subscribed to notification events.");
        }

        #endregion
    }
}
