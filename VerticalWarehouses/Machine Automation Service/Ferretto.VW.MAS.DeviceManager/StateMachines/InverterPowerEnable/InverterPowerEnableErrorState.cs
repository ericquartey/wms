using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DeviceManager.InverterPowerEnable.Interfaces;
using Ferretto.VW.MAS.Utils.Messages;
using Microsoft.Extensions.Logging;


namespace Ferretto.VW.MAS.DeviceManager.InverterPowerEnable
{
    internal class InverterPowerEnableErrorState : StateBase
    {
        #region Fields

        private readonly IInverterPowerEnableMachineData machineData;

        private readonly IInverterPowerEnableStateData stateData;

        #endregion

        #region Constructors

        public InverterPowerEnableErrorState(IInverterPowerEnableStateData stateData)
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
                $"InverterPowerEnable failed on bay {this.machineData.TargetBay}. Filed message: {this.stateData.FieldMessage.Description}",
                MessageActor.DeviceManager,
                MessageActor.DeviceManager,
                MessageType.InverterPowerEnable,
                this.machineData.RequestingBay,
                this.machineData.TargetBay,
                MessageStatus.OperationError,
                ErrorLevel.Error);

            this.ParentStateMachine.PublishNotificationMessage(notificationMessage);
        }

        public override void Stop(StopRequestReason reason)
        {
            this.Logger.LogDebug($"Stop with reason: {reason}");
        }

        #endregion
    }
}
