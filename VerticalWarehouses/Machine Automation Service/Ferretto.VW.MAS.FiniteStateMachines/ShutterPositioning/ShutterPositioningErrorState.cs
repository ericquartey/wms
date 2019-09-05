using System;
using Ferretto.VW.CommonUtils.Enumerations;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.FiniteStateMachines.Interface;
using Ferretto.VW.MAS.FiniteStateMachines.ShutterPositioning.Interfaces;
using Ferretto.VW.MAS.InverterDriver.InverterStatus;
using Ferretto.VW.MAS.Utils.Enumerations;
using Ferretto.VW.MAS.Utils.Messages;
using Ferretto.VW.MAS.Utils.Messages.FieldData;
using Microsoft.Extensions.Logging;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS.FiniteStateMachines.ShutterPositioning
{
    public class ShutterPositioningErrorState : StateBase
    {

        #region Fields

        private readonly FieldNotificationMessage errorMessage;

        private readonly IShutterPositioningStateMachineData shutterPositioningStateMachineData;

        private bool disposed;

        #endregion

        #region Constructors

        public ShutterPositioningErrorState(
            IStateMachine parentMachine,
            IShutterPositioningStateMachineData shutterPositioningStateMachineData,
            FieldNotificationMessage errorMessage)
            : base(parentMachine, shutterPositioningStateMachineData.Logger)
        {
            this.shutterPositioningStateMachineData = shutterPositioningStateMachineData;
            this.errorMessage = errorMessage;
        }

        #endregion

        #region Destructors

        ~ShutterPositioningErrorState()
        {
            this.Dispose(false);
        }

        #endregion



        #region Methods

        /// <inheritdoc/>
        public override void ProcessCommandMessage(CommandMessage message)
        {
            this.Logger.LogTrace($"1:Process Command Message {message.Type} Source {message.Source}");
        }

        public override void ProcessFieldNotificationMessage(FieldNotificationMessage message)
        {
            this.Logger.LogTrace($"1:Process NotificationMessage {message.Type} Source {message.Source} Status {message.Status}");

            if (message.Type == FieldMessageType.InverterStop && message.Status != MessageStatus.OperationStart)
            {
                var notificationMessageData = new ShutterPositioningMessageData(this.shutterPositioningStateMachineData.PositioningMessageData);
                var inverterStatus = new AglInverterStatus(message.DeviceIndex);
                int sensorStart = (int)(IOMachineSensors.PowerOnOff + message.DeviceIndex * inverterStatus.aglInverterInputs.Length);
                Array.Copy(this.shutterPositioningStateMachineData.MachineSensorsStatus.DisplayedInputs, sensorStart, inverterStatus.aglInverterInputs, 0, inverterStatus.aglInverterInputs.Length);
                notificationMessageData.ShutterPosition = inverterStatus.CurrentShutterPosition;

                var notificationMessage = new NotificationMessage(
                    notificationMessageData,
                    "Shutter Positioning Stopped for an error",
                    MessageActor.Any,
                    MessageActor.FiniteStateMachines,
                    MessageType.ShutterPositioning,
                    this.shutterPositioningStateMachineData.RequestingBay,
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

        public override void Start()
        {
            var stopMessage = new FieldCommandMessage(
                null,
                "Reset Inverter ShutterPositioning",
                FieldMessageActor.InverterDriver,
                FieldMessageActor.FiniteStateMachines,
                FieldMessageType.InverterStop,
                (byte)this.shutterPositioningStateMachineData.InverterIndex);

            this.Logger.LogTrace($"1:Publish Field Command Message processed: {stopMessage.Type}, {stopMessage.Destination}");

            this.ParentStateMachine.PublishFieldCommandMessage(stopMessage);

            var inverterDataMessage = new InverterSetTimerFieldMessageData(InverterTimer.SensorStatus, true, 500);
            var inverterMessage = new FieldCommandMessage(
                inverterDataMessage,
                "Update Inverter digital input status",
                FieldMessageActor.InverterDriver,
                FieldMessageActor.FiniteStateMachines,
                FieldMessageType.InverterSetTimer,
                (byte)InverterIndex.MainInverter);

            this.Logger.LogTrace($"2:Publishing Field Command Message {inverterMessage.Type} Destination {inverterMessage.Destination}");

            this.ParentStateMachine.PublishFieldCommandMessage(inverterMessage);

            inverterDataMessage = new InverterSetTimerFieldMessageData(InverterTimer.AxisPosition, false, 0);
            inverterMessage = new FieldCommandMessage(
                inverterDataMessage,
                "Update Inverter axis position status",
                FieldMessageActor.InverterDriver,
                FieldMessageActor.FiniteStateMachines,
                FieldMessageType.InverterSetTimer,
                (byte)InverterIndex.MainInverter);
            this.Logger.LogTrace($"3:Publishing Field Command Message {inverterMessage.Type} Destination {inverterMessage.Destination}");

            this.ParentStateMachine.PublishFieldCommandMessage(inverterMessage);
        }

        public override void Stop(StopRequestReason reason = StopRequestReason.Stop)
        {
            this.Logger.LogTrace("1:Method Start");

            this.ParentStateMachine.ChangeState(new ShutterPositioningEndState(this.ParentStateMachine, this.shutterPositioningStateMachineData, true));
        }

        protected override void Dispose(bool disposing)
        {
            if (this.disposed)
            {
                return;
            }

            this.disposed = true;
            base.Dispose(disposing);
        }

        #endregion
    }
}
