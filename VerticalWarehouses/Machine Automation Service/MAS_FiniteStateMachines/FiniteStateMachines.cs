using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Ferretto.VW.Common_Utils.Enumerations;
using Ferretto.VW.Common_Utils.Events;
using Ferretto.VW.Common_Utils.Messages;
using Ferretto.VW.Common_Utils.Messages.Interfaces;
using Ferretto.VW.MAS_FiniteStateMachines.Mission;
using Ferretto.VW.MAS_FiniteStateMachines.NewHoming;
using Ferretto.VW.MAS_FiniteStateMachines.Positioning;
using Ferretto.VW.MAS_FiniteStateMachines.VerticalHoming;
using Ferretto.VW.MAS_InverterDriver;
using Ferretto.VW.MAS_IODriver;
using Microsoft.Extensions.Hosting;
using Prism.Events;

namespace Ferretto.VW.MAS_FiniteStateMachines
{
    public class FiniteStateMachines : BackgroundService
    {
        #region Fields

        private readonly INewInverterDriver driver; //TODO to be removed

        private readonly IEventAggregator eventAggregator;

        private readonly ConcurrentQueue<CommandMessage> messageQueue;

        private readonly ManualResetEventSlim messageReceived;

        private readonly INewRemoteIODriver remoteIODriver; //TODO to be removed

        private readonly StateMachineVerticalPositioning verticalPositioning; //TODO to be removed

        private IStateMachine currentStateMachine;

        #endregion

        #region Constructors

        public FiniteStateMachines(INewInverterDriver driver, INewRemoteIODriver remoteIODriver,
            IEventAggregator eventAggregator)
        {
            //TODO The input args INewInverterDriver driver, INewRemoteIODriver remoteIODriver can be removed

            this.driver = driver; //TODO TO BE REMOVED
            this.remoteIODriver = remoteIODriver; //TODO TO BE REMOVED
            this.eventAggregator = eventAggregator;

            this.messageReceived = new ManualResetEventSlim(false);

            this.messageQueue = new ConcurrentQueue<CommandMessage>();

            var commandEvent = this.eventAggregator.GetEvent<CommandEvent>();
            commandEvent.Subscribe(this.DoAction);

            var machineManagerMessagEvent = this.eventAggregator.GetEvent<CommandEvent>();
            machineManagerMessagEvent.Subscribe(message =>
                {
                    this.messageQueue.Enqueue(message);
                    this.messageReceived.Set();
                },
                ThreadOption.PublisherThread,
                false,
                message => message.Destination == MessageActor.FiniteStateMachines);

            this.StateMachineHoming = new StateMachineHoming(this.driver, this.remoteIODriver, this.eventAggregator); //TODO to be removed
            this.StateMachineVerticalHoming = new StateMachineVerticalHoming(this.driver, this.eventAggregator); //TODO to be removed
            this.verticalPositioning = new StateMachineVerticalPositioning(this.driver, this.eventAggregator); //TODO to be removed
        }

        #endregion

        #region Properties

        public StateMachineHoming StateMachineHoming { get; } //TODO to be removed

        //TODO to be removed
        public StateMachineVerticalHoming StateMachineVerticalHoming { get; }

        #endregion

        #region Methods

        // -----
        //TODO to be removed
        public void Destroy()
        {
            try
            {
                this.driver.Destroy();
            }
            catch (ArgumentNullException exc)
            {
                Debug.WriteLine("The inverter driver does not exist.");
                throw new ArgumentNullException("The inverter driver does not exist.", exc);
            }
            catch (Exception exc)
            {
                Debug.WriteLine("Invalid operation.");
                throw new Exception("Invalid operation", exc);
            }
        }

        public void DoAction(CommandMessage action)
        {
            switch (action.Type)
            {
                case MessageType.Homing:
                    {
                        if (null == this.StateMachineHoming) throw new ArgumentNullException();

                        this.StateMachineHoming.Start();
                        break;
                    }

                case MessageType.StopHoming:
                    {
                        if (null == this.StateMachineHoming) throw new ArgumentNullException();

                        this.StateMachineHoming.Stop();
                        break;
                    }

                case MessageType.ExecuteVerticalPositioning:
                    {
                        if (null == this.verticalPositioning) throw new ArgumentNullException();

                        this.verticalPositioning.Start();
                        break;
                    }

                case MessageType.ExecuteStopVerticalPositioning:
                    {
                        if (null == this.verticalPositioning) throw new ArgumentNullException();

                        this.verticalPositioning.Stop();
                        break;
                    }
            }
        }

        public void DoHoming()
        {
            if (null == this.StateMachineHoming) throw new ArgumentNullException();

            this.StateMachineHoming.Start();
        }

        public void DoVerticalHoming()
        {
            if (null == this.StateMachineVerticalHoming) throw new ArgumentNullException();

            this.StateMachineVerticalHoming.Start();
        }

        // -------

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

                        case MessageType.HorizontalHoming:
                            break;

                        // Added this performed action
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
                // handle the calibration data and pass to the calibrate states machine

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
