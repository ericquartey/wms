using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Ferretto.VW.Common_Utils.Events;
using Ferretto.VW.Common_Utils.Messages;
using Ferretto.VW.Common_Utils.Messages.Data;
using Ferretto.VW.Common_Utils.Messages.Interfaces;
using Microsoft.Extensions.Hosting;
using Prism.Events;
using System.Linq;

namespace Ferretto.VW.MAS_MissionsManager
{
    public class MissionsManager : BackgroundService
    {
        #region Fields

        private readonly IEventAggregator eventAggregator;

        private readonly ConcurrentQueue<Event_Message> messageQueue;

        private readonly ManualResetEventSlim messageReceived;

        private readonly ManualResetEventSlim missionExecuted;

        private readonly Dictionary<IMissionMessageData, int> missionsCollection;

        #endregion

        #region Constructors

        public MissionsManager(IEventAggregator eventAggregator)
        {
            this.eventAggregator = eventAggregator;

            this.messageReceived = new ManualResetEventSlim(false);

            this.missionExecuted = new ManualResetEventSlim(true);

            this.messageQueue = new ConcurrentQueue<Event_Message>();

            this.missionsCollection = new Dictionary<IMissionMessageData, int>();

            var automationServiceMessageEvent = this.eventAggregator.GetEvent<MachineAutomationService_Event>();
            automationServiceMessageEvent.Subscribe((message) =>
            {
                this.messageQueue.Enqueue(message);
                this.messageReceived.Set();
            },
                ThreadOption.PublisherThread,
                false,
                message => message.Source == MessageActor.AutomationService);

            var finiteStateMachineMessageEvent = this.eventAggregator.GetEvent<MachineAutomationService_Event>();
            finiteStateMachineMessageEvent.Subscribe((message) =>
            {
                this.missionExecuted.Set();
            },
                ThreadOption.PublisherThread,
                false,
                message => (message.Source == MessageActor.FiniteStateMachines && message.Status == MessageStatus.End));
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
            Task.Run(() => this.MissionsExecutionTaskFunction(stoppingToken), stoppingToken);
            await Task.Run(() => this.MissionsManagerTaskFunction(stoppingToken), stoppingToken);
        }

        private Task MissionsExecutionTaskFunction(CancellationToken stoppingToken)
        {
            do
            {
                try
                {
                    this.missionExecuted.Wait(Timeout.Infinite, stoppingToken);
                }
                catch (OperationCanceledException ex)
                {
                    return Task.FromException(ex);
                }
                this.missionExecuted.Reset();

                if (this.missionsCollection.Count > 0)
                {
                    var min = this.missionsCollection.Min(x => x.Value);
                    var mostUrgentMissions = this.missionsCollection.Keys.Where(z => z.Priority == min).ToList();
                    if (mostUrgentMissions.Count == 1)
                    {
                        var executeMissionMessage = new Event_Message(mostUrgentMissions[0],
                                                                "Execute Mission",
                                                                MessageActor.FiniteStateMachines,
                                                                MessageActor.MissionsManager,
                                                                MessageStatus.Start,
                                                                MessageType.StartAction,
                                                                MessageVerbosity.Debug);
                        this.eventAggregator.GetEvent<MachineAutomationService_Event>().Publish(executeMissionMessage);
                    }
                    else
                    {
                        // TODO implement multy-bay logic here
                    }
                }
                else
                {
                    try
                    {
                        this.messageReceived.Wait(Timeout.Infinite, stoppingToken);
                    }
                    catch (OperationCanceledException ex)
                    {
                        return Task.FromException(ex);
                    }
                }
            } while (!stoppingToken.IsCancellationRequested);
            return Task.CompletedTask;
        }

        private Task MissionsManagerTaskFunction(CancellationToken stoppingToken)
        {
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
                {
                    switch (receivedMessage.Type)
                    {
                        case MessageType.AddMission:
                            this.ProcessAddMissionMessage(receivedMessage);
                            break;

                        case MessageType.HorizontalHoming:
                            break;

                        default:
                            throw new InvalidOperationException("Type of message unmanaged.");
                    }
                }
            } while (!stoppingToken.IsCancellationRequested);

            return Task.CompletedTask;
        }

        private void ProcessAddMissionMessage(Event_Message message)
        {
            try
            {
                var missionData = (MissionData)message.Data;
                var missionPriority = ((MissionData)message.Data).Priority;
                this.missionsCollection.Add(missionData, missionPriority);
            }
            catch (InvalidCastException)
            {
                throw;
            }
            catch (ArgumentNullException)
            {
                throw;
            }
            catch (ArgumentException)
            {
                throw;
            }

            message.Source = MessageActor.MissionsManager;
            message.Destination = MessageActor.FiniteStateMachines;
            this.eventAggregator.GetEvent<MachineAutomationService_Event>().Publish(message);
        }

        private void ProcessCreateMissionMessage(Event_Message message)
        {
            //TODO apply Mission Manager Business Logic to the message

            message.Source = MessageActor.MissionsManager;
            message.Destination = MessageActor.FiniteStateMachines;
            this.eventAggregator.GetEvent<MachineAutomationService_Event>().Publish(message);
        }

        #endregion
    }
}
