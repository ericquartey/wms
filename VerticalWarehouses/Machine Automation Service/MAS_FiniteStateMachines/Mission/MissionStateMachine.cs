using Ferretto.VW.Common_Utils.Enumerations;
using Ferretto.VW.Common_Utils.Events;
using Ferretto.VW.Common_Utils.Messages;
using Prism.Events;

namespace Ferretto.VW.MAS_FiniteStateMachines.Mission
{
    public class MissionStateMachine : StateMachineBase
    {
        #region Fields

        private bool IsStopRequested;

        #endregion

        #region Constructors

        public MissionStateMachine(IEventAggregator eventAggregator)
            : base(eventAggregator)
        {
        }

        #endregion

        #region Methods

        /// <inheritdoc/>
        public override void OnPublishNotification(NotificationMessage message)
        {
            switch (message.Type)
            {
                case MessageType.StartAction:
                    {
                        //TEMP Send a notification about the start operation to all the world
                        var newMessage = new NotificationMessage(null,
                            "Mission Start",
                            MessageActor.Any,
                            MessageActor.FiniteStateMachines,
                            MessageType.Positioning,
                            MessageStatus.OperationStart,
                            ErrorLevel.NoError,
                            MessageVerbosity.Info);

                        this.EventAggregator.GetEvent<NotificationEvent>().Publish(newMessage);
                        break;
                    }

                case MessageType.Stop:
                    {
                        var msgStatus = (this.IsStopRequested) ? MessageStatus.OperationStop : MessageStatus.OperationEnd;
                        //TEMP Send a notification about the end (/stop) operation to all the world
                        var newMessage = new NotificationMessage(null,
                            "Mission End",
                            MessageActor.Any,
                            MessageActor.FiniteStateMachines,
                            MessageType.Stop,
                            msgStatus,
                            ErrorLevel.NoError,
                            MessageVerbosity.Info);

                        this.EventAggregator.GetEvent<NotificationEvent>().Publish(newMessage);
                        break;
                    }
                default:
                    {
                        break;
                    }
            }
        }

        /// <inheritdoc/>
        public override void ProcessCommandMessage(CommandMessage message)
        {
            switch (message.Type)
            {
                case MessageType.Stop:
                    //TEMP Add business logic to stop current action at state machine level
                    this.IsStopRequested = true;
                    break;

                case MessageType.EndAction:
                    //TEMP Add state business logic to stop current action
                    break;

                case MessageType.ErrorAction:
                    break;
            }

            this.CurrentState?.ProcessCommandMessage(message);
        }

        /// <inheritdoc/>
        public override void ProcessNotificationMessage(NotificationMessage message)
        {
            if (message.Type == MessageType.Positioning)
            {
                switch (message.Status)
                {
                    case MessageStatus.OperationEnd:
                        //TEMP Add business logic after the positioning operation is done successfully
                        break;

                    case MessageStatus.OperationError:
                        //TEMP Add business logic when an error occurs
                        break;

                    default:
                        break;
                }
            }

            this.CurrentState?.ProcessNotificationMessage(message);
        }

        /// <inheritdoc/>
        public override void Start()
        {
            this.CurrentState = new MissionStartState(this);
        }

        #endregion
    }
}
