using Ferretto.VW.MAS_FiniteStateMachines.Interface;
using Ferretto.VW.MAS_Utils.Enumerations;
using Ferretto.VW.MAS_Utils.Messages;
using Ferretto.VW.MAS_Utils.Messages.Data;
using Ferretto.VW.MAS_Utils.Messages.FieldData;
using Ferretto.VW.MAS_Utils.Messages.Interfaces;
using Microsoft.Extensions.Logging;

namespace Ferretto.VW.MAS_FiniteStateMachines.Positioning
{
    public class PositioningSwitchAxisDoneState : StateBase
    {
        #region Fields

        private readonly ILogger logger;

        private readonly IPositioningMessageData positioningMessageData;

        #endregion

        #region Constructors

        public PositioningSwitchAxisDoneState(IStateMachine parentMachine, IPositioningMessageData positioningMessageData, ILogger logger)
        {
            logger.LogDebug("1:Method Start");
            this.logger = logger;

            this.ParentStateMachine = parentMachine;

            this.positioningMessageData = positioningMessageData;

            var commandMessage = new FieldCommandMessage(new PositioningFieldMessageData(this.positioningMessageData),
                $"{this.positioningMessageData.AxisMovement} Positioning State Started",
                FieldMessageActor.InverterDriver,
                FieldMessageActor.FiniteStateMachines,
                FieldMessageType.Positioning);

            this.logger.LogTrace($"2:Publishing Field Command Message {commandMessage.Type} Destination {commandMessage.Destination}");

            this.ParentStateMachine.PublishFieldCommandMessage(commandMessage);

            this.logger.LogDebug("3:Method End");
        }

        #endregion

        #region Destructors

        ~PositioningSwitchAxisDoneState()
        {
            this.Dispose(false);
        }

        #endregion

        #region Methods

        public override void ProcessCommandMessage(CommandMessage message)
        {
            this.logger.LogDebug("1:Method Start");

            this.logger.LogTrace($"2:Process Command Message {message.Type} Source {message.Source}");

            this.logger.LogDebug("3:Method End");
        }

        public override void ProcessFieldNotificationMessage(FieldNotificationMessage message)
        {
            this.logger.LogDebug("1:Method Start");
            this.logger.LogTrace($"2:Process Field Notification Message {message.Type} Source {message.Source} Status {message.Status}");

            if (message.Type == FieldMessageType.Positioning)
            {
                switch (message.Status)
                {
                    case MessageStatus.OperationStart:
                        var commandMessage = new FieldCommandMessage(null,
                            $"{this.positioningMessageData.AxisMovement} Movement Start Request",
                            FieldMessageActor.InverterDriver,
                            FieldMessageActor.FiniteStateMachines,
                            FieldMessageType.Positioning);

                        this.logger.LogTrace($"3:Publishing Field Command Message {commandMessage.Type} Destination {commandMessage.Destination}");

                        this.ParentStateMachine.PublishFieldCommandMessage(commandMessage);
                        break;

                    case MessageStatus.OperationEnd:
                        this.ParentStateMachine.ChangeState(new PositioningEndState(this.ParentStateMachine, this.positioningMessageData, this.logger));
                        break;

                    case MessageStatus.OperationError:
                        this.ParentStateMachine.ChangeState(new PositioningErrorState(this.ParentStateMachine, this.positioningMessageData, message, this.logger));
                        break;
                }
            }
            else if (message.Type == FieldMessageType.InverterStatusUpdate)
            {
                switch (message.Status)
                {
                    case MessageStatus.OperationExecuting:
                        if (message.Data is InverterStatusUpdateFieldMessageData data)
                        {
                            var notificationMessage = new NotificationMessage(new CurrentPositionMessageData(data),
                                $"Current Encoder position: {data.CurrentPosition}",
                                MessageActor.AutomationService,
                                MessageActor.FiniteStateMachines,
                                MessageType.CurrentEncoderPosition,
                                MessageStatus.OperationExecuting
                            );

                            this.ParentStateMachine.PublishNotificationMessage(notificationMessage);
                        }
                        break;

                    case MessageStatus.OperationError:
                        this.ParentStateMachine.ChangeState(new PositioningErrorState(this.ParentStateMachine, this.positioningMessageData, message, this.logger));
                        break;
                }
            }
            this.logger.LogDebug("5:Method End");
        }

        public override void ProcessNotificationMessage(NotificationMessage message)
        {
            this.logger.LogDebug("1:Method Start");

            this.logger.LogTrace($"2:Process Notification Message {message.Type} Source {message.Source} Status {message.Status}");

            this.logger.LogDebug("3:Method End");
        }

        public override void Stop()
        {
            this.logger.LogDebug("1:Method Start");

            this.ParentStateMachine.ChangeState(new PositioningEndState(this.ParentStateMachine, this.positioningMessageData, this.logger, true));

            this.logger.LogDebug("2:Method End");
        }

        #endregion
    }
}
