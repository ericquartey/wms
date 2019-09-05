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
    public class PositioningStartState : StateBase
    {

        #region Fields

        private readonly IPositioningMachineData machineData;

        private readonly IPositioningStateData stateData;

        private bool disposed;

        private bool inverterSwitched;

        private bool ioSwitched;

        #endregion

        #region Constructors

        public PositioningStartState(IPositioningStateData stateData)
            : base(stateData.ParentMachine, stateData.MachineData.RequestingBay, stateData.MachineData.Logger)
        {
            this.stateData = stateData;
            this.machineData = stateData.MachineData as IPositioningMachineData;
        }

        #endregion

        #region Destructors

        ~PositioningStartState()
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
            this.Logger.LogTrace($"1:Process Field Notification Message {message.Type} Source {message.Source} Status {message.Status}");

            if (message.Type == FieldMessageType.SwitchAxis)
            {
                switch (message.Status)
                {
                    case MessageStatus.OperationEnd:
                        this.Logger.LogDebug("I/O switch completed");
                        this.ioSwitched = true;
                        break;

                    case MessageStatus.OperationError:
                        this.stateData.FieldMessage = message;
                        this.ParentStateMachine.ChangeState(new PositioningErrorState(this.stateData));
                        break;
                }
            }

            if (message.Type == FieldMessageType.InverterSwitchOn)
            {
                switch (message.Status)
                {
                    case MessageStatus.OperationEnd:
                        this.inverterSwitched = true;
                        this.Logger.LogDebug("Inverter switch ON completed");
                        break;

                    case MessageStatus.OperationError:
                        this.stateData.FieldMessage = message;
                        this.ParentStateMachine.ChangeState(new PositioningErrorState(this.stateData));
                        break;
                }
            }

            if (this.ioSwitched && this.inverterSwitched)
            {
                this.ParentStateMachine.ChangeState(new PositioningExecutingState(this.stateData));
            }
        }

        public override void ProcessNotificationMessage(NotificationMessage message)
        {
            this.Logger.LogTrace($"1:Process Notification Message {message.Type} Source {message.Source} Status {message.Status}");
        }

        public override void Start()
        {
            switch (this.machineData.MessageData.MovementMode)
            {
                case MovementMode.FindZero:
                case MovementMode.Profile:
                case MovementMode.Position:
                    switch (this.machineData.CurrentInverterIndex)
                    {
                        case InverterIndex.MainInverter:
                            var ioCommandMessageData = new SwitchAxisFieldMessageData(this.machineData.MessageData.AxisMovement);
                            var ioCommandMessage = new FieldCommandMessage(
                                ioCommandMessageData,
                                $"Switch Axis {this.machineData.MessageData.AxisMovement}",
                                FieldMessageActor.IoDriver,
                                FieldMessageActor.FiniteStateMachines,
                                FieldMessageType.SwitchAxis,
                                (byte)IoIndex.IoDevice1);

                            this.Logger.LogDebug($"1:Publishing Field Command Message {ioCommandMessage.Type} Destination {ioCommandMessage.Destination}");

                            this.ParentStateMachine.PublishFieldCommandMessage(ioCommandMessage);

                            break;

                        case InverterIndex.Slave1:
                            this.ioSwitched = true;
                            break;
                    }
                    break;
            }

            var inverterCommandMessageData = new InverterSwitchOnFieldMessageData(this.machineData.MessageData.AxisMovement);
            var inverterCommandMessage = new FieldCommandMessage(
                inverterCommandMessageData,
                $"Switch Axis {this.machineData.MessageData.AxisMovement}",
                FieldMessageActor.InverterDriver,
                FieldMessageActor.FiniteStateMachines,
                FieldMessageType.InverterSwitchOn,
                (byte)this.machineData.CurrentInverterIndex);

            this.Logger.LogDebug($"5:Publishing Field Command Message {inverterCommandMessage.Type} Destination {inverterCommandMessage.Destination}");

            this.ParentStateMachine.PublishFieldCommandMessage(inverterCommandMessage);

            var inverterDataMessage = new InverterSetTimerFieldMessageData(InverterTimer.StatusWord, false, 0);
            var inverterMessage = new FieldCommandMessage(
                inverterDataMessage,
                "Update Inverter status word status",
                FieldMessageActor.InverterDriver,
                FieldMessageActor.FiniteStateMachines,
                FieldMessageType.InverterSetTimer,
                (byte)InverterIndex.MainInverter);

            this.Logger.LogTrace($"4:Publishing Field Command Message {inverterMessage.Type} Destination {inverterMessage.Destination}");

            this.ParentStateMachine.PublishFieldCommandMessage(inverterMessage);

            this.machineData.MessageData.CurrentPosition = (this.machineData.MessageData.AxisMovement == Axis.Vertical) ? this.machineData.MachineSensorStatus.AxisYPosition : this.machineData.MachineSensorStatus.AxisXPosition;

            this.machineData.MessageData.ExecutedCycles = 0;

            var notificationMessage = new NotificationMessage(
                this.machineData.MessageData,
                $"{this.machineData.MessageData.MovementMode} Positioning Started",
                MessageActor.Any,
                MessageActor.FiniteStateMachines,
                MessageType.Positioning,
                this.machineData.RequestingBay,
                MessageStatus.OperationStart);

            this.Logger.LogTrace($"6:Publishing Automation Notification Message {notificationMessage.Type} Destination {notificationMessage.Destination} Status {notificationMessage.Status}");

            this.ParentStateMachine.PublishNotificationMessage(notificationMessage);
        }

        public override void Stop(StopRequestReason reason = StopRequestReason.Stop)
        {
            this.Logger.LogTrace("1:Method Start");

            this.stateData.StopRequestReason = true;
            this.ParentStateMachine.ChangeState(new PositioningEndState(this.stateData));
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
