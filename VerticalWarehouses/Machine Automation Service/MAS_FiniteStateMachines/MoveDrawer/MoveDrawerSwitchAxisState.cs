using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS_FiniteStateMachines.Interface;
using Ferretto.VW.MAS_Utils.Enumerations;
using Ferretto.VW.MAS_Utils.Messages;
using Microsoft.Extensions.Logging;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS_FiniteStateMachines.MoveDrawer
{
    public class MoveDrawerSwitchAxisState : StateBase
    {
        #region Fields

        private readonly Axis targetAxis;

        private bool disposed;

        #endregion

        #region Constructors

        public MoveDrawerSwitchAxisState(
            IStateMachine parentMachine,
            Axis targetAxis,
            ILogger logger)
            : base(parentMachine, logger)
        {
            this.targetAxis = targetAxis;
        }

        #endregion

        #region Destructors

        ~MoveDrawerSwitchAxisState()
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
            //TODO When IODriver and Inverter Driver report Axis Switch move to next state
            if (message.Type == FieldMessageType.NoType)
            {
                switch (message.Status)
                {
                    case MessageStatus.OperationEnd:
                        if (this.targetAxis == Axis.Horizontal)
                        {
                            this.ParentStateMachine.ChangeState(new MoveDrawerCradleState(this.ParentStateMachine, this.Logger));
                        }
                        else
                        {
                            this.ParentStateMachine.ChangeState(new MoveDrawerElevatorToPositionState(this.ParentStateMachine, this.Logger));
                        }
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
            //TODO Send Switch Axis commands to IODriver and Inverter Driver. Destination axis is provide by constructor
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
            //this.ParentStateMachine.ChangeState(new TemplateEndState(this.ParentStateMachine, this.Logger, true));
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
