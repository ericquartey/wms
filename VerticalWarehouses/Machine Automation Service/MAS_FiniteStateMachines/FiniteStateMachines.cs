using System;
using System.Threading;
using System.Threading.Tasks;
using Ferretto.VW.Common_Utils.Enumerations;
using Ferretto.VW.Common_Utils.Events;
using Ferretto.VW.Common_Utils.Messages;
using Ferretto.VW.Common_Utils.Messages.Interfaces;
using Ferretto.VW.Common_Utils.Utilities;
using Ferretto.VW.MAS_FiniteStateMachines.Homing;
using Ferretto.VW.MAS_FiniteStateMachines.Mission;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Prism.Events;

namespace Ferretto.VW.MAS_FiniteStateMachines
{
    public class FiniteStateMachines : BackgroundService
    {
        #region Fields

        private readonly Task commadReceiveTask;

        private readonly IEventAggregator eventAggregator;

        private readonly ILogger logger;

        private readonly BlockingConcurrentQueue<CommandMessage> messageQueue;

        private readonly Task messageReceiveTask;

        private readonly BlockingConcurrentQueue<NotificationMessage> notifyQueue;

        private IStateMachine currentStateMachine;

        private CancellationToken stoppingToken;

        #endregion

        #region Constructors

        public FiniteStateMachines(IEventAggregator eventAggregator, ILogger<FiniteStateMachines> logger)
        {
            this.eventAggregator = eventAggregator;

            this.logger = logger;

            this.messageQueue = new BlockingConcurrentQueue<CommandMessage>();

            this.notifyQueue = new BlockingConcurrentQueue<NotificationMessage>();

            this.commadReceiveTask = new Task(() => this.CommandReceiveTaskFunction());
            this.messageReceiveTask = new Task(() => this.MessageReceiveData());

            var machineManagerMessagEvent = this.eventAggregator.GetEvent<CommandEvent>();
            machineManagerMessagEvent.Subscribe(message =>
                {
                    this.messageQueue.Enqueue(message);
                },
                ThreadOption.PublisherThread,
                false,
                message => message.Destination == MessageActor.FiniteStateMachines || message.Destination == MessageActor.Any);

            var notificationMessageEvent = this.eventAggregator.GetEvent<NotificationEvent>();
            notificationMessageEvent.Subscribe(message =>
                {
                    this.notifyQueue.Enqueue(message);
                },
                ThreadOption.PublisherThread,
                false,
                message => message.Destination == MessageActor.FiniteStateMachines || message.Destination == MessageActor.Any);

            this.logger.LogInformation("Finite State Machine Constructor");
        }

        public FiniteStateMachines(IEventAggregator eventAggregator)
        {
            this.eventAggregator = eventAggregator;

            this.messageQueue = new BlockingConcurrentQueue<CommandMessage>();

            this.notifyQueue = new BlockingConcurrentQueue<NotificationMessage>();

            this.commadReceiveTask = new Task(() => this.CommandReceiveTaskFunction());
            this.messageReceiveTask = new Task(() => this.MessageReceiveData());

            var machineManagerMessagEvent = this.eventAggregator.GetEvent<CommandEvent>();
            machineManagerMessagEvent.Subscribe(message =>
            {
                this.messageQueue.Enqueue(message);
            },
                ThreadOption.PublisherThread,
                false,
                message => message.Destination == MessageActor.FiniteStateMachines || message.Destination == MessageActor.Any);

            var notificationMessageEvent = this.eventAggregator.GetEvent<NotificationEvent>();
            notificationMessageEvent.Subscribe(message =>
            {
                this.notifyQueue.Enqueue(message);
            },
                ThreadOption.PublisherThread,
                false,
                message => message.Destination == MessageActor.FiniteStateMachines || message.Destination == MessageActor.Any);
        }

        #endregion

        #region Methods

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            this.stoppingToken = stoppingToken;

            try
            {
                this.commadReceiveTask.Start();
                this.messageReceiveTask.Start();
            }
            catch (Exception ex)
            {
                //TODO define custom Exception
                throw new Exception($"Exception: {ex.Message} while starting service threads", ex);
            }

            await Task.CompletedTask;
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
                catch (OperationCanceledException ex)
                {
                    return Task.FromException(ex);
                }

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

                    case MessageType.Stop:
                        this.ProcessStopMessage(receivedMessage);
                        break;

                    case MessageType.StopAction:
                        this.ProcessStopActionMessage(receivedMessage);
                        break;
                }

                //TEMP this.currentStateMachine.ProcessCommandMessage(receivedMessage);
            } while (!this.stoppingToken.IsCancellationRequested);

            return Task.CompletedTask;
        }

        private Task MessageReceiveData()
        {
            do
            {
                NotificationMessage receivedMessage;
                try
                {
                    this.notifyQueue.TryDequeue(Timeout.Infinite, this.stoppingToken, out receivedMessage);
                }
                catch (OperationCanceledException)
                {
                    return Task.CompletedTask;
                }

                if (this.currentStateMachine != null)
                {
                    this.currentStateMachine.ProcessNotificationMessage(receivedMessage);
                }
            } while (!this.stoppingToken.IsCancellationRequested);

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

        private void ProcessStopMessage(CommandMessage receivedMessage)
        {
            if (this.currentStateMachine == null)
            {
                //TODO throw missing state machine exception
            }

            this.currentStateMachine.ProcessCommandMessage(receivedMessage);
        }

        #endregion
    }
}
