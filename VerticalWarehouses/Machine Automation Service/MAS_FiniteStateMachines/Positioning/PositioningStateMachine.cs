using Ferretto.VW.Common_Utils.Messages;
using Ferretto.VW.Common_Utils.Messages.Interfaces;
using Ferretto.VW.MAS_Utils.Messages;
using Microsoft.Extensions.Logging;
using Prism.Events;

// ReSharper disable ArrangeThisQualifier

namespace Ferretto.VW.MAS_FiniteStateMachines.Positioning
{
    public class PositioningStateMachine : StateMachineBase
    {
        #region Fields

        private readonly ILogger logger;

        private readonly IVerticalPositioningMessageData verticalPositioningMessageData;

        private bool disposed;

        #endregion

        #region Constructors

        public PositioningStateMachine(IEventAggregator eventAggregator, IVerticalPositioningMessageData verticalPositioningMessageData, ILogger logger)
            : base(eventAggregator, logger)
        {
            this.logger = logger;

            this.logger.LogDebug("1:Method Start");

            this.CurrentState = new EmptyState(logger);

            this.verticalPositioningMessageData = verticalPositioningMessageData;
        }

        #endregion

        #region Destructors

        ~PositioningStateMachine()
        {
            this.Dispose(false);
        }

        #endregion

        #region Methods

        public override void ProcessCommandMessage(CommandMessage message)
        {
            this.logger.LogDebug("1:Method Start");

            this.logger.LogTrace($"2:Process Command Message {message.Type} Source {message.Source}");

            lock (this.CurrentState)
            {
                this.CurrentState.ProcessCommandMessage(message);
            }
        }

        public override void ProcessFieldNotificationMessage(FieldNotificationMessage message)
        {
            this.logger.LogDebug("1:Method Start");

            this.logger.LogTrace($"2:Process Field Notification Message {message.Type} Source {message.Source} Status {message.Status}");

            lock (this.CurrentState)
            {
                this.CurrentState.ProcessFieldNotificationMessage(message);
            }
        }

        public override void ProcessNotificationMessage(NotificationMessage message)
        {
            this.logger.LogDebug("1:Method Start");

            this.logger.LogTrace($"2:Process Notification Message {message.Type} Source {message.Source} Status {message.Status}");

            lock (this.CurrentState)
            {
                this.CurrentState.ProcessNotificationMessage(message);
            }
        }

        public override void Start()
        {
            this.logger.LogDebug("1:Method Start");

            lock (this.CurrentState)
            {
                this.CurrentState = new PositioningStartState(this, this.verticalPositioningMessageData, this.logger);
                this.CurrentState?.Start();
            }

            this.logger.LogTrace($"2:CurrentState{this.CurrentState.GetType()}");
        }

        public override void Stop()
        {
            this.logger.LogDebug("1:Method Start");

            lock (this.CurrentState)
            {
                this.CurrentState.Stop();
            }
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
