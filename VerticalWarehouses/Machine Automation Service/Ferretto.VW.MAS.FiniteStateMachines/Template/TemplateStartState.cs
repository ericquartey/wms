using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.FiniteStateMachines.Template.Interfaces;
using Ferretto.VW.MAS.Utils.Enumerations;
using Ferretto.VW.MAS.Utils.Messages;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS.FiniteStateMachines.Template
{
    public class TemplateStartState : StateBase
    {

        #region Fields

        private readonly ITemplateData machineData;

        private readonly ITemplateStateData stateData;

        private bool disposed;

        #endregion

        #region Constructors

        public TemplateStartState(ITemplateStateData stateData)
            : base(stateData.ParentMachine, stateData.MachineData.RequestingBay, stateData.MachineData.Logger)
        {
            this.stateData = stateData;
            this.machineData = stateData.MachineData as ITemplateData;
        }

        #endregion

        #region Destructors

        ~TemplateStartState()
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
            if (message.Type == FieldMessageType.NoType)
            {
                switch (message.Status)
                {
                    case MessageStatus.OperationEnd:
                        this.ParentStateMachine.ChangeState(new TemplateEndState(this.stateData));
                        break;

                    case MessageStatus.OperationError:
                        this.stateData.FieldMessage = message;
                        this.ParentStateMachine.ChangeState(new TemplateErrorState(this.stateData));
                        break;
                }
            }
        }

        public override void ProcessNotificationMessage(NotificationMessage message)
        {
        }

        public override void Start()
        {
            var commandMessage = new FieldCommandMessage(
                null,
                $"Template Start State Field Command",
                FieldMessageActor.IoDriver,
                FieldMessageActor.FiniteStateMachines,
                FieldMessageType.NoType,
                (byte)InverterIndex.None);

            this.ParentStateMachine.PublishFieldCommandMessage(commandMessage);

            var notificationMessage = new NotificationMessage(
                null,
                $"Template Start State Notification with {this.machineData.Message}",
                MessageActor.Any,
                MessageActor.FiniteStateMachines,
                MessageType.NoType,
                this.RequestingBay,
                MessageStatus.OperationStart);

            this.ParentStateMachine.PublishNotificationMessage(notificationMessage);
        }

        public override void Stop(StopRequestReason reason = StopRequestReason.Stop)
        {
            this.stateData.StopRequestReason = true;
            this.ParentStateMachine.ChangeState(new TemplateEndState(this.stateData));
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
