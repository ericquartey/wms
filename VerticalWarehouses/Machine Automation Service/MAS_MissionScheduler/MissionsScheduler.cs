using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Ferretto.Common.Common_Utils;
using Ferretto.VW.Common_Utils.Events;
using Ferretto.VW.Common_Utils.Messages;
using Microsoft.Extensions.Hosting;
using Prism.Events;

namespace Ferretto.VW.MAS_MissionScheduler
{
    public class MissionsScheduler : BackgroundService
    {
        #region Fields

        private readonly IEventAggregator eventAggregator;

        private readonly ConcurrentQueue<Event_Message> messageQueue;

        private readonly ManualResetEventSlim messageReceived;

        private readonly Queue<Mission> missionsQueue = new Queue<Mission>();

        #endregion

        #region Constructors

        public MissionsScheduler(IEventAggregator eventAggregator)
        {
            this.eventAggregator = eventAggregator;

            this.messageReceived = new ManualResetEventSlim(false);

            this.messageQueue = new ConcurrentQueue<Event_Message>();

            var automationServiceMessageEvent = this.eventAggregator.GetEvent<MachineAutomationService_Event>();
            automationServiceMessageEvent.Subscribe((message) =>
               {
                   this.messageQueue.Enqueue(message);
                   this.messageReceived.Set();
               },
                ThreadOption.PublisherThread,
                false,
                message => message.Source == MessageActor.AutomationService);
        }

        #endregion

        #region Methods

        public bool AddMission(Mission mission)
        {
            if (mission == null) throw new ArgumentNullException("Mission is null, cannot add a null item to the Mission Queue.\n");
            this.missionsQueue.Enqueue(mission);
            return true;
        }

        public new Task StopAsync(CancellationToken stoppingToken)
        {
            var returnValue = base.StopAsync(stoppingToken);

            return returnValue;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Task.Run(() => this.MissionSchedulerTaskFunction(stoppingToken), stoppingToken);
        }

        private Task MissionSchedulerTaskFunction(CancellationToken stoppingToken)
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

                Event_Message receivedMessage;

                while (this.messageQueue.TryDequeue(out receivedMessage))
                {
                    switch (receivedMessage.Type)
                    {
                        case MessageType.AddMission:
                            this.ProcessAddMissionMessage(receivedMessage);
                            break;

                        case MessageType.HorizontalHoming:
                            break;
                    }
                }
            } while (!stoppingToken.IsCancellationRequested);

            return Task.CompletedTask;
        }

        private void ProcessAddMissionMessage(Event_Message message)
        {
            //TODO apply Mission SchedulerBusiness Logic to the message

            message.Source = MessageActor.MissionScheduler;
            message.Destination = MessageActor.MachineManager;
            this.eventAggregator.GetEvent<MachineAutomationService_Event>().Publish(message);
        }

        #endregion
    }
}
