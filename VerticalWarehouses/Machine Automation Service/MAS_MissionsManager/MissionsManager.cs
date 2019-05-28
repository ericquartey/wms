using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Ferretto.VW.Common_Utils.Messages;
using Ferretto.VW.Common_Utils.Messages.Enumerations;
using Ferretto.VW.Common_Utils.Messages.Interfaces;
using Ferretto.VW.MAS_DataLayer.Interfaces;
using Ferretto.VW.MAS_Utils.Events;
using Ferretto.VW.MAS_Utils.Exceptions;
using Ferretto.VW.MAS_Utils.Messages;
using Ferretto.VW.MAS_Utils.Utilities;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Prism.Events;

namespace Ferretto.VW.MAS_MissionsManager
{
    public class MissionsManager : BackgroundService
    {
        #region Fields

        private readonly BlockingConcurrentQueue<CommandMessage> commandQueue;

        private readonly Task commandReceiveTask;

        private readonly IEventAggregator eventAggregator;

        private readonly ILogger logger;

        private readonly ManualResetEventSlim missionExecuted;

        private readonly ManualResetEventSlim missionReady;

        private readonly Dictionary<IMissionMessageData, int> missionsCollection;

        private readonly BlockingConcurrentQueue<NotificationMessage> notificationQueue;

        private readonly Task notificationReceiveTask;

        private bool disposed;

        private IGeneralInfo generalInfo;

        private CancellationToken stoppingToken;

        #endregion

        #region Constructors

        public MissionsManager(
            IEventAggregator eventAggregator,
            ILogger<MissionsManager> logger,
            IGeneralInfo generalInfo)
        {
            logger.LogDebug("1:Method Start");

            this.eventAggregator = eventAggregator;

            this.logger = logger;

            this.missionExecuted = new ManualResetEventSlim(true);

            this.missionReady = new ManualResetEventSlim(false);

            this.generalInfo = generalInfo;

            this.commandQueue = new BlockingConcurrentQueue<CommandMessage>();
            this.notificationQueue = new BlockingConcurrentQueue<NotificationMessage>();

            this.commandReceiveTask = new Task(() => this.CommandReceiveTaskFunction());
            this.notificationReceiveTask = new Task(() => this.NotificationReceiveTaskFunction());

            this.missionsCollection = new Dictionary<IMissionMessageData, int>();
            this.InitializeMethodSubscriptions();

            this.logger.LogDebug("2:Method End");
        }

        #endregion

        #region Methods

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            this.stoppingToken = stoppingToken;

            try
            {
                this.Initialize();
                this.commandReceiveTask.Start();
                this.notificationReceiveTask.Start();
            }
            catch (Exception ex)
            {
                throw new MissionsManagerException($"Exception: {ex.Message} while starting service threads", ex);
            }
        }

        private void CommandReceiveTaskFunction()
        {
            this.logger.LogDebug("1:Method Start");
            do
            {
                CommandMessage receivedMessage;
                try
                {
                    this.commandQueue.TryDequeue(Timeout.Infinite, this.stoppingToken, out receivedMessage);
                    this.logger.LogTrace($"2:Dequeued Message:{receivedMessage.Type}:Destination{receivedMessage.Source}");
                }
                catch (OperationCanceledException)
                {
                    this.logger.LogDebug("3:Method End - Operation Canceled");
                    return;
                }

                switch (receivedMessage.Type)
                {
                }
            } while (!this.stoppingToken.IsCancellationRequested);

            this.logger.LogDebug("4:Method End");
        }

        private void Initialize()
        {
            var baysQuantity = this.generalInfo.BaysQuantity;
        }

        private void InitializeMethodSubscriptions()
        {
            this.logger.LogTrace("1:Commands Subscription");
            var commandEvent = this.eventAggregator.GetEvent<CommandEvent>();
            commandEvent.Subscribe(
                commandMessage =>
                {
                    this.commandQueue.Enqueue(commandMessage);
                },
                ThreadOption.PublisherThread,
                false,
                commandMessage => commandMessage.Destination == MessageActor.MissionsManager || commandMessage.Destination == MessageActor.Any);

            this.logger.LogTrace("2:Notifications Subscription");
            var notificationEvent = this.eventAggregator.GetEvent<NotificationEvent>();
            notificationEvent.Subscribe(
                notificationMessage =>
                {
                    this.notificationQueue.Enqueue(notificationMessage);
                },
                ThreadOption.PublisherThread,
                false,
                notificationMessage => notificationMessage.Destination == MessageActor.AutomationService || notificationMessage.Destination == MessageActor.Any);
        }

        private void NotificationReceiveTaskFunction()
        {
            this.logger.LogDebug("1:Method Start");

            do
            {
                NotificationMessage receivedMessage;
                try
                {
                    this.notificationQueue.TryDequeue(Timeout.Infinite, this.stoppingToken, out receivedMessage);

                    this.logger.LogTrace($"2:Notification received: {receivedMessage.Type}, destination: {receivedMessage.Destination}, source: {receivedMessage.Source}, status: {receivedMessage.Status}");
                }
                catch (OperationCanceledException)
                {
                    this.logger.LogDebug("3:Method End - Operation Canceled");

                    return;
                }

                switch (receivedMessage.Type)
                {
                }
            } while (!this.stoppingToken.IsCancellationRequested);

            this.logger.LogDebug("4:Method End");

            return;
        }

        #endregion
    }
}
