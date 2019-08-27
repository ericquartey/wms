using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.FiniteStateMachines.Homing.Models;
using Ferretto.VW.MAS.Utils.Enumerations;
using Ferretto.VW.MAS.Utils.Messages;
using Ferretto.VW.MAS.Utils.Messages.FieldData;
using Microsoft.Extensions.Logging;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS.FiniteStateMachines.Homing
{
    public class HomingStateMachine : StateMachineBase
    {

        #region Fields

        private readonly Axis axisToCalibrate;

        private readonly HomingOperation homingOperation;

        private bool disposed;

        #endregion

        #region Constructors

        public HomingStateMachine(
            Axis axixToCalibrate,
            HomingOperation homingOperation)
            : base(homingOperation.EventAggregator, homingOperation.Logger, homingOperation.ServiceScopeFactory)
        {
            this.axisToCalibrate = axixToCalibrate;

            this.homingOperation = homingOperation;

            this.CurrentState = new EmptyState(homingOperation.Logger);
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
            this.Logger.LogTrace($"1:Process Command Message {message.Type} Source {message.Source}");

            lock (this.CurrentState)
            {
                this.CurrentState.ProcessCommandMessage(message);
            }
        }

        public override void ProcessFieldNotificationMessage(FieldNotificationMessage message)
        {
            this.Logger.LogTrace($"1:Process Field Notification Message {message.Type} Source {message.Source} Status {message.Status}");

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
                        this.homingOperation.RequestingBay,
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
                        this.homingOperation.RequestingBay,
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
            this.Logger.LogTrace($"1:Process Notification Message {message.Type} Source {message.Source} Status {message.Status}");

            lock (this.CurrentState)
            {
                this.CurrentState.ProcessNotificationMessage(message);
            }
        }

        /// <inheritdoc/>
        public override void PublishNotificationMessage(NotificationMessage message)
        {
            this.Logger.LogTrace($"1:Publish Notification Message {message.Type} Source {message.Source} Status {message.Status}");

            base.PublishNotificationMessage(message);
        }

        /// <inheritdoc/>
        public override void Start()
        {
            this.Logger.LogTrace("1:Method Start");
            switch (this.axisToCalibrate)
            {
                case Axis.Both:
                    this.homingOperation.AxisToCalibrate = Axis.Horizontal;
                    this.homingOperation.NumberOfExecutedSteps = 0;
                    this.homingOperation.MaximumSteps = 3;
                    break;

                case Axis.Horizontal:
                    this.homingOperation.AxisToCalibrate = Axis.Horizontal;
                    this.homingOperation.NumberOfExecutedSteps = 0;
                    this.homingOperation.MaximumSteps = 1;
                    break;

                case Axis.Vertical:
                    this.homingOperation.AxisToCalibrate = Axis.Vertical;
                    this.homingOperation.NumberOfExecutedSteps = 0;
                    this.homingOperation.MaximumSteps = 1;
                    break;
            }

            lock (this.CurrentState)
            {
                this.CurrentState = new HomingStartState(this, this.homingOperation);

                this.CurrentState.Start();
            }

            this.Logger.LogTrace($"2:CurrentState{this.CurrentState.GetType()}");
        }

        public override void Stop()
        {
            this.Logger.LogTrace("1:Method Start");

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
