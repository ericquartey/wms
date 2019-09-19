using System.Collections.Generic;
using System.Linq;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.AutomationService.StateMachines.PowerEnable.Interfaces;
using Ferretto.VW.MAS.Utils.Messages;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS.AutomationService.StateMachines.PowerEnable
{
    public class PowerEnableStartState : StateBase
    {

        #region Fields

        private readonly IPowerEnableMachineData machineData;

        private readonly IPowerEnableStateData stateData;

        private readonly Dictionary<BayNumber, MessageStatus> stateMachineResponses;

        private bool disposed;

        #endregion

        #region Constructors

        public PowerEnableStartState(IPowerEnableStateData stateData)
            : base(stateData.ParentMachine, stateData.MachineData.RequestingBay, stateData.MachineData.Logger)
        {
            this.stateData = stateData;
            this.machineData = stateData.MachineData as IPowerEnableMachineData;

            this.stateMachineResponses = new Dictionary<BayNumber, MessageStatus>();
        }

        #endregion

        #region Destructors

        ~PowerEnableStartState()
        {
            this.Dispose(false);
        }

        #endregion



        #region Methods

        public override void ProcessCommandMessage(CommandMessage message)
        {
        }

        public override void ProcessNotificationMessage(NotificationMessage message)
        {
            switch(message.Type)
            {
                case MessageType.Stop:
                if(message.Status != MessageStatus.OperationStart &&
                    message.Status != MessageStatus.OperationExecuting)
                {
                    if(this.stateMachineResponses.TryGetValue(message.TargetBay, out var stateMachineResponse))
                    {
                        stateMachineResponse = message.Status;
                        this.stateMachineResponses[message.TargetBay] = stateMachineResponse;
                    }
                    else
                    {
                        this.stateMachineResponses.Add(message.TargetBay, message.Status);
                    }
                }

                if(this.stateMachineResponses.Values.Count == this.machineData.ConfiguredBays.Count)
                {
                    if(this.stateMachineResponses.Values.Any(r => r != MessageStatus.OperationEnd))
                    {
                        this.stateData.NotificationMessage = message;
                        this.ParentStateMachine.ChangeState(new PowerEnableErrorState(this.stateData));
                    }
                    else
                    {
                        var commandData = new PowerEnableMessageData(this.machineData.RequestedPowerState);
                        var commandMessage = new CommandMessage(
                            commandData,
                            $"Setting Power enable state to {this.machineData.RequestedPowerState}",
                            MessageActor.FiniteStateMachines,
                            MessageActor.AutomationService,
                            MessageType.PowerEnable,
                            this.RequestingBay);

                        this.ParentStateMachine.PublishCommandMessage(commandMessage);
                    }
                }

                break;

                case MessageType.PowerEnable:

                switch(message.Status)
                {
                    case MessageStatus.OperationEnd:
                    if(this.machineData.RequestedPowerState)
                    {
                        this.ParentStateMachine.ChangeState(new PowerEnableResetFaultState(this.stateData));
                    }
                    else
                    {
                        this.ParentStateMachine.ChangeState(new PowerEnableEndState(this.stateData));
                    }
                    break;

                    case MessageStatus.OperationError:
                    this.stateData.NotificationMessage = message;
                    this.ParentStateMachine.ChangeState(new PowerEnableErrorState(this.stateData));
                    break;
                }
                break;
            }
        }

        public override void Start()
        {
            CommandMessage commandMessage;
            if(this.machineData.RequestedPowerState)
            {
                var commandData = new PowerEnableMessageData(this.machineData.RequestedPowerState);
                commandMessage = new CommandMessage(
                    commandData,
                    $"Setting Power enable state to {this.machineData.RequestedPowerState}",
                    MessageActor.FiniteStateMachines,
                    MessageActor.AutomationService,
                    MessageType.PowerEnable,
                    this.RequestingBay);

                this.ParentStateMachine.PublishCommandMessage(commandMessage);
            }
            else
            {
                var messageData = new StopMessageData(StopRequestReason.Stop);
                commandMessage = new CommandMessage(
                    messageData,
                    "Requesting to stop all Sate Machines currently active",
                    MessageActor.FiniteStateMachines,
                    MessageActor.AutomationService,
                    MessageType.Stop,
                    this.RequestingBay);

                foreach(var configuredBay in this.machineData.ConfiguredBays)
                {
                    var newCommandMessage = new CommandMessage(commandMessage);
                    newCommandMessage.TargetBay = configuredBay.Index;

                    this.ParentStateMachine.PublishCommandMessage(newCommandMessage);
                }
            }

            var notificationData = new PowerEnableMessageData(this.machineData.RequestedPowerState);

            var notificationMessage = new NotificationMessage(
                notificationData,
                $"Starting State Machine to Set Power Enable to {this.machineData.RequestedPowerState}",
                MessageActor.AutomationService,
                MessageActor.AutomationService,
                MessageType.PowerEnable,
                this.RequestingBay,
                BayNumber.None,
                MessageStatus.OperationStart);

            this.ParentStateMachine.PublishNotificationMessage(notificationMessage);
        }

        public override void Stop(StopRequestReason reason)
        {
            this.stateData.StopRequestReason = reason;
            this.ParentStateMachine.ChangeState(new PowerEnableEndState(this.stateData));
        }

        protected override void Dispose(bool disposing)
        {
            if(this.disposed)
            {
                return;
            }

            if(disposing)
            {
            }

            this.disposed = true;

            base.Dispose(disposing);
        }

        #endregion
    }
}
