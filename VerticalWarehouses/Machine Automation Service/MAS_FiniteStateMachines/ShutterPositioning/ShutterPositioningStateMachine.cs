using Ferretto.VW.Common_Utils.Messages;
using Ferretto.VW.Common_Utils.Messages.Enumerations;
using Ferretto.VW.Common_Utils.Messages.Interfaces;
using Ferretto.VW.MAS_DataLayer.Interfaces;
using Ferretto.VW.MAS_Utils.Messages;
using Microsoft.Extensions.Logging;
using Prism.Events;
// ReSharper disable ArrangeThisQualifier

namespace Ferretto.VW.MAS_FiniteStateMachines.ShutterPositioning
{
    public class ShutterPositioningStateMachine : StateMachineBase
    {
        #region Fields

        private readonly IDataLayerConfigurationValueManagment dataLayerConfigurationValueManagment;

        private readonly ILogger logger;

        private readonly IShutterPositioningMessageData shutterPositioningMessageData;

        private readonly int shutterType;

        private bool disposed;

        #endregion

        #region Constructors

        public ShutterPositioningStateMachine(IEventAggregator eventAggregator, IShutterPositioningMessageData shutterPositioningMessageData, ILogger logger)
            : base(eventAggregator, logger)
        {
            logger.LogDebug("1:Method Start");

            this.logger = logger;

            this.CurrentState = new EmptyState(logger);

            this.shutterPositioningMessageData = shutterPositioningMessageData;

            this.shutterType = shutterPositioningMessageData.ShutterType;

            logger.LogDebug("2:Method End");
        }

        #endregion

        #region Destructors

        ~ShutterPositioningStateMachine()
        {
            this.Dispose(false);
        }

        #endregion

        #region Methods

        /// <inheritdoc/>
        public override void ProcessCommandMessage(CommandMessage message)
        {
            this.logger.LogDebug("1:Method Start");

            this.logger.LogTrace($"2:Process Command Message {message.Type} Source {message.Source}");

            lock (this.CurrentState)
            {
                if (message.Type == MessageType.Stop)
                {
                    this.CurrentState.Stop();
                }
                else
                {
                    this.CurrentState.ProcessCommandMessage(message);
                }
            }
            this.logger.LogDebug("3:Method End");
        }

        public override void ProcessFieldNotificationMessage(FieldNotificationMessage message)
        {
            this.logger.LogDebug("1:Method Start");

            this.logger.LogTrace($"2:Process Field Notification Message {message.Type} Source {message.Source} Status {message.Status}");

            lock (this.CurrentState)
            {
                this.CurrentState.ProcessFieldNotificationMessage(message);
            }

            this.logger.LogDebug("3:Method End");
        }

        /// <inheritdoc/>
        public override void ProcessNotificationMessage(NotificationMessage message)
        {
            this.logger.LogDebug("1:Method Start");

            this.logger.LogTrace($"2:Process Notification Message {message.Type} Source {message.Source} Status {message.Status}");

            lock (this.CurrentState)
            {
                this.CurrentState.ProcessNotificationMessage(message);
            }

            this.logger.LogDebug("3:Method End");
        }

        /// <inheritdoc/>
        public override void PublishNotificationMessage(NotificationMessage message)
        {
            this.logger.LogDebug("1:Method Start");

            this.logger.LogTrace($"2:Publish Notification Message {message.Type} Source {message.Source} Status {message.Status}");

            base.PublishNotificationMessage(message);

            this.logger.LogDebug("3:Method End");
        }

        /// <inheritdoc/>
        public override void Start()
        {
            this.logger.LogDebug("1:Method Start");
            lock (this.CurrentState)
            {
                this.CurrentState = new ShutterPositioningStartState(this, this.shutterPositioningMessageData, this.logger, this.shutterType);
            }

            this.logger.LogTrace($"2:CurrentState{this.CurrentState.GetType()}");
            this.logger.LogDebug("3:Method End");
        }

        public override void Stop()
        {
            this.logger.LogDebug("1:Method Start");

            lock (this.CurrentState)
            {
                this.CurrentState.Stop();
            }

            this.logger.LogDebug("2:Method End");
        }

        protected override void Dispose(bool disposing)
        {
            if (this.disposed)
            {
                return;
            }

            this.disposed = true;
            base.Dispose(disposing);
        }

        #endregion
    }
}
