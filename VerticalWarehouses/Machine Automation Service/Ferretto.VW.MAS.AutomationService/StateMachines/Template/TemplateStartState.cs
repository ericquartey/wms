using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.AutomationService.StateMachines.Template.Interfaces;
using Ferretto.VW.MAS.Utils.Messages;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS.AutomationService.StateMachines.Template
{
    public class TemplateStartState : StateBase
    {
        #region Fields

        private readonly ITemplateMachineData machineData;

        private readonly ITemplateStateData stateData;

        private bool disposed;

        #endregion

        #region Constructors

        public TemplateStartState(ITemplateStateData stateData)
            : base(stateData.ParentMachine, stateData.MachineData.RequestingBay, stateData.MachineData.Logger)
        {
            this.stateData = stateData;
            this.machineData = stateData.MachineData as ITemplateMachineData;
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

        public override void ProcessNotificationMessage(NotificationMessage message)
        {
            if (message.Type == MessageType.NoType)
            {
                switch (message.Status)
                {
                    case MessageStatus.OperationEnd:
                        this.ParentStateMachine.ChangeState(new TemplateEndState(this.stateData));
                        break;

                    case MessageStatus.OperationError:
                        this.stateData.NotificationMessage = message;
                        this.ParentStateMachine.ChangeState(new TemplateErrorState(this.stateData));
                        break;
                }
            }
        }

        public override void Start()
        {
            var commandMessage = new CommandMessage(
                null,
                $"Template Start State Field Command",
                MessageActor.IoDriver,
                MessageActor.FiniteStateMachines,
                MessageType.NoType,
                BayNumber.None,
                BayNumber.None);

            this.ParentStateMachine.PublishCommandMessage(commandMessage);

            var notificationMessage = new NotificationMessage(
                null,
                $"Template Start State Notification with {this.machineData.Message}",
                MessageActor.Any,
                MessageActor.FiniteStateMachines,
                MessageType.NoType,
                this.RequestingBay,
                this.RequestingBay,
                MessageStatus.OperationStart);

            this.ParentStateMachine.PublishNotificationMessage(notificationMessage);
        }

        public override void Stop(StopRequestReason reason)
        {
            this.stateData.StopRequestReason = reason;
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
