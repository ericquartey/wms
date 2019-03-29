using Ferretto.VW.Common_Utils.Enumerations;
using Ferretto.VW.Common_Utils.Events;
using Ferretto.VW.Common_Utils.Messages;
using Ferretto.VW.MAS_FiniteStateMachines.Interface;
using Ferretto.VW.MAS_Utils.Messages.Data;
using Ferretto.VW.MAS_Utils.Messages.Interfaces;
using Prism.Events;

namespace Ferretto.VW.MAS_FiniteStateMachines.UpDownRepetitive
{
    public class UpDownRepetitiveStateMachine : StateMachineBase, IUpDownRepetitiveStateMachine
    {
        #region Fields

        private readonly IUpDownRepetitiveMessageData upDownMessageData;

        private bool IsStopRequested;

        private int NumberOfCompletedHalfCycles;

        private int NumberOfRequestedCycles;

        #endregion

        #region Constructors

        public UpDownRepetitiveStateMachine(IEventAggregator eventAggregator, IUpDownRepetitiveMessageData upDownMessageData)
                                    : base(eventAggregator)
        {
            this.upDownMessageData = upDownMessageData;
            this.NumberOfRequestedCycles = upDownMessageData.NumberOfRequiredCycles;
            this.IsStopRequested = false;
            this.OperationDone = false;
        }

        #endregion

        #region Properties

        public IState GetState => this.CurrentState;

        #endregion

        #region Methods

        /// <inheritdoc/>
        public override void OnPublishNotification(NotificationMessage message)
        {
            switch (message.Type)
            {
                case MessageType.UpDown:
                    {
                        var numberOfCompletedCycles = this.NumberOfCompletedHalfCycles / 2;
                        var upDownMessage = new UpDownRepetitiveNotificationMessageData(numberOfCompletedCycles);

                        //TEMP Send a notification about the up&down progression to all the world
                        var newMessage = new NotificationMessage(upDownMessage,
                            "Up&Down executing",
                            MessageActor.Any,
                            MessageActor.FiniteStateMachines,
                            MessageType.UpDown,
                            MessageStatus.OperationExecuting,
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
                            "Up&Down End",
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
                        this.NumberOfCompletedHalfCycles++;

                        var numberOfCompletedCycles = this.NumberOfCompletedHalfCycles / 2;
                        this.OperationDone = (this.NumberOfRequestedCycles == numberOfCompletedCycles);

                        break;

                    case MessageStatus.OperationError:
                        //TEMP Add business logic after an error occurs
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
            this.CurrentState = new UpDownStartState(this, this.upDownMessageData);
        }

        #endregion
    }
}
