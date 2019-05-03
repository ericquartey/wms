using Ferretto.VW.Common_Utils.Messages;
using Ferretto.VW.Common_Utils.Messages.Enumerations;
using Ferretto.VW.Common_Utils.Messages.Interfaces;
using Ferretto.VW.MAS_FiniteStateMachines.Interface;
using Ferretto.VW.MAS_Utils.Enumerations;
using Ferretto.VW.MAS_Utils.Messages;
using Ferretto.VW.MAS_Utils.Messages.FieldData;
using Microsoft.Extensions.Logging;

namespace Ferretto.VW.MAS_FiniteStateMachines.BeltBurnishing
{
    public class BeltBurnishingStartState : StateBase
    {
        #region Fields

        private readonly ILogger logger;

        private readonly IPositioningMessageData positioningMessageData;

        #endregion

        #region Constructors

        public BeltBurnishingStartState(IStateMachine parentMachine, IPositioningMessageData positioningMessageData, ILogger logger)
        {
            this.logger = logger;
            this.logger.LogDebug("1:Method Start");

            this.ParentStateMachine = parentMachine;

            this.positioningMessageData = positioningMessageData;

            var commandFieldMessageData = new SwitchAxisFieldMessageData(this.positioningMessageData.AxisMovement);
            var commandFieldMessage = new FieldCommandMessage(commandFieldMessageData,
                $"Switch Axis to {this.positioningMessageData.AxisMovement}",
                FieldMessageActor.InverterDriver,
                FieldMessageActor.FiniteStateMachines,
                FieldMessageType.SwitchAxis);

            this.logger.LogTrace($"2:Publishing Field Command Message {commandFieldMessage.Type} Destination {commandFieldMessage.Destination}");

            this.ParentStateMachine.PublishFieldCommandMessage(commandFieldMessage);

            var notificationMessage = new NotificationMessage(
                this.positioningMessageData,
                "Belt Break-In Started",
                MessageActor.Any,
                MessageActor.FiniteStateMachines,
                MessageType.BeltBurnishing,
                MessageStatus.OperationStart);

            this.logger.LogTrace($"3:Publishing Automation Notification Message {notificationMessage.Type} Destination {notificationMessage.Destination} Status {notificationMessage.Status}");

            this.ParentStateMachine.PublishNotificationMessage(notificationMessage);

            this.logger.LogDebug("4:Method End");
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

            if (message.Type == FieldMessageType.SwitchAxis)
            {
                switch (message.Status)
                {
                    case MessageStatus.OperationEnd:
                        this.ParentStateMachine.ChangeState(new BeltBurnishingExecutingState(this.ParentStateMachine, this.positioningMessageData, this.logger));
                        break;

                    case MessageStatus.OperationError:
                        this.ParentStateMachine.ChangeState(new BeltBurnishingErrorState(this.ParentStateMachine, this.positioningMessageData, message, this.logger));
                        break;
                }
            }

            this.logger.LogDebug("3:Method End");
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

            this.ParentStateMachine.ChangeState(new BeltBurnishingEndState(this.ParentStateMachine, this.positioningMessageData, this.logger, 0, true));

            this.logger.LogDebug("2:Method End");
        }

        #endregion
    }
}
