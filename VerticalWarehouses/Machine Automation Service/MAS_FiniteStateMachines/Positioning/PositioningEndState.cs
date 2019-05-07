using System;
using Ferretto.VW.Common_Utils.Messages;
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

        private readonly ILogger logger;

        private readonly IPositioningMessageData positioningMessageData;

        private readonly bool stopRequested;

        #endregion

        #region Constructors

        public PositioningEndState(IStateMachine parentMachine, IPositioningMessageData positioningMessageData, ILogger logger, bool stopRequested = false)
        {
            try
            {
                this.logger = logger;
                this.logger?.LogDebug("1:Method Start");

                this.stopRequested = stopRequested;
                this.ParentStateMachine = parentMachine;
                this.positioningMessageData = positioningMessageData;

                this.logger?.LogDebug("2:Method End");
            }
            catch (NullReferenceException)
            {
                throw new NullReferenceException();
            }
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

            this.logger.LogTrace($"2:Process NotificationMessage {message.Type} Source {message.Source} Status {message.Status}");

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
                                this.stopRequested ? MessageStatus.OperationStop : MessageStatus.OperationEnd);

                            this.ParentStateMachine.PublishNotificationMessage(notificationMessage);
                            break;

                        case MessageStatus.OperationError:
                            this.ParentStateMachine.ChangeState(new PositioningErrorState(this.ParentStateMachine, this.positioningMessageData, message, this.logger));
                            break;
                    }
                    break;
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
        }

        #endregion
    }
}
