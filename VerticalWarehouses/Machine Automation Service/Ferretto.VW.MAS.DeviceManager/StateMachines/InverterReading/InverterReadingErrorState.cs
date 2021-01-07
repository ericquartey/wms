using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DeviceManager.InverterReading.Interfaces;
using Ferretto.VW.MAS.DeviceManager.PowerEnable.Interfaces;
using Ferretto.VW.MAS.Utils.Messages;
using Microsoft.Extensions.Logging;

namespace Ferretto.VW.MAS.DeviceManager.InverterReading
{
    internal class InverterReadingErrorState : StateBase
    {
        #region Fields

        private readonly IInverterReadingMachineData machineData;

        private readonly IInverterReadingStateData stateData;

        #endregion

        #region Constructors

        public InverterReadingErrorState(IInverterReadingStateData stateData, ILogger logger)
            : base(stateData.ParentMachine, logger)
        {
            this.stateData = stateData;
            this.machineData = stateData.MachineData as IInverterReadingMachineData;
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
                new InverterReadingMessageData(this.machineData.InverterParametersData),
                $"Inverter Reading  stopped due to an error. Filed message: {this.stateData.FieldMessage?.Description ?? string.Empty}",
                MessageActor.DeviceManager,
                MessageActor.DeviceManager,
                MessageType.InverterReading,
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
