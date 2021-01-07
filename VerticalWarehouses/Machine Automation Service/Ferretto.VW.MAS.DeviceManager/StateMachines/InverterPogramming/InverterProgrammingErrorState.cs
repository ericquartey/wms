using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DeviceManager.InverterProgramming.Interfaces;
using Ferretto.VW.MAS.DeviceManager.PowerEnable.Interfaces;
using Ferretto.VW.MAS.Utils.Messages;
using Microsoft.Extensions.Logging;

namespace Ferretto.VW.MAS.DeviceManager.InverterPogramming
{
    internal class InverterProgrammingErrorState : StateBase
    {
        #region Fields

        private readonly IInverterProgrammingMachineData machineData;

        private readonly IInverterProgrammingStateData stateData;

        #endregion

        #region Constructors

        public InverterProgrammingErrorState(IInverterProgrammingStateData stateData, ILogger logger)
            : base(stateData.ParentMachine, logger)
        {
            this.stateData = stateData;
            this.machineData = stateData.MachineData as IInverterProgrammingMachineData;
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
                new InverterProgrammingMessageData(this.machineData.InverterParametersData),
                $"Inverter Programming  stopped due to an error. Filed message: {this.stateData.FieldMessage?.Description ?? string.Empty}",
                MessageActor.DeviceManager,
                MessageActor.DeviceManager,
                MessageType.InverterProgramming,
                this.machineData.RequestingBay,
                this.machineData.TargetBay,
                MessageStatus.OperationError,
                ErrorLevel.Error);

            this.ParentStateMachine.PublishNotificationMessage(notificationMessage);
        }

        public override void Stop(StopRequestReason reason)
        {
            // do nothing
        }

        #endregion
    }
}
