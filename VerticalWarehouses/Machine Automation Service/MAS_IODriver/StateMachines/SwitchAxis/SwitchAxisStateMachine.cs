using System.Threading;
using Ferretto.VW.Common_Utils.Enumerations;
using Ferretto.VW.Common_Utils.Messages;
using Ferretto.VW.Common_Utils.Messages.Data;
using Ferretto.VW.Common_Utils.Utilities;
using Microsoft.Extensions.Logging;
using Prism.Events;

namespace Ferretto.VW.MAS_IODriver.StateMachines.SwitchAxis
{
    public class SwitchAxisStateMachine : IoStateMachineBase
    {
        #region Fields

        private const int PauseInterval = 250;

        private readonly Axis axisToSwitchOn;

        private Timer delayTimer;

        private bool disposed;

        private bool switchOffOtherAxis;

        #endregion

        #region Constructors

        public SwitchAxisStateMachine(Axis axisToSwitchOn, bool switchOffOtherAxis, BlockingConcurrentQueue<IoMessage> ioCommandQueue, IEventAggregator eventAggregator, ILogger logger)
        {
            logger.LogDebug("1:Method Start");

            this.axisToSwitchOn = axisToSwitchOn;
            this.switchOffOtherAxis = switchOffOtherAxis;
            this.ioCommandQueue = ioCommandQueue;
            this.eventAggregator = eventAggregator;
            this.logger = logger;

            this.logger.LogDebug("2:Method End");
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
            this.logger.LogDebug("1:Method Start");

            if (message.ValidOutputs && !message.ElevatorMotorOn && !message.CradleMotorOn)
            {
                this.delayTimer = new Timer(this.DelayElapsed, null, PauseInterval, -1);    //VALUE -1 period means timer does not fire multiple times
            }

            this.logger.LogTrace(string.Format("2:{0}:{1}:{2}", message.ValidOutputs, message.ElevatorMotorOn, message.CradleMotorOn));

            base.ProcessMessage(message);
        }

        public override void Start()
        {
            this.logger.LogDebug("1:Method Start");
            this.logger.LogTrace(string.Format("2:{ 0}", this.switchOffOtherAxis));

            if (this.switchOffOtherAxis)
            {
                var messageData = new CalibrateAxisMessageData(this.axisToSwitchOn, MessageVerbosity.Info);
                var notificationMessage = new NotificationMessage(
                    messageData,
                    $"Switch off {this.axisToSwitchOn} axis",
                    MessageActor.AutomationService,
                    MessageActor.IODriver,
                    MessageType.SwitchAxis,
                    MessageStatus.OperationStart,
                    ErrorLevel.NoError,
                    MessageVerbosity.Info);
                this.PublishNotificationEvent(notificationMessage);

                this.CurrentState = new SwitchOffMotorState(this.axisToSwitchOn, this.logger, this);
            }
            else
            {
                var messageData = new CalibrateAxisMessageData(this.axisToSwitchOn, MessageVerbosity.Info);
                var notificationMessage = new NotificationMessage(
                    messageData,
                    $"Switch on {this.axisToSwitchOn} axis",
                    MessageActor.AutomationService,
                    MessageActor.IODriver,
                    MessageType.SwitchAxis,
                    MessageStatus.OperationStart,
                    ErrorLevel.NoError,
                    MessageVerbosity.Info);
                this.PublishNotificationEvent(notificationMessage);

                this.CurrentState = new SwitchOnMotorState(this.axisToSwitchOn, this.logger, this);
            }

            this.logger.LogDebug("3:End Start");
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
            this.ChangeState(new SwitchOnMotorState(this.axisToSwitchOn, this.logger, this));
        }

        #endregion
    }
}
