using System;
using Ferretto.VW.MAS_Utils.Enumerations;
using Ferretto.VW.MAS_Utils.Messages;
using Ferretto.VW.MAS_Utils.Messages.Interfaces;
using Microsoft.Extensions.Logging;
using Prism.Events;
// ReSharper disable ArrangeThisQualifier

namespace Ferretto.VW.MAS_FiniteStateMachines.Positioning
{
    public class PositioningStateMachine : StateMachineBase
    {
        #region Fields

        private readonly ILogger logger;

        private readonly IPositioningMessageData positioningMessageData;

        #endregion

        #region Constructors

        public PositioningStateMachine(IEventAggregator eventAggregator, IPositioningMessageData positioningMessageData, ILogger logger)
            : base(eventAggregator, logger)
        {
            try
            {
                this.logger = logger;
                this.logger.LogDebug("1:Method Start");

                this.CurrentState = new EmptyState(logger);

                this.positioningMessageData = positioningMessageData;

                this.logger.LogDebug("2:Method End");
            }
            catch (Exception ex)
            {
                throw new NullReferenceException(ex.Message);
            }
        }

        #endregion

        #region Methods

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

        public override void Start()
        {
            this.logger.LogDebug("1:Method Start");

            lock (this.CurrentState)
            {
                this.CurrentState = new PositioningStartState(this, this.positioningMessageData, this.logger);
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

            this.logger.LogDebug("2:Method End");
        }

        #endregion
    }
}
