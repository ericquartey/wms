using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DeviceManager.InverterPowerEnable.Interfaces;
using Ferretto.VW.MAS.DeviceManager.InverterProgramming.Interfaces;
using Ferretto.VW.MAS.Utils.Messages;
using Ferretto.VW.MAS.Utils.Utilities;
using Microsoft.Extensions.Logging;

namespace Ferretto.VW.MAS.DeviceManager.InverterPogramming
{
    internal class InverterProgrammingEndState : StateBase
    {
        #region Fields

        private readonly IInverterProgrammingMachineData machineData;

        private readonly IInverterProgrammingStateData stateData;

        #endregion

        #region Constructors

        public InverterProgrammingEndState(IInverterProgrammingStateData stateData, ILogger logger)
            : base(stateData?.ParentMachine, logger)
        {
            this.stateData = stateData;

            this.machineData = stateData?.MachineData as IInverterProgrammingMachineData;
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
                new InverterProgrammingMessageData(this.machineData.InverterParametersData),
                $"Inverter Programming completed for Bay {this.machineData.TargetBay}",
                MessageActor.DeviceManager,
                MessageActor.DeviceManager,
                MessageType.InverterProgramming,
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
