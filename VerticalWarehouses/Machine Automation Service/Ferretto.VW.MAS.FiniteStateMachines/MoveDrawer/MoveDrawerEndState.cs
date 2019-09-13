using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;
using Ferretto.VW.MAS.Utils.Messages;
using Microsoft.Extensions.Logging;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS.FiniteStateMachines.MoveDrawer
{
    internal class MoveDrawerEndState : StateBase
    {
        #region Fields

        private readonly IDrawerOperationMessageData drawerOperationData;

        private readonly bool stopRequested;

        private bool disposed;

        #endregion

        #region Constructors

        public MoveDrawerEndState(
            IStateMachine parentMachine,
            IDrawerOperationMessageData drawerOperationData,
            ILogger logger,
            bool stopRequested = false)
            : base(parentMachine, logger)
        {
            this.stopRequested = stopRequested;
            this.drawerOperationData = drawerOperationData;
        }

        #endregion

        #region Destructors

        ~MoveDrawerEndState()
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
            this.Logger.LogTrace($"1:Process FieldNotificationMessage {message.Type} Source {message.Source} Status {message.Status}");
        }

        /// <inheritdoc/>
        public override void ProcessNotificationMessage(NotificationMessage message)
        {
            this.Logger.LogTrace($"1:Process NotificationMessage {message.Type} Source {message.Source} Status {message.Status}");
        }

        public override void Start()
        {
            // Send a notification message about the completion of operation
            var notificationMessage = new NotificationMessage(
                this.drawerOperationData,
                $"{MessageType.DrawerOperation} end",
                MessageActor.Any,
                MessageActor.FiniteStateMachines,
                MessageType.DrawerOperation,
                this.stopRequested ? MessageStatus.OperationStop : MessageStatus.OperationEnd);

            this.ParentStateMachine.PublishNotificationMessage(notificationMessage);

            this.Logger.LogDebug($"1:Publishing Automation Notification Message {notificationMessage.Type} Destination {notificationMessage.Destination} Status {notificationMessage.Status}");
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
