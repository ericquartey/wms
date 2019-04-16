using System.Threading;
using Ferretto.VW.Common_Utils.Messages.Enumerations;
using Ferretto.VW.MAS_Utils.Enumerations;
using Ferretto.VW.MAS_Utils.Messages;
using Ferretto.VW.MAS_Utils.Messages.FieldData;
using Ferretto.VW.MAS_Utils.Utilities;
using Microsoft.Extensions.Logging;
using Prism.Events;
// ReSharper disable ArrangeThisQualifier

namespace Ferretto.VW.MAS_IODriver.StateMachines.SwitchAxis
{
    public class SwitchAxisStateMachine : IoStateMachineBase
    {
        #region Fields

        private const int PAUSE_INTERVAL = 250;

        private readonly Axis axisToSwitchOn;

        private readonly bool switchOffOtherAxis;

        private Timer delayTimer;

        private bool disposed;

        #endregion

        #region Constructors

        public SwitchAxisStateMachine(Axis axisToSwitchOn, bool switchOffOtherAxis, BlockingConcurrentQueue<IoMessage> ioCommandQueue, IEventAggregator eventAggregator, ILogger logger)
        {
            logger.LogDebug("1:Method Start");

            this.axisToSwitchOn = axisToSwitchOn;
            this.switchOffOtherAxis = switchOffOtherAxis;
            this.IoCommandQueue = ioCommandQueue;
            this.EventAggregator = eventAggregator;
            this.Logger = logger;

            this.Logger.LogDebug("2:Method End");
        }

        #endregion

        #region Destructors

        ~SwitchAxisStateMachine()
        {
            this.Dispose(false);
        }

        #endregion

        #region Methods

        /// <inheritdoc/>
        public override void ProcessMessage(IoMessage message)
        {
            this.Logger.LogDebug("1:Method Start");

            if (message.ValidOutputs && !message.ElevatorMotorOn && !message.CradleMotorOn)
            {
                this.delayTimer = new Timer(this.DelayElapsed, null, PAUSE_INTERVAL, -1);    //VALUE -1 period means timer does not fire multiple times
            }

            this.Logger.LogTrace($"2:Valid Outputs={message.ValidOutputs}:Elevator motor on={message.ElevatorMotorOn}:Cradle motor on={message.CradleMotorOn}");

            base.ProcessMessage(message);
        }

        public override void Start()
        {
            this.Logger.LogDebug("1:Method Start");
            this.Logger.LogTrace($"2:Switch off other axis={this.switchOffOtherAxis}");

            if (this.switchOffOtherAxis)
            {
                this.Logger.LogTrace("3:Change State to SwitchOffMotorState");
                this.CurrentState = new SwitchOffMotorState(this.axisToSwitchOn, this.Logger, this);

                var messageData = new SwitchAxisFieldMessageData(this.axisToSwitchOn, MessageVerbosity.Info);
                var notificationMessage = new FieldNotificationMessage(
                    messageData,
                    $"Switch on {this.axisToSwitchOn} axis",
                    FieldMessageActor.Any,
                    FieldMessageActor.IoDriver,
                    FieldMessageType.SwitchAxis,
                    MessageStatus.OperationStart);
                this.Logger.LogTrace($"3:Start Notification published: {notificationMessage.Type}, {notificationMessage.Status}, {notificationMessage.Destination}");
                this.PublishNotificationEvent(notificationMessage);
            }
            else
            {
                this.Logger.LogTrace("4:Change State to SwitchOnMotorState");
                this.CurrentState = new SwitchOnMotorState(this.axisToSwitchOn, this.Logger, this);

                var messageData = new SwitchAxisFieldMessageData(this.axisToSwitchOn, MessageVerbosity.Info);
                var notificationMessage = new FieldNotificationMessage(
                    messageData,
                    $"Switch on {this.axisToSwitchOn} axis",
                    FieldMessageActor.Any,
                    FieldMessageActor.IoDriver,
                    FieldMessageType.SwitchAxis,
                    MessageStatus.OperationStart);
                this.Logger.LogTrace($"4:Start Notification published: {notificationMessage.Type}, {notificationMessage.Status}, {notificationMessage.Destination}");
                this.PublishNotificationEvent(notificationMessage);
            }

            this.Logger.LogDebug("5:End Start");
        }

        protected override void Dispose(bool disposing)
        {
            if (this.disposed)
            {
                return;
            }

            if (disposing)
            {
                this.delayTimer?.Dispose();
                this.CurrentState.Dispose();
            }

            this.disposed = true;

            base.Dispose(disposing);
        }

        private void DelayElapsed(object state)
        {
            this.Logger.LogTrace("1:Change State to SwitchOnMotorState");
            this.ChangeState(new SwitchOnMotorState(this.axisToSwitchOn, this.Logger, this));
        }

        #endregion
    }
}
