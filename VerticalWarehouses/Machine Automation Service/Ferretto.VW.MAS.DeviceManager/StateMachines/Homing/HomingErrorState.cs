using Ferretto.VW.CommonUtils;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DeviceManager.Homing.Interfaces;
using Ferretto.VW.MAS.Utils.Enumerations;
using Ferretto.VW.MAS.Utils.Messages;
using Microsoft.Extensions.Logging;

namespace Ferretto.VW.MAS.DeviceManager.Homing
{
    internal class HomingErrorState : StateBase
    {
        #region Fields

        private readonly IHomingMachineData machineData;

        private readonly IHomingStateData stateData;

        #endregion

        #region Constructors

        public HomingErrorState(IHomingStateData stateData, ILogger logger)
            : base(stateData.ParentMachine, logger)
        {
            this.stateData = stateData;
            this.machineData = stateData.MachineData as IHomingMachineData;
        }

        #endregion

        #region Methods

        public override void ProcessCommandMessage(CommandMessage message)
        {
            // do nothing
        }

        public override void ProcessFieldNotificationMessage(FieldNotificationMessage message)
        {
            this.Logger.LogTrace($"1:Process NotificationMessage {message.Type} Source {message.Source} Status {message.Status}");

            if (message.Type == FieldMessageType.InverterPowerOff && message.Status != MessageStatus.OperationStart)
            {
                var notificationMessageData = new HomingMessageData(this.machineData.RequestedAxisToCalibrate, this.machineData.CalibrationType, this.machineData.LoadingUnitId, false, MessageVerbosity.Error);
                var notificationMessage = new NotificationMessage(
                    notificationMessageData,
                    "Homing Stopped due to an error",
                    MessageActor.DeviceManager,
                    MessageActor.DeviceManager,
                    MessageType.Homing,
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

        /// <inheritdoc/>
        public override void Start()
        {
            var currentInverterIndex = this.machineData.CurrentInverterIndex;
            this.Logger.LogDebug($"Start {this.GetType().Name} Inverter {currentInverterIndex} show errors {this.machineData.ShowErrors}");

            if (this.machineData.ShowErrors)
            {
                var stopMessage = new FieldCommandMessage(
                null,
                $"Reset Inverter Axis {this.machineData.AxisToCalibrate}",
                FieldMessageActor.InverterDriver,
                FieldMessageActor.DeviceManager,
                FieldMessageType.InverterStop,
                (byte)currentInverterIndex);

                this.ParentStateMachine.PublishFieldCommandMessage(stopMessage);
            }

            var notificationMessageData = new HomingMessageData(this.machineData.RequestedAxisToCalibrate, this.machineData.CalibrationType, this.machineData.LoadingUnitId, false, MessageVerbosity.Info);
            var notificationMessage = new NotificationMessage(
                                notificationMessageData,
                                "Homing Error",
                                MessageActor.DeviceManager,
                                MessageActor.DeviceManager,
                                MessageType.Homing,
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
