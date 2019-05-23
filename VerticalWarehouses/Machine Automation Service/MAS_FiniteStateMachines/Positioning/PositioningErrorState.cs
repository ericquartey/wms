using Ferretto.VW.Common_Utils.Messages;
using Ferretto.VW.Common_Utils.Messages.Data;
using Ferretto.VW.Common_Utils.Messages.Enumerations;
using Ferretto.VW.Common_Utils.Messages.Interfaces;
using Ferretto.VW.MAS_FiniteStateMachines.Interface;
using Ferretto.VW.MAS_Utils.Enumerations;
using Ferretto.VW.MAS_Utils.Messages;
using Ferretto.VW.MAS_Utils.Messages.FieldData;
using Microsoft.Extensions.Logging;
// ReSharper disable ArrangeThisQualifier

namespace Ferretto.VW.MAS_FiniteStateMachines.Positioning
{
    public class PositioningErrorState : StateBase
    {
        #region Fields

        private readonly decimal acceleration;

        private readonly Axis axisMovement;

        private readonly decimal deceleration;

        private readonly FieldNotificationMessage errorMessage;

        private readonly ILogger logger;

        private readonly decimal lowerBound;

        private readonly MovementType movementType;

        private readonly int numberCycles;

        private readonly decimal speed;

        private readonly decimal target;

        private readonly decimal upperBound;

        #endregion

        #region Constructors

        public PositioningErrorState(IStateMachine parentMachine, IPositioningMessageData positioningMessageData, FieldNotificationMessage errorMessage, ILogger logger)
        {
            logger.LogDebug( "1:Method Start" );

            this.logger = logger;
            this.ParentStateMachine = parentMachine;
            this.errorMessage = errorMessage;
        }

        #endregion

        #region Destructors

        ~PositioningErrorState()
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

            PositioningMessageData messageData = null;
            //if (message.Data is PositioningFieldMessageData data)
            //{
            //    messageData = new PositioningMessageData(data.AxisMovement, data.MovementType, data.TargetPosition, data.TargetSpeed,
            //        data.TargetAcceleration, data.TargetDeceleration, data.NumberCycles, this.positioningMessageData.LowerBound,
            //        this.positioningMessageData.UpperBound, data.Verbosity);
            //}
            var notificationMessage = new NotificationMessage(
                messageData,
                "Positioning Stopped due to an error",
                MessageActor.Any,
                MessageActor.FiniteStateMachines,
                MessageType.Positioning,
                MessageStatus.OperationError,
                ErrorLevel.Error );

            this.ParentStateMachine.PublishNotificationMessage( notificationMessage );
        }

        public override void ProcessNotificationMessage(NotificationMessage message)
        {
            this.logger.LogDebug( "1:Method Start" );

            this.logger.LogTrace( $"2:Process Notification Message {message.Type} Source {message.Source} Status {message.Status}" );
        }

        public override void Start()
        {
            this.logger.LogDebug( "1:Method Start" );

            var stopMessage = new FieldCommandMessage( null,
                $"Reset Positioning Axis {this.axisMovement}",
                FieldMessageActor.InverterDriver,
                FieldMessageActor.FiniteStateMachines,
                FieldMessageType.InverterStop );

            this.logger.LogTrace( $"2:Publish Field Command Message processed: {stopMessage.Type}, {stopMessage.Destination}" );

            this.ParentStateMachine.PublishFieldCommandMessage( stopMessage );

            var notificationMessageData = new PositioningMessageData( this.axisMovement, this.movementType, this.target, this.speed, this.acceleration, this.deceleration,
                this.numberCycles, this.lowerBound, this.upperBound, MessageVerbosity.Info );
            var notificationMessage = new NotificationMessage(
                                notificationMessageData,
                                "Positioning Error",
                                MessageActor.Any,
                                MessageActor.FiniteStateMachines,
                                MessageType.Positioning,
                                MessageStatus.OperationError );

            this.ParentStateMachine.PublishNotificationMessage( notificationMessage );
        }

        public override void Stop()
        {
            this.logger.LogDebug( "1:Method Start" );
        }

        #endregion
    }
}
