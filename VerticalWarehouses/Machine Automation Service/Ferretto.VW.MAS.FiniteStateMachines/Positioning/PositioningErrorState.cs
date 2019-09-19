using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;
using Ferretto.VW.MAS.InverterDriver.Contracts;
using Ferretto.VW.MAS.Utils.Enumerations;
using Ferretto.VW.MAS.Utils.Messages;
using Ferretto.VW.MAS.Utils.Messages.FieldData;
using Microsoft.Extensions.Logging;
// ReSharper disable ArrangeThisQualifier

namespace Ferretto.VW.MAS.FiniteStateMachines.Positioning
{
    internal class PositioningErrorState : StateBase
    {
        #region Fields

        private readonly FieldNotificationMessage errorMessage;

        private readonly IMachineSensorsStatus machineSensorsStatus;

        private readonly IPositioningMessageData positioningMessageData;

        #endregion

        #region Constructors

        public PositioningErrorState(
            IStateMachine parentMachine,
            IMachineSensorsStatus machineSensorsStatus,
            IPositioningMessageData positioningMessageData,
            FieldNotificationMessage errorMessage,
            ILogger logger)
            : base(parentMachine, logger)
        {
            this.positioningMessageData = positioningMessageData;
            this.machineSensorsStatus = machineSensorsStatus;
            this.errorMessage = errorMessage;
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

            if (message.Type == FieldMessageType.InverterStop && message.Status == MessageStatus.OperationError)
            {
                var notificationMessageData = new PositioningMessageData(
                    this.positioningMessageData.AxisMovement,
                    this.positioningMessageData.MovementType,
                    this.positioningMessageData.MovementMode,
                    this.positioningMessageData.TargetPosition,
                    this.positioningMessageData.TargetSpeed,
                    this.positioningMessageData.TargetAcceleration,
                    this.positioningMessageData.TargetDeceleration,
                    0,
                    this.positioningMessageData.LowerBound,
                    this.positioningMessageData.UpperBound,
                    0,
                    this.positioningMessageData.SwitchPosition,
                    this.positioningMessageData.Direction,
                    MessageVerbosity.Error);

                var notificationMessage = new NotificationMessage(
                    notificationMessageData,
                    this.positioningMessageData.NumberCycles == 0 ? "Positioning Stopped due to an error" : "Belt Burnishing Stopped due to an error",
                    MessageActor.Any,
                    MessageActor.FiniteStateMachines,
                    MessageType.Positioning,
                    MessageStatus.OperationError,
                    ErrorLevel.Error);

                this.ParentStateMachine.PublishNotificationMessage(notificationMessage);
            }
        }

        public override void ProcessNotificationMessage(NotificationMessage message)
        {
            this.Logger.LogTrace($"1:Process Notification Message {message.Type} Source {message.Source} Status {message.Status}");
        }

        public override void Start()
        {
            var inverterIndex = (this.positioningMessageData.IsOneKMachine && this.positioningMessageData.AxisMovement == Axis.Horizontal) ? InverterIndex.Slave1 : InverterIndex.MainInverter;
            var description = this.positioningMessageData.NumberCycles == 0 ? $"Reset Inverter Axis {this.positioningMessageData.AxisMovement}" : $"Reset Inverter Belt Burninshing";
            var stopMessage = new FieldCommandMessage(
                null,
                    description,
                    FieldMessageActor.InverterDriver,
                    FieldMessageActor.FiniteStateMachines,
                    FieldMessageType.InverterStop,
                (byte)inverterIndex);

            this.Logger.LogTrace($"1:Publish Field Command Message processed: {stopMessage.Type}, {stopMessage.Destination}");

            this.ParentStateMachine.PublishFieldCommandMessage(stopMessage);

            var inverterDataMessage = new InverterSetTimerFieldMessageData(InverterTimer.SensorStatus, true, SENSOR_UPDATE_SLOW);
            var inverterMessage = new FieldCommandMessage(
                inverterDataMessage,
                "Update Inverter digital input status",
                FieldMessageActor.InverterDriver,
                FieldMessageActor.FiniteStateMachines,
                FieldMessageType.InverterSetTimer,
                (byte)InverterIndex.MainInverter);

            this.Logger.LogTrace($"2:Publishing Field Command Message {inverterMessage.Type} Destination {inverterMessage.Destination}");

            this.ParentStateMachine.PublishFieldCommandMessage(inverterMessage);

            inverterDataMessage = new InverterSetTimerFieldMessageData(InverterTimer.StatusWord, false, 0);
            inverterMessage = new FieldCommandMessage(
                inverterDataMessage,
                "Update Inverter status word status",
                FieldMessageActor.InverterDriver,
                FieldMessageActor.FiniteStateMachines,
                FieldMessageType.InverterSetTimer,
                (byte)inverterIndex);
            this.Logger.LogTrace($"4:Publishing Field Command Message {inverterMessage.Type} Destination {inverterMessage.Destination}");

            this.ParentStateMachine.PublishFieldCommandMessage(inverterMessage);

            inverterDataMessage = new InverterSetTimerFieldMessageData(InverterTimer.AxisPosition, false, 0);
            inverterMessage = new FieldCommandMessage(
                inverterDataMessage,
                "Update Inverter axis position status",
                FieldMessageActor.InverterDriver,
                FieldMessageActor.FiniteStateMachines,
                FieldMessageType.InverterSetTimer,
                (byte)inverterIndex);
            this.Logger.LogTrace($"3:Publishing Field Command Message {inverterMessage.Type} Destination {inverterMessage.Destination}");

            this.ParentStateMachine.PublishFieldCommandMessage(inverterMessage);

            var notificationMessageData = new PositioningMessageData(
                this.positioningMessageData.AxisMovement,
                this.positioningMessageData.MovementType,
                this.positioningMessageData.MovementMode,
                this.positioningMessageData.TargetPosition,
                this.positioningMessageData.TargetSpeed,
                this.positioningMessageData.TargetAcceleration,
                this.positioningMessageData.TargetDeceleration,
                0,
                this.positioningMessageData.LowerBound,
                this.positioningMessageData.UpperBound,
                0,
                this.positioningMessageData.SwitchPosition,
                this.positioningMessageData.Direction,
                MessageVerbosity.Info);

            var notificationMessage = new NotificationMessage(
                                    notificationMessageData,
                                    this.positioningMessageData.NumberCycles == 0 ? "Positioning Error" : "Belt Burnishing Error",
                                    MessageActor.Any,
                                    MessageActor.FiniteStateMachines,
                                    MessageType.Positioning,
                                    MessageStatus.OperationError);

            this.ParentStateMachine.PublishNotificationMessage(notificationMessage);
        }

        public override void Stop()
        {
            this.Logger.LogTrace("1:Method Start");
        }

        #endregion
    }
}
