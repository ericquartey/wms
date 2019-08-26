using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;
using Ferretto.VW.MAS.FiniteStateMachines.Interface;
using Ferretto.VW.MAS.Utils.Enumerations;
using Ferretto.VW.MAS.Utils.Messages;
using Ferretto.VW.MAS.Utils.Messages.FieldData;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Prism.Events;

namespace Ferretto.VW.MAS.FiniteStateMachines.ShutterControl
{
    public class ShutterControlStateMachine : StateMachineBase
    {
        #region Fields

        private readonly ILogger logger;

        private readonly IShutterTestStatusChangedMessageData shutterControlMessageData;

        private bool disposed;

        private int numberOfExecutedCycles;

        private int numberOfRequestedCycles;

        #endregion

        #region Constructors

        public ShutterControlStateMachine(
            IEventAggregator eventAggregator,
            IShutterTestStatusChangedMessageData shutterControlMessageData,
            ILogger logger,
            IServiceScopeFactory serviceScopeFactory)
            : base(eventAggregator, logger, serviceScopeFactory)
        {
            this.logger = logger;

            this.logger.LogTrace("1:Method Start");

            this.CurrentState = new EmptyState(logger);

            this.shutterControlMessageData = shutterControlMessageData;
        }

        #endregion

        #region Destructors

        ~ShutterControlStateMachine()
        {
            this.Dispose(false);
        }

        #endregion

        #region Methods

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

        /// <inheritdoc/>
        public override void ChangeState(IState newState, CommandMessage message = null)
        {
            if (this.numberOfExecutedCycles == this.numberOfRequestedCycles)
            {
                this.shutterControlMessageData.ExecutedCycles = this.numberOfExecutedCycles;

                newState = new ShutterControlEndState(this, this.shutterControlMessageData, this.logger);
            }

            base.ChangeState(newState, message);
        }

        /// <inheritdoc/>
        public override void ProcessCommandMessage(CommandMessage message)
        {
            this.logger.LogTrace($"1:Process Command Message {message.Type} Source {message.Source}");

            lock (this.CurrentState)
            {
                this.CurrentState.ProcessCommandMessage(message);
            }
        }

        /// <inheritdoc/>
        public override void ProcessFieldNotificationMessage(FieldNotificationMessage message)
        {
            this.logger.LogTrace($"1:Process Field Notification Message {message.Type} Source {message.Source} Status {message.Status}");

            if (message.Type == FieldMessageType.ShutterPositioning)
            {
                if (message.Status == MessageStatus.OperationEnd && message.Data is InverterShutterPositioningFieldMessageData s)
                {
                    if (s.ShutterPosition == ShutterPosition.Opened)
                    {
                        this.numberOfExecutedCycles++;
                    }
                }
            }

            lock (this.CurrentState)
            {
                this.CurrentState.ProcessFieldNotificationMessage(message);
            }
        }

        /// <inheritdoc/>
        public override void ProcessNotificationMessage(NotificationMessage message)
        {
            this.logger.LogTrace($"1:Process Notification Message {message.Type} Source {message.Source} Status {message.Status}");

            lock (this.CurrentState)
            {
                this.CurrentState.ProcessNotificationMessage(message);
            }
        }

        /// <inheritdoc/>
        public override void PublishNotificationMessage(NotificationMessage message)
        {
            this.logger.LogTrace($"1:Publish Notification Message {message.Type} Source {message.Source} Status {message.Status}");

            if (message.Type == MessageType.ShutterPositioning && message.Status == MessageStatus.OperationExecuting)
            {
                //TEMP Update the number of executed cycles so far
                if (message.Data is IShutterTestStatusChangedMessageData s)
                {
                    s.ExecutedCycles = this.numberOfExecutedCycles;
                }
            }

            base.PublishNotificationMessage(message);
        }

        /// <inheritdoc/>
        public override void Start()
        {
            this.numberOfRequestedCycles = this.shutterControlMessageData.NumberCycles;
            this.numberOfExecutedCycles = -1;

            lock (this.CurrentState)
            {
                this.CurrentState = new ShutterControlStartState(this, this.shutterControlMessageData, this.logger);
                this.CurrentState?.Start();
            }

            this.logger.LogTrace($"1:CurrentState{this.CurrentState.GetType()}");
        }

        /// <inheritdoc/>
        public override void Stop()
        {
            this.logger.LogTrace("1:Method Start");

            lock (this.CurrentState)
            {
                this.CurrentState.Stop();
            }
        }

        #endregion
    }
}
