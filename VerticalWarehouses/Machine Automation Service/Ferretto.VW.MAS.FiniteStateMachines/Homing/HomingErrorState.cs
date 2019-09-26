using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.FiniteStateMachines.Homing.Interfaces;
using Ferretto.VW.MAS.InverterDriver.Contracts;
using Ferretto.VW.MAS.Utils.Enumerations;
using Ferretto.VW.MAS.Utils.Messages;
using Ferretto.VW.MAS.Utils.Messages.FieldData;
using Microsoft.Extensions.Logging;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS.FiniteStateMachines.Homing
{
    internal class HomingErrorState : StateBase
    {
        #region Fields

        private readonly IHomingMachineData machineData;

        private readonly IHomingStateData stateData;

        #endregion

        #region Constructors

        public HomingErrorState(IHomingStateData stateData)
            : base(stateData.ParentMachine, stateData.MachineData.Logger)
        {
            this.stateData = stateData;
            this.machineData = stateData.MachineData as IHomingMachineData;
        }

        #endregion

        #region Methods

        public override void ProcessCommandMessage(CommandMessage message)
        {
            this.Logger.LogTrace($"1:Process Command Message {message.Type} Source {message.Source}");
        }

        public override void ProcessFieldNotificationMessage(FieldNotificationMessage message)
        {
            this.Logger.LogTrace($"1:Process NotificationMessage {message.Type} Source {message.Source} Status {message.Status}");

            if (message.Type == FieldMessageType.InverterPowerOff && message.Status != MessageStatus.OperationStart)
            {
                var notificationMessageData = new HomingMessageData(this.machineData.AxisToCalibrate, MessageVerbosity.Error);
                var notificationMessage = new NotificationMessage(
                    notificationMessageData,
                    "Homing Stopped due to an error",
                    MessageActor.FiniteStateMachines,
                    MessageActor.FiniteStateMachines,
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
            this.Logger.LogTrace($"1:Process Notification Message {message.Type} Source {message.Source} Status {message.Status}");
        }

        /// <inheritdoc/>
        public override void Start()
        {
            var inverterDataMessage = new InverterSetTimerFieldMessageData(InverterTimer.AxisPosition, false, 0);
            var inverterMessage = new FieldCommandMessage(
                inverterDataMessage,
                "Update Inverter axis position status",
                FieldMessageActor.InverterDriver,
                FieldMessageActor.FiniteStateMachines,
                FieldMessageType.InverterSetTimer,
                (byte)InverterIndex.MainInverter);

            this.ParentStateMachine.PublishFieldCommandMessage(inverterMessage);

            if (this.machineData.IsOneKMachine)
            {
                inverterMessage = new FieldCommandMessage(
                    inverterDataMessage,
                    "Update Inverter axis position status",
                    FieldMessageActor.InverterDriver,
                    FieldMessageActor.FiniteStateMachines,
                    FieldMessageType.InverterSetTimer,
                    (byte)InverterIndex.Slave1);

                this.ParentStateMachine.PublishFieldCommandMessage(inverterMessage);
            }

            var currentInverterIndex = (this.machineData.IsOneKMachine && this.machineData.AxisToCalibrate == Axis.Horizontal) ? InverterIndex.Slave1 : InverterIndex.MainInverter;

            var stopMessage = new FieldCommandMessage(
                null,
                $"Reset Inverter Axis {this.machineData.AxisToCalibrate}",
                FieldMessageActor.InverterDriver,
                FieldMessageActor.FiniteStateMachines,
                FieldMessageType.InverterStop,
                (byte)currentInverterIndex);

            this.ParentStateMachine.PublishFieldCommandMessage(stopMessage);

            var notificationMessageData = new HomingMessageData(this.machineData.AxisToCalibrate, MessageVerbosity.Info);
            var notificationMessage = new NotificationMessage(
                                notificationMessageData,
                                "Homing Error",
                                MessageActor.FiniteStateMachines,
                                MessageActor.FiniteStateMachines,
                                MessageType.Homing,
                                this.machineData.RequestingBay,
                                this.machineData.TargetBay,
                                MessageStatus.OperationError,
                                ErrorLevel.Error);

            this.ParentStateMachine.PublishNotificationMessage(notificationMessage);
        }

        public override void Stop(StopRequestReason reason)
        {
            this.Logger.LogDebug("1:Stop Method Empty");
        }

        #endregion
    }
}
