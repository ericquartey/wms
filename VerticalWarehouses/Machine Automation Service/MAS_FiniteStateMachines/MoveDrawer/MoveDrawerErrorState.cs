using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS_FiniteStateMachines.Interface;
using Ferretto.VW.MAS_Utils.Enumerations;
using Ferretto.VW.MAS_Utils.Messages;
using Microsoft.Extensions.Logging;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS_FiniteStateMachines.MoveDrawer
{
    public class MoveDrawerErrorState : StateBase
    {
        #region Fields

        private readonly FieldNotificationMessage errorMessage;

        private bool disposed;

        #endregion

        #region Constructors

        public MoveDrawerErrorState(
            IStateMachine parentMachine,
            FieldNotificationMessage errorMessage,
            ILogger logger)
            : base(parentMachine, logger)
        {
            this.errorMessage = errorMessage;
        }

        #endregion

        #region Destructors

        ~MoveDrawerErrorState()
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
            var stopMessage = new FieldCommandMessage(
                null,
                $"Message Description",
                FieldMessageActor.InverterDriver,
                FieldMessageActor.FiniteStateMachines,
                FieldMessageType.NoType);

            this.ParentStateMachine.PublishFieldCommandMessage(stopMessage);

            var notificationMessage = new NotificationMessage(
                null,
                "Message Description",
                MessageActor.Any,
                MessageActor.FiniteStateMachines,
                MessageType.NoType,
                MessageStatus.OperationError);

            this.ParentStateMachine.PublishNotificationMessage(notificationMessage);
        }

        public override void Stop()
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
