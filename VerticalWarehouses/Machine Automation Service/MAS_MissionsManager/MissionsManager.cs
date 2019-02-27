using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ferretto.VW.Common_Utils.Enumerations;
using Ferretto.VW.Common_Utils.Events;
using Ferretto.VW.Common_Utils.Messages;
using Ferretto.VW.Common_Utils.Messages.Data;
using Ferretto.VW.Common_Utils.Messages.Interfaces;
using Microsoft.Extensions.Hosting;
using Prism.Events;

namespace Ferretto.VW.MAS_MissionsManager
{
    public class MissionsManager : BackgroundService
    {
        #region Fields

        private readonly IEventAggregator eventAggregator;

        private readonly ConcurrentQueue<CommandMessage> messageQueue;

        private readonly ManualResetEventSlim messageReceived;

        private readonly ManualResetEventSlim missionExecuted;

        private readonly ManualResetEventSlim missionReady;

        private readonly Dictionary<IMissionMessageData, int> missionsCollection;

        private Task missionExecutionTask;

        #endregion

        #region Constructors

        public MissionsManager(IEventAggregator eventAggregator)
        {
            this.eventAggregator = eventAggregator;

            this.messageReceived = new ManualResetEventSlim(false);

            this.missionExecuted = new ManualResetEventSlim(true);

            this.missionReady = new ManualResetEventSlim(false);

            this.messageQueue = new ConcurrentQueue<CommandMessage>();

            this.missionsCollection = new Dictionary<IMissionMessageData, int>();

            var automationServiceMessageEvent = this.eventAggregator.GetEvent<CommandEvent>();
            automationServiceMessageEvent.Subscribe(commandMessage => this.EnqueueMessageAndSetSemaphor(commandMessage),
                ThreadOption.PublisherThread,
                false,
                commandMessage => commandMessage.Destination == MessageActor.MissionsManager);

            var finiteStateMachineMessageEvent = this.eventAggregator.GetEvent<NotificationEvent>();
            finiteStateMachineMessageEvent.Subscribe(x => this.missionExecuted.Set(),
                ThreadOption.PublisherThread,
                false,
                notificationMessage => notificationMessage.Source == MessageActor.FiniteStateMachines &&
                                       notificationMessage.Status == MessageStatus.OperationEnd);
        }

        #endregion

        #region Methods

        public new Task StopAsync(CancellationToken stoppingToken)
        {
            var returnValue = base.StopAsync(stoppingToken);

            return returnValue;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Task.Run(() => this.MissionsManagerTaskFunction(stoppingToken), stoppingToken);
        }

        private void EnqueueMessageAndSetSemaphor(CommandMessage message)
        {
            this.messageQueue.Enqueue(message);
            this.messageReceived.Set();
        }

        private Task MissionsExecutionTaskFunction(CancellationToken stoppingToken)
        {
            do
            {
                try
                {
                    this.missionExecuted.Wait(Timeout.Infinite, stoppingToken);
                    this.missionReady.Wait(Timeout.Infinite, stoppingToken);
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
            } while (!stoppingToken.IsCancellationRequested);

            return Task.CompletedTask;
        }

        private Task MissionsManagerTaskFunction(CancellationToken stoppingToken)
        {
            this.missionExecutionTask =
                Task.Run(() => this.MissionsExecutionTaskFunction(stoppingToken), stoppingToken);
            do
            {
                try
                {
                    this.messageReceived.Wait(Timeout.Infinite, stoppingToken);
                }
                catch (OperationCanceledException ex)
                {
                    return Task.FromException(ex);
                }

                this.messageReceived.Reset();

                while (this.messageQueue.TryDequeue(out var receivedMessage))
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
            } while (!stoppingToken.IsCancellationRequested);

            return Task.CompletedTask;
        }

        private void ProcessAddMissionMessage(CommandMessage message)
        {
            var missionData = (MissionMessageData) message.Data;
            var missionPriority = ( (MissionMessageData) message.Data ).Priority;
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
