using System;
using Ferretto.VW.Common_Utils.Messages;
using Ferretto.VW.Common_Utils.Messages.Enumerations;
using Ferretto.VW.Common_Utils.Messages.Interfaces;
using Ferretto.VW.MAS_Utils.Messages;
using Microsoft.Extensions.Logging;
using Prism.Events;
// ReSharper disable ArrangeThisQualifier

namespace Ferretto.VW.MAS_FiniteStateMachines.VerticalPositioning
{
    public class VerticalPositioningStateMachine : StateMachineBase
    {
        #region Fields

        private readonly ILogger logger;

        private readonly IVerticalPositioningMessageData verticalPositioningMessageData;

        #endregion

        #region Constructors

        public VerticalPositioningStateMachine(IEventAggregator eventAggregator, IVerticalPositioningMessageData verticalPositioningMessageData, ILogger logger)
            : base( eventAggregator, logger )
        {
            try
            {
                this.logger = logger;

                this.logger.LogDebug( "1:Method Start" );

                this.CurrentState = new EmptyState( logger );

                this.verticalPositioningMessageData = verticalPositioningMessageData;
            }
            catch (Exception ex)
            {
                throw new NullReferenceException( ex.Message );
            }
        }

        #endregion

        #region Methods

        public override void ProcessCommandMessage(CommandMessage message)
        {
            this.logger.LogDebug( "1:Method Start" );

            this.logger.LogTrace( $"2:Process Command Message {message.Type} Source {message.Source}" );

            lock (this.CurrentState)
            {
                if (message.Type == MessageType.Stop)
                {
                    this.CurrentState.Stop();
                }
                else
                {
                    this.CurrentState.ProcessCommandMessage( message );
                }
            }
        }

        public override void ProcessFieldNotificationMessage(FieldNotificationMessage message)
        {
            this.logger.LogDebug( "1:Method Start" );

            this.logger.LogTrace( $"2:Process Field Notification Message {message.Type} Source {message.Source} Status {message.Status}" );

            lock (this.CurrentState)
            {
                this.CurrentState.ProcessFieldNotificationMessage( message );
            }
        }

        public override void ProcessNotificationMessage(NotificationMessage message)
        {
            this.logger.LogDebug( "1:Method Start" );

            this.logger.LogTrace( $"2:Process Notification Message {message.Type} Source {message.Source} Status {message.Status}" );

            lock (this.CurrentState)
            {
                this.CurrentState.ProcessNotificationMessage( message );
            }
        }

        public override void Start()
        {
            this.logger.LogDebug( "1:Method Start" );

            lock (this.CurrentState)
            {
                this.CurrentState = new VerticalPositioningStartState( this, this.verticalPositioningMessageData, this.logger );
            }

            this.logger.LogTrace( $"2:CurrentState{this.CurrentState.GetType()}" );
        }

        public override void Stop()
        {
            this.logger.LogDebug( "1:Method Start" );

            lock (this.CurrentState)
            {
                this.CurrentState.Stop();
            }
        }

        #endregion
    }
}
