using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DeviceManager.ResetFault.Interfaces;
using Ferretto.VW.MAS.Utils.Messages;
using Ferretto.VW.MAS.Utils.Utilities;
using Microsoft.Extensions.Logging;

namespace Ferretto.VW.MAS.DeviceManager.ResetFault
{
    internal class ResetFaultEndState : StateBase
    {
        #region Fields

        private readonly IResetFaultMachineData machineData;

        private readonly IResetFaultStateData stateData;

        #endregion

        #region Constructors

        public ResetFaultEndState(IResetFaultStateData stateData, ILogger logger)
            : base(stateData.ParentMachine, logger)
        {
            this.stateData = stateData;
            this.machineData = stateData.MachineData as IResetFaultMachineData;
        }

        #endregion

        #region Methods

        public override void ProcessCommandMessage(CommandMessage message)
        {
            // do nothing
        }

        public override void ProcessFieldNotificationMessage(FieldNotificationMessage message)
        {
            // do nothing
        }

        /// <inheritdoc/>
        public override void ProcessNotificationMessage(NotificationMessage message)
        {
            // do nothing
        }

        public override void Start()
        {
            this.Logger.LogDebug($"Start {this.GetType().Name}");
            var notificationMessage = new NotificationMessage(
                null,
                $"Inverter Fault reset completed for bay {this.machineData.TargetBay}",
                MessageActor.DeviceManager,
                MessageActor.DeviceManager,
                MessageType.InverterFaultReset,
                this.machineData.RequestingBay,
                this.machineData.TargetBay,
                StopRequestReasonConverter.GetMessageStatusFromReason(this.stateData.StopRequestReason));

            this.ParentStateMachine.PublishNotificationMessage(notificationMessage);
        }

        public override void Stop(StopRequestReason reason)
        {
            // do nothing
        }

        #endregion
    }
}
