using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.AutomationService.StateMachines.PowerEnable.Interfaces;
using Ferretto.VW.MAS.Utils.Messages;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS.AutomationService.StateMachines.PowerEnable
{
    public class PowerEnableResetSecurityState : StateBase
    {

        #region Fields

        private readonly IPowerEnableMachineData machineData;

        private readonly IPowerEnableStateData stateData;

        private bool disposed;

        #endregion

        #region Constructors

        public PowerEnableResetSecurityState(IPowerEnableStateData stateData)
            : base(stateData.ParentMachine, stateData.MachineData.RequestingBay, stateData.MachineData.Logger)
        {
            this.stateData = stateData;
            this.machineData = stateData.MachineData as IPowerEnableMachineData;
        }

        #endregion

        #region Destructors

        ~PowerEnableResetSecurityState()
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
            if (message.Type == MessageType.ResetSecurity)
            {
                switch (message.Status)
                {
                    case MessageStatus.OperationEnd:
                        this.ParentStateMachine.ChangeState(new PowerEnableEndState(this.stateData));
                        break;

                    case MessageStatus.OperationError:
                        this.stateData.NotificationMessage = message;
                        this.ParentStateMachine.ChangeState(new PowerEnableErrorState(this.stateData));
                        break;
                }
            }
        }

        public override void Start()
        {
            var commandMessage = new CommandMessage(
                null,
                $"Setting Power enable state to {this.machineData.RequestedPowerState}",
                MessageActor.FiniteStateMachines,
                MessageActor.AutomationService,
                MessageType.ResetSecurity,
                this.RequestingBay);

            this.ParentStateMachine.PublishCommandMessage(commandMessage);

            var notificationData = new PowerEnableMessageData(this.machineData.RequestedPowerState);

            var notificationMessage = new NotificationMessage(
                notificationData,
                $"Setting machine power to {this.machineData.RequestedPowerState}",
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
