using Ferretto.VW.Common_Utils.Enumerations;
using Ferretto.VW.Common_Utils.Events;
using Ferretto.VW.Common_Utils.Messages;
using Ferretto.VW.Common_Utils.Messages.Data;
using Ferretto.VW.Common_Utils.Messages.Interfaces;
using Ferretto.VW.MAS_FiniteStateMachines.Interface;
using Microsoft.Extensions.Logging;
using Prism.Events;

namespace Ferretto.VW.MAS_FiniteStateMachines.Homing
{
    public class HomingStateMachine : StateMachineBase, IHomingStateMachine
    {
        #region Fields

        private readonly Axis calibrateAxis;

        private readonly ICalibrateMessageData calibrateMessageData;

        private readonly ILogger logger;

        private Axis currentAxis;

        private bool disposed;

        private bool IsStopRequested;

        private int NMaxSteps;

        private int NumberOfExecutedSteps;

        #endregion

        #region Constructors

        public HomingStateMachine(IEventAggregator eventAggregator, ICalibrateMessageData calibrateMessageData, ILogger logger)
            : base(eventAggregator, logger)
        {
            this.logger = logger;
            this.logger.LogTrace($"1:Homing State Machine ctor");

            this.calibrateMessageData = calibrateMessageData;
            this.calibrateAxis = calibrateMessageData.AxisToCalibrate;
            this.IsStopRequested = false;
            this.OperationDone = false;
        }

        #endregion

        #region Destructors

        ~HomingStateMachine()
        {
            this.Dispose(false);
        }

        #endregion

        #region Methods

        /// <inheritdoc/>
        public override void ProcessCommandMessage(CommandMessage message)
        {
            this.logger.LogTrace($"2:Process CommandMessage {message.Type} Source {message.Source}");
            switch (message.Type)
            {
                case MessageType.Stop:
                    //TODO Add business logic to stop current action at state machine level
                    this.IsStopRequested = true;
                    break;

                default:
                    break;
            }

            this.CurrentState.ProcessCommandMessage(message);
        }

        /// <inheritdoc/>
        public override void ProcessNotificationMessage(NotificationMessage message)
        {
            this.logger.LogTrace($"3:Process NotificationMessage {message.Type} Source {message.Source} Status {message.Status}");
            if (message.Type == MessageType.SwitchAxis)
            {
                switch (message.Status)
                {
                    case MessageStatus.OperationEnd:
                        if (this.OperationDone)
                        {
                            //TEMP Change to end state (the operation is done)
                            this.ChangeState(new HomingEndState(this, this.currentAxis, this.logger));
                        }
                        else
                        {
                            //TEMP Change to switch end state (the operation of switch for the current axis has been done)
                            this.ChangeState(new HomingSwitchAxisDoneState(this, this.currentAxis, this.logger));
                        }

                        return;

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
                        this.ChangeAxis();
                        break;

                    case MessageStatus.OperationError:
                        //TEMP Add business logic after an error occurs
                        break;

                    default:
                        break;
                }
            }

            this.CurrentState.ProcessNotificationMessage(message);
        }

        /// <inheritdoc/>
        public override void PublishNotificationMessage(NotificationMessage message)
        {
            this.logger.LogTrace($"4:Publish NotificationMessage {message.Type} Source {message.Source} Status {message.Status}");

            base.PublishNotificationMessage(message);
        }

        /// <inheritdoc/>
        public override void Start()
        {
            this.logger.LogTrace($"5:Starting FSM");
            switch (this.calibrateAxis)
            {
                case Axis.Both:
                    this.NMaxSteps = 3;
                    this.NumberOfExecutedSteps = 0;
                    this.currentAxis = Axis.Horizontal;
                    break;

                case Axis.Horizontal:
                    this.NMaxSteps = 1;
                    this.NumberOfExecutedSteps = 0;
                    this.currentAxis = Axis.Horizontal;
                    break;

                case Axis.Vertical:
                    this.NMaxSteps = 1;
                    this.NumberOfExecutedSteps = 0;
                    this.currentAxis = Axis.Vertical;
                    break;
            }

            this.CurrentState = new HomingStartState(this, this.currentAxis, this.logger);
            this.logger.LogTrace($"6:CurrentState{CurrentState.GetType()}");
        }

        protected override void Dispose(bool disposing)
        {
            if (this.disposed)
            {
                return;
            }

            if (disposing)
            {
            }

            this.disposed = true;
            base.Dispose(disposing);
        }

        /// <summary>
        /// Change the current axis.
        /// </summary>
        private Axis ChangeAxis()
        {
            this.currentAxis = (this.currentAxis == Axis.Vertical) ? Axis.Horizontal : Axis.Vertical;
            return this.currentAxis;
        }

        #endregion
    }
}
