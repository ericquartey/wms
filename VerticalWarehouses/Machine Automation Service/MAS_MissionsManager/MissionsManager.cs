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

        public MissionsManager( IEventAggregator eventAggregator )
        {
            this.eventAggregator = eventAggregator;

            this.messageReceived = new ManualResetEventSlim( false );

            this.missionExecuted = new ManualResetEventSlim( false );

            this.messageQueue = new ConcurrentQueue<Event_Message>();

            this.missionsCollection = new Dictionary<IMissionMessageData, int>();

            var automationServiceMessagEvent = this.eventAggregator.GetEvent<MachineAutomationService_Event>();
            automationServiceMessagEvent.Subscribe( ( message ) =>
             {
                 this.messageQueue.Enqueue( message );
                 this.messageReceived.Set();
             },
                ThreadOption.PublisherThread,
                false,
                message => message.Source == MessageActor.AutomationService );
        }

        #endregion

        #region Methods

        public new Task StopAsync( CancellationToken stoppingToken )
        {
            var returnValue = base.StopAsync( stoppingToken );

            return returnValue;
        }

        protected override async Task ExecuteAsync( CancellationToken stoppingToken )
        {
            await Task.Run( () => this.MissionsManagerTaskFunction( stoppingToken ), stoppingToken );
        }

        private Task MissionsManagerTaskFunction( CancellationToken stoppingToken )
        {
            do
            {
                try
                {
                    this.messageReceived.Wait( Timeout.Infinite, stoppingToken );
                }
                catch(OperationCanceledException ex)
                {
                    return Task.FromException( ex );
                }

                this.messageReceived.Reset();

                Event_Message receivedMessage;

                while(this.messageQueue.TryDequeue( out receivedMessage ))
                {
                    switch(receivedMessage.Type)
                    {
                        case MessageType.AddMission:
                            this.ProcessAddMissionMessage( receivedMessage );
                            break;

                        case MessageType.HorizontalHoming:
                            break;

                        default:
                            throw new InvalidOperationException( "Type of message unmanaged." );
                    }
                }
            } while(!stoppingToken.IsCancellationRequested);

            return Task.CompletedTask;
        }

        private void ProcessAddMissionMessage( Event_Message message )
        {
            try
            {
                var missionData = (MissionMessageData)message.Data;
                var missionPriority = ((MissionMessageData)message.Data).Priority;
                this.missionsCollection.Add( missionData, missionPriority );
            }
            catch(InvalidCastException)
            {
                throw;
            }
            catch(ArgumentNullException)
            {
                throw;
            }
            catch(ArgumentException)
            {
                throw;
            }

            message.Source = MessageActor.MissionsManager;
            message.Destination = MessageActor.FiniteStateMachines;
            this.eventAggregator.GetEvent<MachineAutomationService_Event>().Publish( message );
        }

        private void ProcessCreateMissionMessage( Event_Message message )
        {
            //TODO apply Mission Manager Business Logic to the message

            message.Source = MessageActor.MissionsManager;
            message.Destination = MessageActor.FiniteStateMachines;
            this.eventAggregator.GetEvent<MachineAutomationService_Event>().Publish( message );
        }

        #endregion
    }
}
