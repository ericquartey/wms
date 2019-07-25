using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;
using Ferretto.VW.MAS.FiniteStateMachines.Interface;
using Ferretto.VW.MAS.Utils.Enumerations;
using Ferretto.VW.MAS.Utils.Messages;
using Microsoft.Extensions.Logging;
using Prism.Events;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS.FiniteStateMachines.Homing
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
            logger.LogTrace("1:Method Start");
            this.logger = logger;

            this.CurrentState = new EmptyState(logger);

            this.calibrateAxis = calibrateMessageData.AxisToCalibrate;
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
        public override void ChangeState(IState newState, CommandMessage message = null)
        {
            if (this.numberOfExecutedSteps == this.nMaxSteps)
            {
                newState = new HomingEndState(this, this.currentAxis, this.logger);
            }
            base.ChangeState(newState, message);
        }

        /// <inheritdoc/>
        public override void ProcessCommandMessage(CommandMessage message)
        {
            this.logger.LogTrace($"1:Process Command Message {message.Type} Source {message.Source}");

            lock (this.CurrentState)
            {
                this.CurrentState.ProcessCommandMessage(message);
            }
        }

        public override void ProcessFieldNotificationMessage(FieldNotificationMessage message)
        {
            this.logger.LogTrace($"1:Process Field Notification Message {message.Type} Source {message.Source} Status {message.Status}");

            if (message.Type == FieldMessageType.CalibrateAxis)
            {
                if (message.Status == MessageStatus.OperationEnd)
                {
                    this.numberOfExecutedSteps++;
                    this.currentAxis = (this.currentAxis == Axis.Vertical) ? Axis.Horizontal : Axis.Vertical;
                }
            }

            lock (this.CurrentState)
            {
                this.CurrentState.ProcessFieldNotificationMessage(message);
            }
        }

        /// <inheritdoc/>
        public override void ProcessNotificationMessage(NotificationMessage message)
        {
            this.logger.LogTrace($"1:Process Notification Message {message.Type} Source {message.Source} Status {message.Status}");

            lock (this.CurrentState)
            {
                this.CurrentState.ProcessNotificationMessage(message);
            }
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
            this.logger.LogTrace("1:Method Start");
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

            lock (this.CurrentState)
            {
                this.CurrentState = new HomingStartState(this, this.currentAxis, this.logger);
                this.CurrentState?.Start();
            }

            this.logger.LogTrace($"2:CurrentState{this.CurrentState.GetType()}");
        }

        public override void Stop()
        {
            this.logger.LogTrace("1:Method Start");

            lock (this.CurrentState)
            {
                this.CurrentState.Stop();
            }
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

        #endregion
    }
}
