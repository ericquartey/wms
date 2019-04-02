using Ferretto.VW.Common_Utils.Enumerations;
using Ferretto.VW.Common_Utils.Events;
using Ferretto.VW.Common_Utils.Messages;
using Ferretto.VW.Common_Utils.Messages.Interfaces;
using Ferretto.VW.MAS_FiniteStateMachines.Interface;
using Prism.Events;

namespace Ferretto.VW.MAS_FiniteStateMachines.Positioning
{
    public class PositioningStateMachine : StateMachineBase, IPositioningStateMachine
    {
        #region Fields

        private readonly Axis axisMovement;

        private readonly IPositioningMessageData positioningMessageData;

        private bool IsStopRequested;

        #endregion

        #region Constructors

        public PositioningStateMachine(IEventAggregator eventAggregator, IPositioningMessageData positioningMessageData)
            : base(eventAggregator)
        {
            this.axisMovement = positioningMessageData.AxisMovement;
            this.positioningMessageData = positioningMessageData;
            this.IsStopRequested = false;
            this.OperationDone = false;
        }

        #endregion

        #region Properties

        public IState GetState => this.CurrentState;

        public IPositioningMessageData PositioningData => this.positioningMessageData;

        #endregion

        #region Methods

        /// <inheritdoc/>
        public override void OnPublishNotification(NotificationMessage message)
        {
            switch (message.Type)
            {
                case MessageType.Movement:
                    {
                        //TEMP Send a notification about the start operation to all the world
                        var newMessage = new NotificationMessage(null,
                            string.Format("Start Positioning {0}", this.axisMovement),
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
                            string.Format("End Positioning {0}", this.axisMovement),
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
                    //TODO Add business logic to stop current action at state machine level
                    this.IsStopRequested = true;
                    break;

                default:
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
                        this.OperationDone = true;
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
            this.CurrentState = new PositioningStartState(this, this.positioningMessageData);
        }

        #endregion
    }
}
