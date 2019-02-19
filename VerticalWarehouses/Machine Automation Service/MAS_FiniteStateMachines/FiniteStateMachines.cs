using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Ferretto.VW.Common_Utils.EventParameters;
using Ferretto.VW.Common_Utils.Events;
using Ferretto.VW.Common_Utils.Messages;
using Ferretto.VW.MAS_FiniteStateMachines.Mission;
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

        private readonly INewInverterDriver driver;

        private readonly IEventAggregator eventAggregator;

        private readonly StateMachineHoming homing;

        private readonly ConcurrentQueue<Event_Message> messageQueue;

        private readonly ManualResetEventSlim messageReceived;

        private readonly INewRemoteIODriver remoteIODriver;

        private readonly StateMachineVerticalHoming verticalHoming;

        private IStateMachine currentStateMachine;

        #endregion

        #region Constructors

        public FiniteStateMachines(INewInverterDriver driver, INewRemoteIODriver remoteIODriver, IEventAggregator eventAggregator)
        {
            this.driver = driver;
            this.remoteIODriver = remoteIODriver;
            this.eventAggregator = eventAggregator;

            this.messageReceived = new ManualResetEventSlim(false);

            this.messageQueue = new ConcurrentQueue<Event_Message>();

            var commandEvent = this.eventAggregator.GetEvent<WebAPI_CommandEvent>();
            commandEvent.Subscribe(this.DoAction);

            var machineManagerMessagEvent = this.eventAggregator.GetEvent<MachineAutomationService_Event>();
            machineManagerMessagEvent.Subscribe((message) =>
               {
                   this.messageQueue.Enqueue(message);
                   this.messageReceived.Set();
               },
                ThreadOption.PublisherThread,
                false,
                message => message.Source == MessageActor.MachineManager);

            this.homing = new StateMachineHoming(this.driver, this.remoteIODriver, this.eventAggregator);
            this.verticalHoming = new StateMachineVerticalHoming(this.driver, this.eventAggregator);
        }

        #endregion

        #region Properties

        public StateMachineHoming StateMachineHoming => this.homing;

        public StateMachineVerticalHoming StateMachineVerticalHoming => this.verticalHoming;

        #endregion

        #region Methods

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

        public void DoAction(Command_EventParameter action)
        {
            switch (action.CommandType)
            {
                case CommandType.ExecuteHoming:
                    {
                        if (null == this.homing)
                        {
                            throw new ArgumentNullException();
                        }

                        this.homing.Start();
                        break;
                    }

                case CommandType.ExecuteStopHoming:
                    {
                        if (null == this.homing)
                        {
                            throw new ArgumentNullException();
                        }

                        this.homing.Stop();
                        break;
                    }
                default:
                    break;
            }
        }

        public void DoHoming()
        {
            if (null == this.homing)
            {
                throw new ArgumentNullException();
            }

            this.homing.Start();
        }

        public void DoVerticalHoming()
        {
            if (null == this.verticalHoming)
            {
                throw new ArgumentNullException();
            }

            this.verticalHoming.Start();
        }

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
                            this.PorocessAddMissionMessage(receivedMessage);
                            break;

                        case MessageType.HorizontalHoming:
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

        private void PorocessAddMissionMessage(Event_Message message)
        {
            if (this.currentStateMachine != null)
            {
                //TODO throw concurrent action exception
            }

            //TODO apply Finite State Machine Business Logic to the message
            this.currentStateMachine = new MissionStateMachine(this.eventAggregator);

            this.currentStateMachine.Start();
        }

        private void ProcessStopActionMessage(Event_Message receivedMessage)
        {
            if (this.currentStateMachine == null)
            {
                //TODO throw missing state machine exception
            }
        }

        #endregion
    }
}
