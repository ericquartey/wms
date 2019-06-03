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
using Ferretto.VW.MAS_Utils.Enumerations;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Prism.Events;
using System.Linq;
using Ferretto.WMS.Data.WebAPI.Contracts;

namespace Ferretto.VW.MAS_MissionsManager
{
    public class MissionsManager : BackgroundService
    {
        #region Fields

        private readonly List<Ferretto.VW.MAS_Utils.Utilities.Bay> bays;

        private readonly BlockingConcurrentQueue<CommandMessage> commandQueue;

        private readonly Task commandReceiveTask;

        private readonly IEventAggregator eventAggregator;

        private readonly ILogger logger;

        private readonly Task missionManagementTask;

        private readonly BlockingConcurrentQueue<NotificationMessage> notificationQueue;

        private readonly Task notificationReceiveTask;

        private AutoResetEvent bayNowServiceableResetEvent;

        private bool disposed;

        private IGeneralInfo generalInfo;

        private int lastServedBay = 0;

        private AutoResetEvent newMissionArrivedResetEvent;

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

            this.generalInfo = generalInfo;

            this.commandQueue = new BlockingConcurrentQueue<CommandMessage>();
            this.notificationQueue = new BlockingConcurrentQueue<NotificationMessage>();
            this.bays = new List<MAS_Utils.Utilities.Bay>();

            this.commandReceiveTask = new Task(() => this.CommandReceiveTaskFunction());
            this.notificationReceiveTask = new Task(() => this.NotificationReceiveTaskFunction());
            this.missionManagementTask = new Task(() => this.MissionManagementTaskFunction());

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
                this.commandReceiveTask.Start();
                this.notificationReceiveTask.Start();
            }
            catch (Exception ex)
            {
                throw new MissionsManagerException($"Exception: {ex.Message} while starting service threads", ex);
            }
        }

        private void ChooseAndExecuteMission()
        {
            if (this.bays.ElementAt(0)?.Missions != null && this.bays.ElementAt(0)?.Missions?.Count != 0)
            {
                if (this.bays[0].Missions.TryDequeue(out var mission)) this.ExecuteMission(mission);
            }
            else if (this.bays.ElementAt(1)?.Missions != null && this.bays.ElementAt(1)?.Missions?.Count != 0)
            {
                if (this.bays[1].Missions.TryDequeue(out var mission)) this.ExecuteMission(mission);
            }
            else if (this.bays.ElementAt(2)?.Missions != null && this.bays.ElementAt(2)?.Missions?.Count != 0)
            {
                if (this.bays[2].Missions.TryDequeue(out var mission)) this.ExecuteMission(mission);
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
                    case MessageType.MissionAdded:
                        if (receivedMessage.Data is IMissionMessageData data)
                        {
                            for (int i = 0; i < data.Missions.Count; i++)
                            {
                                switch (data.Missions[i].BayId)
                                {
                                    case 1:
                                        if (this.bays[0] != null)
                                        {
                                            this.bays[0].Missions.Enqueue(data.Missions[i]);
                                            this.newMissionArrivedResetEvent.Set();
                                        }
                                        break;

                                    case 2:
                                        if (this.bays.Count >= 2 && this.bays[1] != null)
                                        {
                                            this.bays[1].Missions.Enqueue(data.Missions[i]);
                                            this.newMissionArrivedResetEvent.Set();
                                        }
                                        break;

                                    case 3:
                                        if (this.bays.Count >= 3 && this.bays[2] != null)
                                        {
                                            this.bays[2].Missions.Enqueue(data.Missions[i]);
                                            this.newMissionArrivedResetEvent.Set();
                                        }
                                        break;

                                    default:
                                        break;
                                }
                            }
                        }
                        break;
                }
            } while (!this.stoppingToken.IsCancellationRequested);

            this.logger.LogDebug("4:Method End");
        }

        private void ExecuteMission(Mission mission)
        {
        }

        private async Task InitializeAsync()
        {
            var baysQuantity = await this.generalInfo.BaysQuantity;
            if (baysQuantity > 0)
            {
                for (int i = 0; i < baysQuantity; i++)
                {
                    this.bays.Add(new MAS_Utils.Utilities.Bay { Id = i, Status = BayStatus.Available });
                }
            }
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
                notificationMessage => notificationMessage.Destination == MessageActor.MissionsManager ||
                notificationMessage.Destination == MessageActor.Any);
        }

        private bool IsAnyBayServiceable()
        {
            return this.bays.ElementAt(0)?.Status == BayStatus.Available || this.bays.ElementAt(1)?.Status == BayStatus.Available || this.bays.ElementAt(2)?.Status == BayStatus.Available;
        }

        private bool IsAnyMissionExecutable()
        {
            var returnValue = false;
            for (int i = 0; i < this.bays.Count; i++)
            {
                if (this.bays.ElementAt(i)?.Status == BayStatus.Available)
                {
                    if (this.bays.ElementAt(i).Missions != null && !this.bays.ElementAt(i).Missions.IsEmpty)
                    {
                        returnValue = true;
                    }
                }
            }
            return returnValue;
        }

        private void MissionManagementTaskFunction()
        {
            this.logger.LogDebug("1:Method Start");
            this.bayNowServiceableResetEvent = new AutoResetEvent(false);
            this.newMissionArrivedResetEvent = new AutoResetEvent(false);
            do
            {
                if (this.IsAnyBayServiceable())
                {
                    if (this.IsAnyMissionExecutable())
                    {
                        this.ChooseAndExecuteMission();
                    }
                    else
                    {
                        WaitHandle.WaitAny(new WaitHandle[] { this.bayNowServiceableResetEvent, this.newMissionArrivedResetEvent, this.stoppingToken.WaitHandle });
                    }
                }
                else
                {
                    WaitHandle.WaitAny(new WaitHandle[] { this.bayNowServiceableResetEvent, this.stoppingToken.WaitHandle });
                }
            } while (!this.stoppingToken.IsCancellationRequested);
        }

        private async void NotificationReceiveTaskFunction()
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
                    case MessageType.MissionCompleted:
                        this.bayNowServiceableResetEvent.Set();
                        break;

                    case MessageType.DataLayerReady:
                        await this.InitializeAsync();
                        this.missionManagementTask.Start();
                        break;
                }
            } while (!this.stoppingToken.IsCancellationRequested);

            this.logger.LogDebug("4:Method End");

            return;
        }

        #endregion
    }
}
