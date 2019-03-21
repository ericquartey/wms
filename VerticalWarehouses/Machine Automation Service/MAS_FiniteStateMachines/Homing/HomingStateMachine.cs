using Ferretto.VW.Common_Utils.Enumerations;
using Ferretto.VW.Common_Utils.Events;
using Ferretto.VW.Common_Utils.Messages;
using Ferretto.VW.Common_Utils.Messages.Interfaces;
using Ferretto.VW.MAS_FiniteStateMachines.Interface;
using Prism.Events;

namespace Ferretto.VW.MAS_FiniteStateMachines.Homing
{
    public class HomingStateMachine : StateMachineBase, IHomingStateMachine
    {
        #region Fields

        private readonly Axis calibrateAxis;

        private readonly ICalibrateMessageData calibrateMessageData;

        private Axis currentAxis;

        private bool IsStopRequested;

        private int NMaxSteps;

        private int NumberOfExecutedSteps;

        #endregion

        #region Constructors

        public HomingStateMachine(IEventAggregator eventAggregator, ICalibrateMessageData calibrateMessageData)
            : base(eventAggregator)
        {
            this.calibrateMessageData = calibrateMessageData;
            this.calibrateAxis = calibrateMessageData.AxisToCalibrate;
            this.IsStopRequested = false;
            this.OperationDone = false;
        }

        #endregion

        #region Properties

        public ICalibrateMessageData CalibrateData => this.calibrateMessageData;

        public IState GetState => this.CurrentState;

        #endregion

        #region Methods

        /// <inheritdoc/>
        public override void OnPublishNotification(NotificationMessage message)
        {
            switch (message.Type)
            {
                case MessageType.Homing:
                    {
                        //TEMP Send a notification about the start operation to all the world
                        var newMessage = new NotificationMessage(null,
                            "Homing",
                            MessageActor.Any,
                            MessageActor.FiniteStateMachines,
                            MessageType.Homing,
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
                            "End Homing",
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
            if (message.Type == MessageType.SwitchAxis)
            {
                switch (message.Status)
                {
                    case MessageStatus.OperationEnd:
                        //TEMP Add business logic after the Switch axis operation is done successfully
                        break;

                    case MessageStatus.OperationError:
                        //TEMP Add business logic when an error occurs
                        break;

                    default:
                        break;
                }
            }

            if (message.Type == MessageType.CalibrateAxis)
            {
                switch (message.Status)
                {
                    case MessageStatus.OperationEnd:
                        //TEMP Add business logic after the CalibrateAxis operation is done successfully
                        this.NumberOfExecutedSteps++;
                        this.OperationDone = (this.NumberOfExecutedSteps == this.NMaxSteps);
                        this.changeAxis();
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
            switch (this.calibrateAxis)
            {
                case Axis.Both:
                    {
                        this.NMaxSteps = 3;
                        this.NumberOfExecutedSteps = 0;
                        this.currentAxis = Axis.Horizontal;
                        break;
                    }
                case Axis.Horizontal:
                    {
                        this.NMaxSteps = 1;
                        this.NumberOfExecutedSteps = 0;
                        this.currentAxis = Axis.Horizontal;
                        break;
                    }

                case Axis.Vertical:
                    {
                        this.NMaxSteps = 1;
                        this.NumberOfExecutedSteps = 0;
                        this.currentAxis = Axis.Vertical;
                        break;
                    }

                default:
                    {
                        break;
                    }
            }

            this.CurrentState = new HomingStartState(this, this.currentAxis);
        }

        /// <summary>
        /// Change the current axis.
        /// </summary>
        private Axis changeAxis()
        {
            this.currentAxis = (this.currentAxis == Axis.Vertical) ? Axis.Horizontal : Axis.Vertical;
            return this.currentAxis;
        }

        #endregion
    }
}
