using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;
using Ferretto.VW.MAS.FiniteStateMachines.Homing.Models;
using Ferretto.VW.MAS.Utils.Enumerations;
using Ferretto.VW.MAS.Utils.Messages;
using Ferretto.VW.MAS.Utils.Messages.FieldData;
using Microsoft.Extensions.DependencyInjection;
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

        private bool disposed;

        private HomingOperation homingOperation;

        #endregion

        #region Constructors

        public HomingStateMachine(
            IEventAggregator eventAggregator,
            IHomingMessageData calibrateMessageData,
            ILogger logger,
            IServiceScopeFactory serviceScopeFactory)
            : base(eventAggregator, logger, serviceScopeFactory)
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
                if (message.Status == MessageStatus.OperationExecuting)
                {
                    var notificationMessageData = new CalibrateAxisMessageData(this.homingOperation.AxisToCalibrate, this.homingOperation.NumberOfExecutedSteps + 1, this.homingOperation.MaximumSteps, MessageVerbosity.Info);
                    var notificationMessage = new NotificationMessage(
                        notificationMessageData,
                        $"{this.homingOperation.AxisToCalibrate} axis calibration executing",
                        MessageActor.Any,
                        MessageActor.FiniteStateMachines,
                        MessageType.CalibrateAxis,
                        MessageStatus.OperationExecuting);

                    this.Logger.LogTrace($"2:Process Field Notification Message {notificationMessage.Type} Destination {notificationMessage.Destination} Status {notificationMessage.Status}");

                    this.PublishNotificationMessage(notificationMessage);
                }

                if (message.Status == MessageStatus.OperationEnd)
                {
                    this.homingOperation.NumberOfExecutedSteps++;
                    this.homingOperation.AxisToCalibrate =
                        (this.homingOperation.AxisToCalibrate == Axis.Vertical) ?
                            Axis.Horizontal :
                            Axis.Vertical;
                }
            }

            if (message.Type == FieldMessageType.InverterStatusUpdate &&
                message.Status == MessageStatus.OperationExecuting)
            {
                if (message.Data is InverterStatusUpdateFieldMessageData data)
                {
                    var notificationMessageData = new CurrentPositionMessageData(data.CurrentPosition);
                    var notificationMessage = new NotificationMessage(
                        notificationMessageData,
                        $"Current Encoder position: {data.CurrentPosition}",
                        MessageActor.Any,
                        MessageActor.FiniteStateMachines,
                        MessageType.CurrentPosition,
                        MessageStatus.OperationExecuting);

                    this.PublishNotificationMessage(notificationMessage);
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
                    this.homingOperation = new HomingOperation(Axis.Horizontal, 0, 3);
                    break;

                case Axis.Horizontal:
                    this.homingOperation = new HomingOperation(Axis.Horizontal, 0, 1);
                    break;

                case Axis.Vertical:
                    this.homingOperation = new HomingOperation(Axis.Vertical, 0, 1);
                    break;
            }

            lock (this.CurrentState)
            {
                this.CurrentState = new HomingStartState(
                    this,
                    this.homingOperation,
                    this.logger);

                this.CurrentState.Start();
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
