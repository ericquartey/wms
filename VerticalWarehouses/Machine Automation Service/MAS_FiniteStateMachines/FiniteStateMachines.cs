using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Ferretto.VW.Common_Utils.Enumerations;
using Ferretto.VW.Common_Utils.Events;
using Ferretto.VW.Common_Utils.Messages;
using Ferretto.VW.Common_Utils.Messages.Interfaces;
using Ferretto.VW.MAS_FiniteStateMachines.Homing;
using Ferretto.VW.MAS_FiniteStateMachines.Mission;
using Ferretto.VW.MAS_InverterDriver;
using Ferretto.VW.MAS_IODriver;
using Microsoft.Extensions.Hosting;
using Prism.Events;

namespace Ferretto.VW.MAS_FiniteStateMachines
{
    public class FiniteStateMachines : BackgroundService
    {
        #region Fields

        private readonly IEventAggregator eventAggregator;

        private readonly ConcurrentQueue<CommandMessage> messageQueue;

        private readonly ManualResetEventSlim messageReceived;

        private IStateMachine currentStateMachine;

        #endregion

        #region Constructors

        public FiniteStateMachines(INewInverterDriver driver, INewRemoteIODriver remoteIODriver,
            IEventAggregator eventAggregator)
        {
            this.eventAggregator = eventAggregator;

            this.messageReceived = new ManualResetEventSlim(false);

            this.messageQueue = new ConcurrentQueue<CommandMessage>();

            var machineManagerMessagEvent = this.eventAggregator.GetEvent<CommandEvent>();
            machineManagerMessagEvent.Subscribe(message =>
                {
                    this.messageQueue.Enqueue(message);
                    this.messageReceived.Set();
                },
                ThreadOption.PublisherThread,
                false,
                message => message.Destination == MessageActor.FiniteStateMachines);
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
            await Task.Run(() => this.FiniteStateMachineTaskFUnction(stoppingToken), stoppingToken);
        }

        private Task FiniteStateMachineTaskFUnction(CancellationToken stoppingToken)
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

                        //TODO to be removed
                        case MessageType.HorizontalHoming:
                            break;

                        case MessageType.Homing:
                            this.ProcessHomingMessage(receivedMessage);
                            break;

                        case MessageType.StopAction:
                            this.ProcessStopActionMessage(receivedMessage);
                            break;
                    }

                    this.currentStateMachine.NotifyMessage(receivedMessage);
                }
            } while (!stoppingToken.IsCancellationRequested);

            return Task.CompletedTask;
        }

        private void ProcessAddMissionMessage(CommandMessage message)
        {
            if (this.currentStateMachine != null)
            {
                //TODO throw concurrent action exception
            }

            //TODO apply Finite State Machine Business Logic to the message
            this.currentStateMachine = new MissionStateMachine(this.eventAggregator);

            this.currentStateMachine.Start();
        }

        private void ProcessHomingMessage(CommandMessage message)
        {
            if (this.currentStateMachine != null)
            {
                //TODO throw concurrent action exception
            }

            if (message.Data is ICalibrateMessageData data)
            {
                //TODO handle the calibration data and pass to the calibrate states machine
                //TODO apply Finite State Machine Business Logic to the message
                this.currentStateMachine = new HomingStateMachine(this.eventAggregator, data);

                this.currentStateMachine.Start();
            }
        }

        private void ProcessStopActionMessage(CommandMessage receivedMessage)
        {
            if (this.currentStateMachine == null)
            {
                //TODO throw missing state machine exception
            }
        }

        #endregion
    }
}
