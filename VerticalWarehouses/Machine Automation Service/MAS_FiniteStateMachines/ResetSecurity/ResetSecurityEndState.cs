using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.FiniteStateMachines.Interface;
using Ferretto.VW.MAS.Utils.Messages;
using Microsoft.Extensions.Logging;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS.FiniteStateMachines.ResetSecurity
{
    public class ResetSecurityEndState : StateBase
    {
        #region Fields

        private readonly bool stopRequested;

        private bool disposed;

        #endregion

        #region Constructors

        public ResetSecurityEndState(
            IStateMachine parentMachine,
            ILogger logger,
            bool stopRequested = false)
            : base(parentMachine, logger)
        {
            this.stopRequested = stopRequested;
        }

        #endregion

        #region Destructors

        ~ResetSecurityEndState()
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
        }

        /// <inheritdoc/>
        public override void ProcessNotificationMessage(NotificationMessage message)
        {
            this.Logger.LogTrace($"1:Process Notification Message {message.Type} Source {message.Source} Status {message.Status}");
        }

        public override void Start()
        {
            var notificationMessage = new NotificationMessage(
                null,
                "Reset Security Completed",
                MessageActor.Any,
                MessageActor.FiniteStateMachines,
                MessageType.ResetSecurity,
                this.stopRequested ? MessageStatus.OperationStop : MessageStatus.OperationEnd);

            this.Logger.LogTrace($"1:Publishing Automation Notification Message {notificationMessage.Type} Destination {notificationMessage.Destination} Status {notificationMessage.Status}");

            this.ParentStateMachine.PublishNotificationMessage(notificationMessage);
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
