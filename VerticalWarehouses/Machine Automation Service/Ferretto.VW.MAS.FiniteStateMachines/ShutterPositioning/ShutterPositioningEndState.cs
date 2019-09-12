using System;
using Ferretto.VW.CommonUtils.Enumerations;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;
using Ferretto.VW.MAS.InverterDriver.InverterStatus;
using Ferretto.VW.MAS.Utils.Enumerations;
using Ferretto.VW.MAS.Utils.Messages;
using Ferretto.VW.MAS.Utils.Messages.FieldData;
using Microsoft.Extensions.Logging;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS.FiniteStateMachines.ShutterPositioning
{
    internal class ShutterPositioningEndState : StateBase
    {
        #region Fields

        private readonly InverterIndex inverterIndex;

        private readonly IMachineSensorsStatus machineSensorsStatus;

        private readonly IShutterPositioningMessageData shutterPositioningMessageData;

        private readonly bool stopRequested;

        private bool disposed;

        #endregion

        #region Constructors

        public ShutterPositioningEndState(
            IStateMachine parentMachine,
            IShutterPositioningMessageData shutterPositioningMessageData,
            InverterIndex inverterIndex,
            IMachineSensorsStatus machineSensorsStatus,
            ILogger logger,
            bool stopRequested = false)
            : base(parentMachine, logger)
        {
            this.shutterPositioningMessageData = shutterPositioningMessageData;
            this.inverterIndex = inverterIndex;
            this.machineSensorsStatus = machineSensorsStatus;
            this.stopRequested = stopRequested;
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
                            var inverterStatus = new AglInverterStatus((byte)this.inverterIndex);
                            int sensorStart = (int)(IOMachineSensors.PowerOnOff + (int)this.inverterIndex * inverterStatus.aglInverterInputs.Length);
                            Array.Copy(this.machineSensorsStatus.DisplayedInputs, sensorStart, inverterStatus.aglInverterInputs, 0, inverterStatus.aglInverterInputs.Length);
                            this.shutterPositioningMessageData.ShutterPosition = inverterStatus.CurrentShutterPosition;

                            var notificationMessage = new NotificationMessage(
                               this.shutterPositioningMessageData,
                               "ShutterPositioning Complete",
                               MessageActor.Any,
                               MessageActor.FiniteStateMachines,
                               MessageType.ShutterPositioning,
                               MessageStatus.OperationStop);

                            this.ParentStateMachine.PublishNotificationMessage(notificationMessage);
                            break;

                        case MessageStatus.OperationError:
                            this.ParentStateMachine.ChangeState(new ShutterPositioningErrorState(this.ParentStateMachine, this.shutterPositioningMessageData, this.inverterIndex, this.machineSensorsStatus, message, this.Logger));
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

            if (this.stopRequested)
            {
                var data = new InverterStopFieldMessageData();

                var stopMessage = new FieldCommandMessage(
                    data,
                    "Reset Inverter ShutterPositioning",
                    FieldMessageActor.InverterDriver,
                    FieldMessageActor.FiniteStateMachines,
                    FieldMessageType.InverterStop,
                    (byte)this.inverterIndex);

                this.ParentStateMachine.PublishFieldCommandMessage(stopMessage);
            }
            else
            {
                var inverterStatus = new AglInverterStatus((byte)this.inverterIndex);
                int sensorStart = (int)(IOMachineSensors.PowerOnOff + (int)this.inverterIndex * inverterStatus.aglInverterInputs.Length);
                Array.Copy(this.machineSensorsStatus.DisplayedInputs, sensorStart, inverterStatus.aglInverterInputs, 0, inverterStatus.aglInverterInputs.Length);
                this.shutterPositioningMessageData.ShutterPosition = inverterStatus.CurrentShutterPosition;

                var notificationMessage = new NotificationMessage(
                    this.shutterPositioningMessageData,
                    "ShutterPositioning Completed",
                    MessageActor.Any,
                    MessageActor.FiniteStateMachines,
                    MessageType.ShutterPositioning,
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

        public override void Stop()
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
