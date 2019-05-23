using Ferretto.VW.Common_Utils.Messages;
using Ferretto.VW.Common_Utils.Messages.Enumerations;
using Ferretto.VW.Common_Utils.Messages.Interfaces;
using Ferretto.VW.MAS_FiniteStateMachines.Interface;
using Ferretto.VW.MAS_Utils.Enumerations;
using Ferretto.VW.MAS_Utils.Messages;
using Ferretto.VW.MAS_Utils.Messages.FieldData;
using Microsoft.Extensions.Logging;

namespace Ferretto.VW.MAS_FiniteStateMachines.VerticalPositioning
{
    public class VerticalPositioningStartState : StateBase
    {
        #region Fields

        private readonly ILogger logger;

        private IVerticalPositioningMessageData verticalPositioningMessageData;

        #endregion

        #region Constructors

        public VerticalPositioningStartState(IStateMachine parentMachine, IVerticalPositioningMessageData verticalPositioningMessageData, ILogger logger)
        {
            logger.LogDebug( "1:Method Start" );

            this.logger = logger;
            this.ParentStateMachine = parentMachine;
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
            this.logger.LogTrace( $"2:Process Field Notification Message {message.Type} Source {message.Source} Status {message.Status}" );

            if (message.Type == FieldMessageType.SwitchAxis)
            {
                switch (message.Status)
                {
                    case MessageStatus.OperationEnd:
                        this.ParentStateMachine.ChangeState( new VerticalPositioningExecutingState( this.ParentStateMachine, this.verticalPositioningMessageData, this.logger ) );
                        break;

                    case MessageStatus.OperationError:
                        this.ParentStateMachine.ChangeState( new VerticalPositioningErrorState( this.ParentStateMachine, this.verticalPositioningMessageData, message, this.logger ) );
                        break;
                }
            }
        }

        public override void ProcessNotificationMessage(NotificationMessage message)
        {
            this.logger.LogDebug( "1:Method Start" );

            this.logger.LogTrace( $"2:Process Notification Message {message.Type} Source {message.Source} Status {message.Status}" );
        }

        public override void Start()
        {
            this.logger.LogDebug( "1:Method Start" );

            this.verticalPositioningMessageData = verticalPositioningMessageData;

            var commandFieldMessageData = new SwitchAxisFieldMessageData( this.verticalPositioningMessageData.AxisMovement );
            var commandFieldMessage = new FieldCommandMessage( commandFieldMessageData,
                $"Switch Axis to {this.verticalPositioningMessageData.AxisMovement}",
                FieldMessageActor.InverterDriver,
                FieldMessageActor.FiniteStateMachines,
                FieldMessageType.SwitchAxis );

            this.logger.LogTrace( $"2:Publishing Field Command Message {commandFieldMessage.Type} Destination {commandFieldMessage.Destination}" );

            this.ParentStateMachine.PublishFieldCommandMessage( commandFieldMessage );

            var notificationMessage = new NotificationMessage(
                this.verticalPositioningMessageData,
                this.verticalPositioningMessageData.NumberCycles == 0 ? $"{this.verticalPositioningMessageData.AxisMovement} Positioning Started" : "Burnishing Started",
                MessageActor.Any,
                MessageActor.FiniteStateMachines,
                MessageType.VerticalPositioning,
                MessageStatus.OperationStart );

            this.logger.LogTrace( $"3:Publishing Automation Notification Message {notificationMessage.Type} Destination {notificationMessage.Destination} Status {notificationMessage.Status}" );

            this.ParentStateMachine.PublishNotificationMessage( notificationMessage );
        }

        public override void Stop()
        {
            this.logger.LogDebug( "1:Method Start" );

            this.ParentStateMachine.ChangeState( new VerticalPositioningEndState( this.ParentStateMachine, this.verticalPositioningMessageData, this.logger, 0, true ) );
        }

        #endregion
    }
}
