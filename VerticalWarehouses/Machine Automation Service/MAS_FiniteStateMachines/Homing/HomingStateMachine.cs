using Ferretto.VW.MAS_Utils.Enumerations;
using Ferretto.VW.MAS_Utils.Messages;
using Ferretto.VW.MAS_Utils.Messages.Interfaces;
using Microsoft.Extensions.Logging;
using Prism.Events;
// ReSharper disable ArrangeThisQualifier

namespace Ferretto.VW.MAS_FiniteStateMachines.Homing
{
    public class HomingStateMachine : StateMachineBase
    {
        #region Fields

        private readonly Axis calibrateAxis;

        private readonly ILogger logger;

        private Axis currentAxis;

        private bool disposed;

        private int nMaxSteps;

        private int numberOfExecutedSteps;

        #endregion

        #region Constructors

        public HomingStateMachine(IEventAggregator eventAggregator, IHomingMessageData calibrateMessageData, ILogger logger)
            : base(eventAggregator, logger)
        {
            logger.LogDebug("1:Method Start");
            this.logger = logger;

            this.calibrateAxis = calibrateMessageData.AxisToCalibrate;
            this.OperationDone = false;

            logger.LogDebug("2:Method End");
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
            this.logger.LogTrace($"1:Process Command Message {message.Type} Source {message.Source}");

            this.CurrentState.ProcessCommandMessage(message);
        }

        public override void ProcessFieldNotificationMessage(FieldNotificationMessage message)
        {
            this.logger.LogTrace($"1:Process Field Notification Message {message.Type} Source {message.Source} Status {message.Status}");

            this.CurrentState.ProcessFieldNotificationMessage(message);
        }

        /// <inheritdoc/>
        public override void ProcessNotificationMessage(NotificationMessage message)
        {
            this.logger.LogTrace($"1:Process Notification Message {message.Type} Source {message.Source} Status {message.Status}");

            this.CurrentState.ProcessNotificationMessage(message);

            //if (message.Type == MessageType.SwitchAxis)
            //{
            //    switch (message.Status)
            //    {
            //        case MessageStatus.OperationEnd:
            //            if (this.OperationDone)
            //            {
            //                //TEMP Change to end state (the operation is done)
            //                this.ChangeState(new HomingEndState(this, this.currentAxis, this.logger));
            //            }
            //            else
            //            {
            //                //TEMP Change to switch end state (the operation of switch for the current axis has been done)
            //                this.ChangeState(new HomingSwitchAxisDoneState(this, this.currentAxis, this.logger));
            //            }

            //            return;

            //        case MessageStatus.OperationError:
            //            //TEMP Add business logic when an error occurs
            //            break;
            //    }
            //}

            //if (message.Type == MessageType.CalibrateAxis)
            //{
            //    switch (message.Status)
            //    {
            //        case MessageStatus.OperationEnd:
            //            //TEMP Add business logic after the CalibrateAxis operation is done successfully
            //            this.numberOfExecutedSteps++;
            //            this.OperationDone = (this.numberOfExecutedSteps == this.nMaxSteps);
            //            this.ChangeAxis();
            //            break;

            //        case MessageStatus.OperationError:
            //            //TEMP Add business logic after an error occurs
            //            break;
            //    }
            //}
        }

        /// <inheritdoc/>
        public override void PublishNotificationMessage(NotificationMessage message)
        {
            this.logger.LogTrace($"1:Publish Notification Message {message.Type} Source {message.Source} Status {message.Status}");

            base.PublishNotificationMessage(message);
        }

        /// <inheritdoc/>
        public override void Start()
        {
            logger.LogDebug("1:Method Start");
            switch (this.calibrateAxis)
            {
                case Axis.Both:
                    this.nMaxSteps = 3;
                    this.numberOfExecutedSteps = 0;
                    this.currentAxis = Axis.Horizontal;
                    break;

                case Axis.Horizontal:
                    this.nMaxSteps = 1;
                    this.numberOfExecutedSteps = 0;
                    this.currentAxis = Axis.Horizontal;
                    break;

                case Axis.Vertical:
                    this.nMaxSteps = 1;
                    this.numberOfExecutedSteps = 0;
                    this.currentAxis = Axis.Vertical;
                    break;
            }

            this.CurrentState = new HomingStartState(this, this.currentAxis, this.logger);

            this.logger.LogTrace($"2:CurrentState{CurrentState.GetType()}");
            logger.LogDebug("1:Method End");
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
        private void ChangeAxis()
        {
            this.currentAxis = (this.currentAxis == Axis.Vertical) ? Axis.Horizontal : Axis.Vertical;
        }

        #endregion
    }
}
