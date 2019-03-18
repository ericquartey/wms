using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ferretto.VW.Common_Utils.Enumerations;
using Ferretto.VW.Common_Utils.Events;
using Ferretto.VW.Common_Utils.Messages;
using Ferretto.VW.Common_Utils.Messages.Data;
using Ferretto.VW.Common_Utils.Messages.Interfaces;
using Ferretto.VW.Common_Utils.Utilities;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Prism.Events;

namespace Ferretto.VW.MAS_MissionsManager
{
    public class MissionsManager : BackgroundService
    {
        #region Fields

        private readonly Task commadReceiveTask;

        private readonly IEventAggregator eventAggregator;

        private readonly ILogger logger;

        private readonly BlockingConcurrentQueue<CommandMessage> messageQueue;

        private readonly ManualResetEventSlim missionExecuted;

        private readonly Task missionExecutionTask;

        private readonly ManualResetEventSlim missionReady;

        private readonly Dictionary<IMissionMessageData, int> missionsCollection;

        private CancellationToken stoppingToken;

        #endregion

        #region Constructors

        public MissionsManager(IEventAggregator eventAggregator, ILogger<MissionsManager> logger)
        {
            this.eventAggregator = eventAggregator;

            this.logger = logger;

            this.missionExecuted = new ManualResetEventSlim(true);

            this.missionReady = new ManualResetEventSlim(false);

            this.messageQueue = new BlockingConcurrentQueue<CommandMessage>();

            this.missionsCollection = new Dictionary<IMissionMessageData, int>();

            this.commadReceiveTask = new Task(() => this.CommandReceiveTaskFunction());

            this.missionExecutionTask = new Task(() => this.MissionsExecutionTaskFunction());

            var automationServiceMessageEvent = this.eventAggregator.GetEvent<CommandEvent>();
            automationServiceMessageEvent.Subscribe(commandMessage => this.messageQueue.Enqueue(commandMessage),
                ThreadOption.PublisherThread,
                false,
                commandMessage => commandMessage.Destination == MessageActor.MissionsManager);

            var finiteStateMachineMessageEvent = this.eventAggregator.GetEvent<NotificationEvent>();
            finiteStateMachineMessageEvent.Subscribe(x => this.missionExecuted.Set(),
                ThreadOption.PublisherThread,
                false,
                notificationMessage => notificationMessage.Source == MessageActor.FiniteStateMachines &&
                                       notificationMessage.Status == MessageStatus.OperationEnd);

            this.logger.LogInformation("Mission Manager Constructor");
        }

        #endregion

        #region Methods

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            this.stoppingToken = stoppingToken;

            try
            {
                this.commadReceiveTask.Start();
                this.missionExecutionTask.Start();
            }
            catch (Exception ex)
            {
                //TODO Define custom exception
                throw new Exception($"Exception: {ex.Message} while starting service threads", ex);
            }
        }

        private Task CommandReceiveTaskFunction()
        {
            do
            {
                CommandMessage receivedMessage;
                try
                {
                    this.messageQueue.TryDequeue(Timeout.Infinite, this.stoppingToken, out receivedMessage);
                }
                catch (OperationCanceledException)
                {
                    return Task.CompletedTask;
                }
                switch (receivedMessage.Type)
                {
                    case MessageType.AddMission:
                        this.ProcessAddMissionMessage(receivedMessage);
                        break;

                    case MessageType.CreateMission:

                        break;

                    case MessageType.HorizontalHoming:
                        break;
                }
            } while (!this.stoppingToken.IsCancellationRequested);

            return Task.CompletedTask;
        }

        private Task MissionsExecutionTaskFunction()
        {
            do
            {
                try
                {
                    this.missionExecuted.Wait(Timeout.Infinite, this.stoppingToken);
                    this.missionReady.Wait(Timeout.Infinite, this.stoppingToken);
                }
                catch (OperationCanceledException ex)
                {
                    return Task.FromException(ex);
                }

                if (this.missionsCollection.Count != 0)
                {
                    // TODO before removing the mission from the dictionary, execute it
                    this.missionsCollection.Remove(this.missionsCollection.Keys.First());
                    if (this.missionsCollection.Count == 0) this.missionReady.Reset();
                    // TODO publish event to notify to the FSM to begin the action
                    this.missionExecuted.Reset();
                }
                else
                    this.missionReady.Reset();
            } while (!this.stoppingToken.IsCancellationRequested);

            return Task.CompletedTask;
        }

        private void ProcessAddMissionMessage(CommandMessage message)
        {
            var missionData = (MissionMessageData)message.Data;
            var missionPriority = ((MissionMessageData)message.Data).Priority;
            this.missionsCollection.Add(missionData, missionPriority);
            this.missionReady.Set();

            message.Source = MessageActor.MissionsManager;
            message.Destination = MessageActor.FiniteStateMachines;
            this.eventAggregator.GetEvent<CommandEvent>().Publish(message);
        }

        private void ProcessCreateMissionMessage(CommandMessage message)
        {
            //TODO apply Mission Manager Business Logic to the message

            message.Source = MessageActor.MissionsManager;
            message.Destination = MessageActor.FiniteStateMachines;
            this.eventAggregator.GetEvent<CommandEvent>().Publish(message);
        }

        #endregion
    }
}
