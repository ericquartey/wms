using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.AutomationService.StateMachines.PowerEnable.Interfaces;
using Ferretto.VW.MAS.Utils.Messages;
using Ferretto.VW.MAS.Utils.Utilities;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS.AutomationService.StateMachines.PowerEnable
{
    public class PowerEnableEndState : StateBase
    {

        #region Fields

        private readonly IPowerEnableMachineData machineData;

        private readonly IPowerEnableStateData stateData;

        private bool disposed;

        #endregion

        #region Constructors

        public PowerEnableEndState(IPowerEnableStateData stateData)
            : base(stateData.ParentMachine, stateData.MachineData.RequestingBay, stateData.MachineData.Logger)
        {
            this.stateData = stateData;
            this.machineData = stateData.MachineData as IPowerEnableMachineData;
        }

        #endregion

        #region Destructors

        ~PowerEnableEndState()
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
            var notificationData = new PowerEnableMessageData(this.machineData.RequestedPowerState);

            var notificationMessage = new NotificationMessage(
                notificationData,
                $"CLosing State Machine to Set Power Enable to{this.machineData.RequestedPowerState}",
                MessageActor.AutomationService,
                MessageActor.AutomationService,
                MessageType.PowerEnable,
                this.RequestingBay,
                BayNumber.None,
                StopRequestReasonConverter.GetMessageStatusFromReason(this.stateData.StopRequestReason));

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
