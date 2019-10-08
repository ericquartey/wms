using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DeviceManager.ResetFault.Interfaces;
using Ferretto.VW.MAS.Utils.Messages;
using Ferretto.VW.MAS.Utils.Utilities;
using Microsoft.Extensions.Logging;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS.DeviceManager.ResetFault
{
    internal class ResetFaultEndState : StateBase
    {
        #region Fields

        private readonly IResetFaultMachineData machineData;

        private readonly IResetFaultStateData stateData;

        private bool disposed;

        #endregion

        #region Constructors

        public ResetFaultEndState(IResetFaultStateData stateData)
            : base(stateData.ParentMachine, stateData.MachineData.Logger)
        {
            this.stateData = stateData;
            this.machineData = stateData.MachineData as IResetFaultMachineData;
        }

        #endregion

        #region Destructors

        ~ResetFaultEndState()
        {
            this.Dispose(false);
        }

        #endregion

        #region Methods

        public override void ProcessCommandMessage(CommandMessage message)
        {
        }

        public override void ProcessFieldNotificationMessage(FieldNotificationMessage message)
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
                $"Inverter Fault reset completed for bay {this.machineData.TargetBay}",
                MessageActor.FiniteStateMachines,
                MessageActor.FiniteStateMachines,
                MessageType.InverterFaultReset,
                this.machineData.RequestingBay,
                this.machineData.TargetBay,
                StopRequestReasonConverter.GetMessageStatusFromReason(this.stateData.StopRequestReason));

            this.ParentStateMachine.PublishNotificationMessage(notificationMessage);
        }

        public override void Stop(StopRequestReason reason)
        {
            this.Logger.LogDebug("1:Stop Method Empty");
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
