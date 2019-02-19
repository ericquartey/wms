using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Ferretto.VW.Common_Utils.Events;
using Ferretto.VW.Common_Utils.Messages;
using Microsoft.Extensions.Hosting;
using Prism.Events;

namespace Ferretto.VW.MAS_MissionsManager
{
    public class MissionsManager : BackgroundService
    {
        #region Fields

        private readonly IEventAggregator eventAggregator;

        private readonly ConcurrentQueue<Event_Message> messageQueue;

        private readonly ManualResetEventSlim messageReceived;

        #endregion

        #region Constructors

        public MissionsManager(IEventAggregator eventAggregator)
        {
            this.eventAggregator = eventAggregator;

            this.messageReceived = new ManualResetEventSlim(false);

            this.messageQueue = new ConcurrentQueue<Event_Message>();

            var automationServiceMessagEvent = this.eventAggregator.GetEvent<MachineAutomationService_Event>();
            automationServiceMessagEvent.Subscribe((message) =>
            {
                this.messageQueue.Enqueue(message);
                this.messageReceived.Set();
            },
                ThreadOption.PublisherThread,
                false,
                message => message.Source == MessageActor.MissionScheduler);
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
            //TODO apply Machine Manager Business Logic to the message

            message.Source = MessageActor.MissionsManager;
            message.Destination = MessageActor.FiniteStateMachines;
            this.eventAggregator.GetEvent<MachineAutomationService_Event>().Publish(message);
        }

        #endregion
    }
}
