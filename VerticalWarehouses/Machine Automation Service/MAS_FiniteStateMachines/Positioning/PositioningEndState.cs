using System;
using Ferretto.VW.Common_Utils.Messages;
using Ferretto.VW.Common_Utils.Messages.Data;
using Ferretto.VW.Common_Utils.Messages.Enumerations;
using Ferretto.VW.Common_Utils.Messages.Interfaces;
using Ferretto.VW.MAS_FiniteStateMachines.Interface;
using Ferretto.VW.MAS_Utils.Enumerations;
using Ferretto.VW.MAS_Utils.Messages;
using Microsoft.Extensions.Logging;
// ReSharper disable ArrangeThisQualifier

namespace Ferretto.VW.MAS_FiniteStateMachines.Positioning
{
    public class PositioningEndState : StateBase
    {
        #region Fields

        private readonly decimal acceleration;

        private readonly Axis axisMovement;

        private readonly decimal deceleration;

        private readonly ILogger logger;

        private readonly decimal lowerBound;

        private readonly MovementType movementType;

        private readonly int numberCycles;

        private readonly IPositioningMessageData positioningMessageData;

        private readonly decimal speed;

        private readonly bool stopRequested;

        private readonly decimal target;

        private readonly decimal upperBound;

        #endregion

        #region Constructors

        public PositioningEndState(IStateMachine parentMachine, IPositioningMessageData positioningMessageData, ILogger logger, bool stopRequested = false)
        {
            this.logger?.LogDebug( "1:Method Start" );

            this.logger = logger;
            this.stopRequested = stopRequested;
            this.ParentStateMachine = parentMachine;
            this.positioningMessageData = positioningMessageData;
        }

        #endregion

        #region Destructors

        ~PositioningEndState()
        {
            this.Dispose( false );
        }

        #endregion

        #region Methods

        public override void ProcessCommandMessage(CommandMessage message)
        {
            this.logger.LogDebug( "1:Method Start" );

            this.logger.LogTrace( $"2:Process Command Message {message.Type} Source {message.Source}" );
        }

        public override void ProcessFieldNotificationMessage(FieldNotificationMessage message)
        {
            this.logger.LogDebug( "1:Method Start" );

            this.logger.LogTrace( $"2:Process NotificationMessage {message.Type} Source {message.Source} Status {message.Status}" );

            switch (message.Type)
            {
                case FieldMessageType.InverterPowerOff:
                    switch (message.Status)
                    {
                        case MessageStatus.OperationStop:
                        case MessageStatus.OperationEnd:
                            var notificationMessage = new NotificationMessage(
                                null,
                                "Positioning Completed",
                                MessageActor.Any,
                                MessageActor.FiniteStateMachines,
                                MessageType.Positioning,
                                this.stopRequested ? MessageStatus.OperationStop : MessageStatus.OperationEnd );

                            this.ParentStateMachine.PublishNotificationMessage( notificationMessage );
                            break;

                        case MessageStatus.OperationError:
                            this.ParentStateMachine.ChangeState( new PositioningErrorState( this.ParentStateMachine, this.positioningMessageData, message, this.logger ) );
                            break;
                    }
                    break;
            }
        }

        public override void ProcessNotificationMessage(NotificationMessage message)
        {
            this.logger.LogDebug( "1:Method Start" );

            this.logger.LogTrace( $"2:Process Notification Message {message.Type} Source {message.Source} Status {message.Status}" );
        }

        public override void Start()
        {
            this.logger?.LogDebug( "1:Method Start" );

            var notificationMessageData = new PositioningMessageData( this.axisMovement, this.movementType, this.target, this.speed, this.acceleration, this.deceleration,
                this.numberCycles, this.lowerBound, this.upperBound, MessageVerbosity.Info );
            var notificationMessage = new NotificationMessage(
                notificationMessageData,
                "Positioning Completed",
                MessageActor.Any,
                MessageActor.FiniteStateMachines,
                MessageType.Positioning,
                this.stopRequested ? MessageStatus.OperationStop : MessageStatus.OperationEnd );

            this.logger.LogTrace( $"2:Publishing Automation Notification Message {notificationMessage.Type} Destination {notificationMessage.Destination} Status {notificationMessage.Status}" );

            this.ParentStateMachine.PublishNotificationMessage( notificationMessage );
        }

        public override void Stop()
        {
            this.logger.LogDebug( "1:Method Start" );
        }

        #endregion
    }
}
