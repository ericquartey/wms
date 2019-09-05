using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.FiniteStateMachines.Positioning.Interfaces;
using Ferretto.VW.MAS.Utils.Enumerations;
using Ferretto.VW.MAS.Utils.Messages;
using Ferretto.VW.MAS.Utils.Messages.FieldData;
using Microsoft.Extensions.Logging;
// ReSharper disable ArrangeThisQualifier

namespace Ferretto.VW.MAS.FiniteStateMachines.Positioning
{
    public class PositioningEndState : StateBase
    {

        #region Fields

        private readonly IPositioningMachineData machineData;

        private readonly IPositioningStateData stateData;

        private bool disposed;

        #endregion

        #region Constructors

        public PositioningEndState(IPositioningStateData stateData)
            : base(stateData.ParentMachine, stateData.MachineData.RequestingBay, stateData.MachineData.Logger)
        {
            this.stateData = stateData;
            this.machineData = stateData.MachineData as IPositioningMachineData;
        }

        #endregion

        #region Destructors

        ~PositioningEndState()
        {
            this.Dispose(false);
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

            switch (message.Type)
            {
                case FieldMessageType.InverterStop:
                    switch (message.Status)
                    {
                        case MessageStatus.OperationEnd:
                            var inverterDataMessage = new InverterSetTimerFieldMessageData(InverterTimer.AxisPosition, false, 0);
                            var inverterMessage = new FieldCommandMessage(
                                inverterDataMessage,
                                "Update Inverter axis position status",
                                FieldMessageActor.InverterDriver,
                                FieldMessageActor.FiniteStateMachines,
                                FieldMessageType.InverterSetTimer,
                                (byte)InverterIndex.MainInverter);
                            this.Logger.LogTrace($"2:Publishing Field Command Message {inverterMessage.Type} Destination {inverterMessage.Destination}");

                            this.ParentStateMachine.PublishFieldCommandMessage(inverterMessage);

                            var notificationMessage = new NotificationMessage(
                               this.machineData.MessageData,
                               $"{this.machineData.MessageData.MovementMode} Positioning Ended",
                               MessageActor.Any,
                               MessageActor.FiniteStateMachines,
                               MessageType.Positioning,
                               this.RequestingBay,
                               MessageStatus.OperationStop);

                            this.ParentStateMachine.PublishNotificationMessage(notificationMessage);
                            break;

                        case MessageStatus.OperationError:
                            this.ParentStateMachine.ChangeState(new PositioningErrorState(this.stateData));
                            break;
                    }
                    break;
            }
        }

        public override void ProcessNotificationMessage(NotificationMessage message)
        {
            this.Logger.LogTrace($"1:Process Notification Message {message.Type} Source {message.Source} Status {message.Status}");
        }

        public override void Start()
        {
            this.Logger?.LogTrace("1:Method Start");

            lock (this.machineData.MachineSensorStatus)
            {
                this.machineData.MessageData.CurrentPosition = (this.machineData.MessageData.AxisMovement == Axis.Vertical) ? this.machineData.MachineSensorStatus.AxisYPosition : this.machineData.MachineSensorStatus.AxisXPosition;
            }

            if (this.stateData.StopRequestReason)
            {
                var stopMessage = new FieldCommandMessage(
                    null,
                    $"{this.machineData.MessageData.MovementMode} Positioning Stopped",
                    FieldMessageActor.InverterDriver,
                    FieldMessageActor.FiniteStateMachines,
                    FieldMessageType.InverterStop,
                    (byte)this.machineData.CurrentInverterIndex);

                this.ParentStateMachine.PublishFieldCommandMessage(stopMessage);

                var notificationMessage = new NotificationMessage(
                    this.machineData.MessageData,
                    $"{this.machineData.MessageData.MovementMode} Positioning Stopped",
                    MessageActor.Any,
                    MessageActor.FiniteStateMachines,
                    MessageType.Positioning,
                    this.RequestingBay,
                    MessageStatus.OperationStop);

                this.ParentStateMachine.PublishNotificationMessage(notificationMessage);
            }
            else
            {
                var notificationMessage = new NotificationMessage(
                    this.machineData.MessageData,
                    $"{this.machineData.MessageData.MovementMode} Positioning Completed",
                    MessageActor.Any,
                    MessageActor.FiniteStateMachines,
                    MessageType.Positioning,
                    this.RequestingBay,
                    MessageStatus.OperationEnd);

                this.Logger.LogDebug("FSM Positioning End");

                this.ParentStateMachine.PublishNotificationMessage(notificationMessage);
            }

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
        }

        public override void Stop(StopRequestReason reason = StopRequestReason.Stop)
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
