using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.FiniteStateMachines.Template.Interfaces;
using Ferretto.VW.MAS.Utils.Messages;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS.FiniteStateMachines.Template
{
    public class TemplateEndState : StateBase
    {

        #region Fields

        private readonly ITemplateData machineData;

        private readonly ITemplateStateData stateData;

        private bool disposed;

        #endregion

        #region Constructors

        public TemplateEndState(ITemplateStateData stateData)
            : base(stateData.ParentMachine, stateData.MachineData.RequestingBay, stateData.MachineData.Logger)
        {
            this.stateData = stateData;
            this.machineData = stateData.MachineData as ITemplateData;
        }

        #endregion

        #region Destructors

        ~TemplateEndState()
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
        }

        /// <inheritdoc/>
        public override void ProcessNotificationMessage(NotificationMessage message)
        {
        }

        public override void Start()
        {
            var notificationMessage = new NotificationMessage(
                null,
                $"Template End State Notification with {this.machineData.Message} and {this.stateData.Message}",
                MessageActor.Any,
                MessageActor.FiniteStateMachines,
                MessageType.NoType,
                this.RequestingBay,
                this.stateData.StopRequestReason ? MessageStatus.OperationStop : MessageStatus.OperationEnd);

            this.ParentStateMachine.PublishNotificationMessage(notificationMessage);
        }

        public override void Stop(StopRequestReason reason = StopRequestReason.Stop)
        {
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
