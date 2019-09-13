using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.FiniteStateMachines.MoveDrawer.Interfaces;
using Ferretto.VW.MAS.Utils.Enumerations;
using Ferretto.VW.MAS.Utils.Messages;
using Ferretto.VW.MAS.Utils.Messages.FieldData;
using Microsoft.Extensions.Logging;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS.FiniteStateMachines.MoveDrawer
{
    public class MoveDrawerStartState : StateBase
    {

        #region Fields

        private readonly IMoveDrawerMachineData machineData;

        private readonly IMoveDrawerStateData stateData;

        private bool disposed;

        private bool inverterSwitched;

        private bool ioSwitched;

        #endregion

        #region Constructors

        public MoveDrawerStartState(IMoveDrawerStateData stateData)
            : base(stateData.ParentMachine, stateData.MachineData.RequestingBay, stateData.MachineData.Logger)
        {
            this.stateData = stateData;
            this.machineData = stateData.MachineData as IMoveDrawerMachineData;
        }

        #endregion

        #region Destructors

        ~MoveDrawerStartState()
        {
            this.Dispose(false);
        }

        #endregion



        #region Methods

        public override void ProcessCommandMessage(CommandMessage message)
        {
            this.Logger.LogTrace($"1:Process CommandMessage {message.Type} Source {message.Source}");
        }

        public override void ProcessFieldNotificationMessage(FieldNotificationMessage message)
        {
            this.Logger.LogTrace($"1:Process Notification Message {message.Type} Source {message.Source} Status {message.Status}");

            //TODO When IODriver and Inverter Driver report Axis Switch move to next state
            if (message.Type == FieldMessageType.SwitchAxis)
            {
                switch (message.Status)
                {
                    case MessageStatus.OperationEnd:
                        this.ioSwitched = true;
                        break;

                    case MessageStatus.OperationError:
                        this.stateData.FieldMessage = message;
                        this.ParentStateMachine.ChangeState(new MoveDrawerErrorState(this.stateData));
                        break;
                }
            }

            if (message.Type == FieldMessageType.InverterSwitchOn)
            {
                switch (message.Status)
                {
                    case MessageStatus.OperationEnd:
                        this.inverterSwitched = true;
                        break;

                    case MessageStatus.OperationError:
                        this.stateData.FieldMessage = message;
                        this.ParentStateMachine.ChangeState(new MoveDrawerErrorState(this.stateData));
                        break;
                }
            }

            if (this.ioSwitched && this.inverterSwitched)
            {
                this.machineData.DrawerOperationData.Step = DrawerOperationStep.None;

                this.ParentStateMachine.ChangeState(new MoveDrawerElevatorToPositionState(this.stateData));
            }
        }

        public override void ProcessNotificationMessage(NotificationMessage message)
        {
            this.Logger.LogTrace($"1:Process NotificationMessage {message.Type} Source {message.Source} Status {message.Status}");
        }

        public override void Start()
        {
            if (this.machineData.IsOneKMachine)
            {
                this.ioSwitched = true;
                var slaveInverterCommandMessageData = new InverterSwitchOnFieldMessageData(Axis.Horizontal);
                var slaveInverterCommandMessage = new FieldCommandMessage(
                    slaveInverterCommandMessageData,
                    $"Switch Axis {Axis.Vertical}",
                    FieldMessageActor.InverterDriver,
                    FieldMessageActor.FiniteStateMachines,
                    FieldMessageType.InverterSwitchOn,
                    (byte)InverterIndex.MainInverter);

                this.Logger.LogDebug($"3:Publishing Field Command Message {slaveInverterCommandMessage.Type} Destination {slaveInverterCommandMessage.Destination}");

                this.ParentStateMachine.PublishFieldCommandMessage(slaveInverterCommandMessage);
            }
            else
            {
                var ioCommandMessageData = new SwitchAxisFieldMessageData(Axis.Vertical);
                var ioCommandMessage = new FieldCommandMessage(
                    ioCommandMessageData,
                    $"Switch Axis {Axis.Vertical}",
                    FieldMessageActor.IoDriver,
                    FieldMessageActor.FiniteStateMachines,
                    FieldMessageType.SwitchAxis,
                    (byte)IoIndex.IoDevice1);

                this.Logger.LogDebug($"1:Publishing Field Command Message {ioCommandMessage.Type} Destination {ioCommandMessage.Destination}");

                this.ParentStateMachine.PublishFieldCommandMessage(ioCommandMessage);
            }

            var inverterCommandMessageData = new InverterSwitchOnFieldMessageData(Axis.Vertical);
            var inverterCommandMessage = new FieldCommandMessage(
                inverterCommandMessageData,
                $"Switch Axis {Axis.Vertical}",
                FieldMessageActor.InverterDriver,
                FieldMessageActor.FiniteStateMachines,
                FieldMessageType.InverterSwitchOn,
                (byte)InverterIndex.MainInverter);

            this.Logger.LogDebug($"3:Publishing Field Command Message {inverterCommandMessage.Type} Destination {inverterCommandMessage.Destination}");

            this.ParentStateMachine.PublishFieldCommandMessage(inverterCommandMessage);

            // Send a notification message about the start operation for MessageType.DrawerOperation
            var notificationMessageData = new DrawerOperationMessageData(
                this.machineData.DrawerOperationData.Operation,
                this.machineData.DrawerOperationData.Step,
                MessageVerbosity.Info);
            var notificationMessage = new NotificationMessage(
                notificationMessageData,
                $"Start",
                MessageActor.Any,
                MessageActor.FiniteStateMachines,
                MessageType.DrawerOperation,
                this.RequestingBay,
                this.RequestingBay,
                MessageStatus.OperationStart);

            this.Logger.LogDebug($"4:Publishing Automation Notification Message {notificationMessage.Type} Destination {notificationMessage.Destination} Status {notificationMessage.Status}");

            this.ParentStateMachine.PublishNotificationMessage(notificationMessage);
        }

        public override void Stop(StopRequestReason reason)
        {
            this.stateData.StopRequestReason = reason;
            this.ParentStateMachine.ChangeState(new MoveDrawerEndState(this.stateData));
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
