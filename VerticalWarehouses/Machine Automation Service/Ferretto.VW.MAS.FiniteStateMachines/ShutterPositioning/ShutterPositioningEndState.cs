using System;
using Ferretto.VW.CommonUtils.Enumerations;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.FiniteStateMachines.ShutterPositioning.Interfaces;
using Ferretto.VW.MAS.InverterDriver.InverterStatus;
using Ferretto.VW.MAS.Utils.Enumerations;
using Ferretto.VW.MAS.Utils.Messages;
using Ferretto.VW.MAS.Utils.Messages.FieldData;
using Microsoft.Extensions.Logging;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS.FiniteStateMachines.ShutterPositioning
{
    public class ShutterPositioningEndState : StateBase
    {

        #region Fields

        private readonly IShutterPositioningMachineData machineData;

        private readonly IShutterPositioningStateData stateData;

        private bool disposed;

        #endregion

        #region Constructors

        public ShutterPositioningEndState(IShutterPositioningStateData stateData)
            : base(stateData.ParentMachine, stateData.MachineData.RequestingBay, stateData.MachineData.Logger)
        {
            this.stateData = stateData;
            this.machineData = stateData.MachineData as IShutterPositioningMachineData;
        }

        #endregion

        #region Destructors

        ~ShutterPositioningEndState()
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

            switch (message.Type)
            {
                case FieldMessageType.InverterStop:
                    //case FieldMessageType.InverterPowerOff:
                    switch (message.Status)
                    {
                        case MessageStatus.OperationEnd:
                            var notificationMessageData = new ShutterPositioningMessageData(this.machineData.PositioningMessageData);
                            var inverterStatus = new AglInverterStatus(message.DeviceIndex);
                            int sensorStart = (int)(IOMachineSensors.PowerOnOff + message.DeviceIndex * inverterStatus.aglInverterInputs.Length);
                            Array.Copy(this.machineData.MachineSensorsStatus.DisplayedInputs, sensorStart, inverterStatus.aglInverterInputs, 0, inverterStatus.aglInverterInputs.Length);
                            notificationMessageData.ShutterPosition = inverterStatus.CurrentShutterPosition;

                            var notificationMessage = new NotificationMessage(
                                notificationMessageData,
                               "ShutterPositioning Complete",
                               MessageActor.Any,
                               MessageActor.FiniteStateMachines,
                               MessageType.ShutterPositioning,
                                this.RequestingBay,
                                this.RequestingBay,
                               MessageStatus.OperationStop);

                            this.ParentStateMachine.PublishNotificationMessage(notificationMessage);
                            break;

                        case MessageStatus.OperationError:
                            this.stateData.FieldMessage = message;
                            this.ParentStateMachine.ChangeState(new ShutterPositioningErrorState(this.stateData));
                            break;
                    }
                    break;
            }
        }

        /// <inheritdoc/>
        public override void ProcessNotificationMessage(NotificationMessage message)
        {
            this.Logger.LogTrace($"1:Process Notification Message {message.Type} Source {message.Source} Status {message.Status}");
        }

        public override void Start()
        {
            this.Logger?.LogTrace("1:Method Start");

            if (this.stateData.StopRequestReason == StopRequestReason.Stop)
            {

                var stopMessage = new FieldCommandMessage(
                    null,
                    "Reset Inverter ShutterPositioning",
                    FieldMessageActor.InverterDriver,
                    FieldMessageActor.FiniteStateMachines,
                    FieldMessageType.InverterStop,
                    (byte)this.machineData.InverterIndex);

                this.ParentStateMachine.PublishFieldCommandMessage(stopMessage);
            }
            else
            {
                var notificationMessageData = new ShutterPositioningMessageData(this.machineData.PositioningMessageData);
                var inverterStatus = new AglInverterStatus((byte)this.machineData.InverterIndex);
                int sensorStart = (int)(IOMachineSensors.PowerOnOff + (int)this.machineData.InverterIndex * inverterStatus.aglInverterInputs.Length);
                Array.Copy(this.machineData.MachineSensorsStatus.DisplayedInputs, sensorStart, inverterStatus.aglInverterInputs, 0, inverterStatus.aglInverterInputs.Length);
                notificationMessageData.ShutterPosition = inverterStatus.CurrentShutterPosition;

                var notificationMessage = new NotificationMessage(
                    notificationMessageData,
                    "ShutterPositioning Completed",
                    MessageActor.Any,
                    MessageActor.FiniteStateMachines,
                    MessageType.ShutterPositioning,
                    this.RequestingBay,
                    this.RequestingBay,
                    MessageStatus.OperationEnd);

                this.ParentStateMachine.PublishNotificationMessage(notificationMessage);
            }

            var inverterDataMessage = new InverterSetTimerFieldMessageData(InverterTimer.SensorStatus, true, SENSOR_UPDATE_SLOW);
            var inverterMessage = new FieldCommandMessage(
                inverterDataMessage,
                "Update Inverter digital input status",
                FieldMessageActor.InverterDriver,
                FieldMessageActor.FiniteStateMachines,
                FieldMessageType.InverterSetTimer,
                (byte)InverterIndex.MainInverter);

            this.Logger.LogTrace($"1:Publishing Field Command Message {inverterMessage.Type} Destination {inverterMessage.Destination}");

            this.ParentStateMachine.PublishFieldCommandMessage(inverterMessage);

            inverterDataMessage = new InverterSetTimerFieldMessageData(InverterTimer.AxisPosition, false, 0);
            inverterMessage = new FieldCommandMessage(
                inverterDataMessage,
                "Update Inverter axis position status",
                FieldMessageActor.InverterDriver,
                FieldMessageActor.FiniteStateMachines,
                FieldMessageType.InverterSetTimer,
                (byte)InverterIndex.MainInverter);
            this.Logger.LogTrace($"2:Publishing Field Command Message {inverterMessage.Type} Destination {inverterMessage.Destination}");

            this.ParentStateMachine.PublishFieldCommandMessage(inverterMessage);
        }

        public override void Stop(StopRequestReason reason)
        {
            this.Logger.LogTrace("1:Method Start");
        }

        protected override void Dispose(bool disposing)
        {
            if (this.disposed)
            {
                return;
            }

            if (disposing)
            {
            }

            this.disposed = true;
            base.Dispose(disposing);
        }

        #endregion
    }
}
