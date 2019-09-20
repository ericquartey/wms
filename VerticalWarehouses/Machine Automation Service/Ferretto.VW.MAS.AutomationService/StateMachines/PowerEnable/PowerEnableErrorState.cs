using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.AutomationService.StateMachines.PowerEnable.Interfaces;
using Ferretto.VW.MAS.Utils.Messages;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS.AutomationService.StateMachines.PowerEnable
{
    public class PowerEnableErrorState : StateBase
    {

        #region Fields

        private readonly IPowerEnableMachineData machineData;

        private readonly IPowerEnableStateData stateData;

        private bool disposed;

        #endregion

        #region Constructors

        public PowerEnableErrorState(IPowerEnableStateData stateData)
            : base(stateData.ParentMachine, stateData.MachineData.RequestingBay, stateData.MachineData.Logger)
        {
            this.stateData = stateData;
            this.machineData = stateData.MachineData as IPowerEnableMachineData;
        }

        #endregion

        #region Destructors

        ~PowerEnableErrorState()
        {
            this.Dispose(false);
        }

        #endregion



        #region Methods

        public override void ProcessCommandMessage(CommandMessage message)
        {
        }

        /// <inheritdoc/>
        public override void ProcessNotificationMessage(NotificationMessage message)
        {
        }

        public override void Start()
        {
            var notificationMessage = new NotificationMessage(
                null,
                $"State Machine encountered an error while setting Power Enable to {this.machineData.RequestedPowerState}. Notification Message: {this.stateData.NotificationMessage.Description} from Bay {this.stateData.NotificationMessage.TargetBay}",
                MessageActor.AutomationService,
                MessageActor.AutomationService,
                MessageType.PowerEnable,
                this.RequestingBay,
                this.stateData.NotificationMessage.TargetBay,
                MessageStatus.OperationError,
                ErrorLevel.Error);

            this.ParentStateMachine.PublishNotificationMessage(notificationMessage);
        }

        public override void Stop(StopRequestReason reason)
        {
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
