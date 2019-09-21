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
    public class PowerEnableResetFaultState : StateBase
    {

        #region Fields

        private readonly IPowerEnableMachineData machineData;

        private readonly IPowerEnableStateData stateData;

        private readonly Dictionary<BayNumber, MessageStatus> stateMachineResponses;

        private bool disposed;

        #endregion

        #region Constructors

        public PowerEnableResetFaultState(IPowerEnableStateData stateData)
            : base(stateData.ParentMachine, stateData.MachineData.RequestingBay, stateData.MachineData.Logger)
        {
            this.stateData = stateData;
            this.machineData = stateData.MachineData as IPowerEnableMachineData;

            this.stateMachineResponses = new Dictionary<BayNumber, MessageStatus>();
        }

        #endregion

        #region Destructors

        ~PowerEnableResetFaultState()
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
            switch (message.Type)
            {
                case MessageType.InverterFaultReset:
                    if (message.Status != MessageStatus.OperationStart &&
                        message.Status != MessageStatus.OperationExecuting)
                    {
                        if (this.stateMachineResponses.TryGetValue(message.TargetBay, out var stateMachineResponse))
                        {
                            stateMachineResponse = message.Status;
                            this.stateMachineResponses[message.TargetBay] = stateMachineResponse;
                        }
                        else
                        {
                            this.stateMachineResponses.Add(message.TargetBay, message.Status);
                        }
                    }

                    if (this.stateMachineResponses.Values.Count == this.machineData.ConfiguredBays.Count)
                    {
                        if (this.stateMachineResponses.Values.Any(r => r != MessageStatus.OperationEnd))
                        {
                            this.stateData.NotificationMessage = message;
                            this.ParentStateMachine.ChangeState(new PowerEnableErrorState(this.stateData));
                        }
                        else
                        {
                            this.ParentStateMachine.ChangeState(new PowerEnableResetSecurityState(this.stateData));
                        }
                    }

                    break;
            }
        }

        public override void Start()
        {
            if (this.machineData.RequestedPowerState)
            {
                var commandMessage = new CommandMessage(
                    null,
                    "Requesting to stop all Sate Machines currently active",
                    MessageActor.FiniteStateMachines,
                    MessageActor.AutomationService,
                    MessageType.InverterFaultReset,
                    this.RequestingBay);

                foreach (var configuredBay in this.machineData.ConfiguredBays)
                {
                    var newCommandMessage = new CommandMessage(commandMessage);
                    newCommandMessage.TargetBay = configuredBay.Index;

                    this.ParentStateMachine.PublishCommandMessage(newCommandMessage);
                }
            }
            else
            {
                this.ParentStateMachine.ChangeState(new PowerEnableErrorState(this.stateData));
            }

            var notificationData = new PowerEnableMessageData(this.machineData.RequestedPowerState);

            var notificationMessage = new NotificationMessage(
                notificationData,
                "Resetting inverters fault state",
                MessageActor.AutomationService,
                MessageActor.AutomationService,
                MessageType.PowerEnable,
                this.RequestingBay,
                BayNumber.None,
                MessageStatus.OperationExecuting);

            this.ParentStateMachine.PublishNotificationMessage(notificationMessage);
        }

        public override void Stop(StopRequestReason reason)
        {
            this.stateData.StopRequestReason = reason;
            this.ParentStateMachine.ChangeState(new PowerEnableEndState(this.stateData));
        }

        protected override void Dispose(bool disposing)
        {
            if (this.disposed)
            {
                return;
            }

            if (disposing)
            {
            }

            this.disposed = true;

            base.Dispose(disposing);
        }

        #endregion
    }
}
