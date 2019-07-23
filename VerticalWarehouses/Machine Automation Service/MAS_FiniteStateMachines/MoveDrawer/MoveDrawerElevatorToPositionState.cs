using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS_FiniteStateMachines.Interface;
using Ferretto.VW.MAS_Utils.Enumerations;
using Ferretto.VW.MAS_Utils.Messages;
using Microsoft.Extensions.Logging;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS_FiniteStateMachines.MoveDrawer
{
    public class MoveDrawerElevatorToPositionState : StateBase
    {
        #region Fields

        private bool disposed;

        #endregion

        #region Constructors

        public MoveDrawerElevatorToPositionState(
            IStateMachine parentMachine,
            ILogger logger)
            : base(parentMachine, logger)
        {
        }

        #endregion

        #region Destructors

        ~MoveDrawerElevatorToPositionState()
        {
            this.Dispose(false);
        }

        #endregion

        #region Methods

        public override void ProcessCommandMessage(CommandMessage message)
        {
        }

        public override void ProcessFieldNotificationMessage(FieldNotificationMessage message)
        {
            //TODO when Inverter Driver notifies completion of Positioning at the destination level move to next state
            if (message.Type == FieldMessageType.NoType)
            {
                switch (message.Status)
                {
                    case MessageStatus.OperationEnd:
                        this.ParentStateMachine.ChangeState(new MoveDrawerSwitchAxisState(this.ParentStateMachine, Axis.Horizontal, this.Logger));
                        break;

                    case MessageStatus.OperationError:
                        this.ParentStateMachine.ChangeState(new MoveDrawerErrorState(this.ParentStateMachine, message, this.Logger));
                        break;
                }
            }
        }

        public override void ProcessNotificationMessage(NotificationMessage message)
        {
        }

        public override void Start()
        {
            //TODO Send Vertical Positioning command to inverter driver, loading positioning data from data layer
            var commandMessage = new FieldCommandMessage(
                null,
                $"Message Description",
                FieldMessageActor.IoDriver,
                FieldMessageActor.FiniteStateMachines,
                FieldMessageType.NoType);

            this.ParentStateMachine.PublishFieldCommandMessage(commandMessage);

            var notificationMessage = new NotificationMessage(
                null,
                "Message Description",
                MessageActor.Any,
                MessageActor.FiniteStateMachines,
                MessageType.NoType,
                MessageStatus.NoStatus);

            this.ParentStateMachine.PublishNotificationMessage(notificationMessage);
        }

        public override void Stop()
        {
            this.ParentStateMachine.ChangeState(new MoveDrawerEndState(this.ParentStateMachine, this.Logger, true));
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
