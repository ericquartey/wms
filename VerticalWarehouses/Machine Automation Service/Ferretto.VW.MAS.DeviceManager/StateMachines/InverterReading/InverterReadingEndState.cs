using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DeviceManager.InverterPowerEnable.Interfaces;
using Ferretto.VW.MAS.DeviceManager.InverterReading.Interfaces;
using Ferretto.VW.MAS.Utils.Messages;
using Ferretto.VW.MAS.Utils.Utilities;
using Microsoft.Extensions.Logging;

namespace Ferretto.VW.MAS.DeviceManager.InverterReading
{
    internal class InverterReadingEndState : StateBase
    {
        #region Fields

        private readonly IInverterPowerEnableMachineData machineData;

        private readonly IInverterReadingStateData stateData;

        #endregion

        #region Constructors

        public InverterReadingEndState(IInverterReadingStateData stateData, ILogger logger)
            : base(stateData?.ParentMachine, logger)
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

        public override void ProcessNotificationMessage(NotificationMessage message)
        {
        }

        public override void Start()
        {
            this.Logger.LogDebug($"1:Starting {this.GetType().Name} with {this.stateData.StopRequestReason} Bay: {this.machineData.TargetBay}");

            var notificationMessage = new NotificationMessage(
                null,
                $"Inverter Reading completed for Bay {this.machineData.TargetBay}",
                MessageActor.DeviceManager,
                MessageActor.DeviceManager,
                MessageType.InverterReading,
                this.machineData.RequestingBay,
                this.machineData.TargetBay,
                StopRequestReasonConverter.GetMessageStatusFromReason(this.stateData.StopRequestReason));

            this.ParentStateMachine.PublishNotificationMessage(notificationMessage);
        }

        public override void Stop(StopRequestReason reason)
        {
            this.Logger.LogDebug("Stop with reason: {reason}", reason);
        }

        #endregion
    }
}
