using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DeviceManager.InverterPowerEnable.Interfaces;
using Ferretto.VW.MAS.Utils.Messages;
using Ferretto.VW.MAS.Utils.Utilities;
using Microsoft.Extensions.Logging;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS.DeviceManager.InverterPowerEnable
{
    internal class InverterPowerEnableEndState : StateBase
    {
        #region Fields

        private readonly IInverterPowerEnableMachineData machineData;

        private readonly IInverterPowerEnableStateData stateData;

        #endregion

        #region Constructors

        public InverterPowerEnableEndState(IInverterPowerEnableStateData stateData)
                    : base(stateData?.ParentMachine, stateData?.MachineData?.Logger)
        {
            this.stateData = stateData;
            this.machineData = stateData?.MachineData as IInverterPowerEnableMachineData;
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
            this.Logger.LogDebug($"1:Starting {this.GetType().Name} with {this.stateData.StopRequestReason}");

            var notificationMessage = new NotificationMessage(
                null,
                $"InverterPowerEnable completed for Bay {this.machineData.TargetBay}",
                MessageActor.DeviceManager,
                MessageActor.DeviceManager,
                MessageType.InverterPowerEnable,
                this.machineData.RequestingBay,
                this.machineData.TargetBay,
                StopRequestReasonConverter.GetMessageStatusFromReason(this.stateData.StopRequestReason));

            this.ParentStateMachine.PublishNotificationMessage(notificationMessage);
        }

        public override void Stop(StopRequestReason reason)
        {
            this.Logger.LogDebug($"Stop with reason: {reason}");
        }

        #endregion
    }
}
