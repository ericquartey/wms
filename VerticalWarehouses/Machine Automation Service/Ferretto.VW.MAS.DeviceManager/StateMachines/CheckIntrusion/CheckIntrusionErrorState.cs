using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DeviceManager.CheckIntrusion.Interfaces;
using Ferretto.VW.MAS.InverterDriver.Contracts;
using Ferretto.VW.MAS.Utils.Messages;
using Ferretto.VW.MAS.Utils.Enumerations;
using Microsoft.Extensions.Logging;

namespace Ferretto.VW.MAS.DeviceManager.CheckIntrusion
{
    internal class CheckIntrusionErrorState : StateBase
    {
        #region Fields

        private readonly ICheckIntrusionMachineData machineData;

        private readonly ICheckIntrusionStateData stateData;

        #endregion

        #region Constructors

        public CheckIntrusionErrorState(ICheckIntrusionStateData stateData, ILogger logger)
            : base(stateData.ParentMachine, logger)
        {
            this.stateData = stateData;
            this.machineData = stateData.MachineData as ICheckIntrusionMachineData;
        }

        #endregion

        #region Methods

        public override void ProcessCommandMessage(CommandMessage message)
        {
            // do nothing
        }

        public override void ProcessFieldNotificationMessage(FieldNotificationMessage message)
        {
            if (message.Type == FieldMessageType.InverterStop && message.Status == MessageStatus.OperationError)
            {
                var notificationMessage = new NotificationMessage(
                    null,
                    $"Check Intrusion Error Detected",
                    MessageActor.DeviceManager,
                    MessageActor.DeviceManager,
                    MessageType.CheckIntrusion,
                    this.machineData.RequestingBay,
                    this.machineData.TargetBay,
                    MessageStatus.OperationError,
                    ErrorLevel.Error);

                this.ParentStateMachine.PublishNotificationMessage(notificationMessage);
            }
        }

        /// <inheritdoc/>
        public override void ProcessNotificationMessage(NotificationMessage message)
        {
            // do nothing
        }

        public override void Start()
        {
            this.Logger.LogDebug($"Error {this.GetType().Name}");

            var stopMessage = new FieldCommandMessage(
                null,
                $"Stop Main Inverter",
                FieldMessageActor.InverterDriver,
                FieldMessageActor.DeviceManager,
                FieldMessageType.InverterStop,
                (byte)InverterIndex.MainInverter);

            this.Logger.LogDebug($"1:Publish Field Command Message processed: {stopMessage.Type}, {stopMessage.Destination}");

            this.ParentStateMachine.PublishFieldCommandMessage(stopMessage);

            var notificationMessage = new NotificationMessage(
                null,
                $"Check intrusion failed on bay {this.machineData.TargetBay}. Filed message: {this.stateData?.FieldMessage?.Description}",
                MessageActor.DeviceManager,
                MessageActor.DeviceManager,
                MessageType.CheckIntrusion,
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
