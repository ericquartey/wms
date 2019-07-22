using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.MAS_Utils.Events;
using Ferretto.VW.MAS_Utils.Messages;
using Ferretto.VW.MAS_Utils.Utilities;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Prism.Events;

namespace Ferretto.VW.MAS.MissionsManager
{
    public abstract class AutomationBackgroundService : BackgroundService
    {
        #region Fields

        private readonly BlockingConcurrentQueue<CommandMessage> commandQueue = new BlockingConcurrentQueue<CommandMessage>();

        private readonly Task commandReceiveTask;

        private readonly BlockingConcurrentQueue<NotificationMessage> notificationQueue = new BlockingConcurrentQueue<NotificationMessage>();

        private readonly Task notificationReceiveTask;

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

            this.commandReceiveTask = new Task(async () => await this.ReceiveCommandsAsync());
            this.notificationReceiveTask = new Task(async () => await this.ReceiveNotificationsAsync());
        }

        #endregion

        #region Properties

        protected IEventAggregator EventAggregator { get; }

        protected ILogger Logger { get; }

        protected CancellationToken StoppingToken { get; private set; }

        #endregion

        #region Methods

        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            await base.StartAsync(cancellationToken);

            this.InitializeMethodSubscriptions();
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            this.StoppingToken = stoppingToken;

            try
            {
                this.commandReceiveTask.Start();
                this.notificationReceiveTask.Start();
            }
            catch (Exception ex)
            {
                throw new Exception($"Exception: {ex.Message} while starting service threads.", ex);
            }

            return Task.CompletedTask;
        }

        protected abstract bool FilterCommand(CommandMessage command);

        protected abstract bool FilterNotification(NotificationMessage notification);

        protected abstract Task OnCommandReceivedAsync(CommandMessage command);

        protected abstract Task OnNotificationReceivedAsync(NotificationMessage message);

        private void InitializeMethodSubscriptions()
        {
            this.Logger.LogTrace("1:Commands Subscription");
            var commandEvent = this.EventAggregator.GetEvent<CommandEvent>();
            commandEvent.Subscribe(
                command => this.commandQueue.Enqueue(command),
                ThreadOption.PublisherThread,
                false,
                command => this.FilterCommand(command));

            this.Logger.LogTrace("2:Notifications Subscription");
            var notificationEvent = this.EventAggregator.GetEvent<NotificationEvent>();
            notificationEvent.Subscribe(
                notification => this.notificationQueue.Enqueue(notification),
                ThreadOption.PublisherThread,
                false,
                notification => this.FilterNotification(notification));
        }

        private async Task ReceiveCommandsAsync()
        {
            do
            {
                CommandMessage command;
                try
                {
                    this.commandQueue.TryDequeue(Timeout.Infinite, this.StoppingToken, out command);
                    this.Logger.LogTrace($"1:Dequeued Message '{command.Type}': Destination '{command.Source}'.");
                }
                catch (OperationCanceledException)
                {
                    this.Logger.LogTrace("2:Method End - Operation Canceled.");
                    return;
                }

                await this.OnCommandReceivedAsync(command);
            }
            while (!this.StoppingToken.IsCancellationRequested);
        }

        private async Task ReceiveNotificationsAsync()
        {
            do
            {
                NotificationMessage notification;
                try
                {
                    this.notificationQueue.TryDequeue(Timeout.Infinite, this.StoppingToken, out notification);

                    this.Logger.LogTrace(
                        $"1:Notification received: {notification.Type}, destination: {notification.Destination}, source: {notification.Source}, status: {notification.Status}");
                }
                catch (OperationCanceledException)
                {
                    this.Logger.LogDebug("2:Method End - Operation Canceled.");

                    return;
                }

                await this.OnNotificationReceivedAsync(notification);
            }
            while (!this.StoppingToken.IsCancellationRequested);
        }

        #endregion
    }
}
