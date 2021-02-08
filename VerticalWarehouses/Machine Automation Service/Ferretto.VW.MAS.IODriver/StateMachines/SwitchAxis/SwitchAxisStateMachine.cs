using System.Threading;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS.Utils.Enumerations;
using Ferretto.VW.MAS.Utils.Messages;
using Ferretto.VW.MAS.Utils.Messages.FieldData;
using Ferretto.VW.MAS.Utils.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Prism.Events;

namespace Ferretto.VW.MAS.IODriver.StateMachines.SwitchAxis
{
    internal sealed class SwitchAxisStateMachine : IoStateMachineBase
    {
        #region Fields

        private const int PauseInterval = 250;

        private readonly Axis axisToSwitchOn;

        private readonly IoIndex index;

        private readonly IoStatus status;

        private readonly bool switchOffOtherAxis;

        private Timer delayTimer;

        private bool isDisposed;

        private bool pulseOneTime;

        #endregion

        #region Constructors

        public SwitchAxisStateMachine(
            Axis axisToSwitchOn,
            bool switchOffOtherAxis,
            BlockingConcurrentQueue<IoWriteMessage> ioCommandQueue,
            IoStatus status,
            IoIndex index,
            IEventAggregator eventAggregator,
            ILogger logger,
            IServiceScopeFactory serviceScopeFactory)
            : base(eventAggregator, logger, ioCommandQueue, serviceScopeFactory)
        {
            this.axisToSwitchOn = axisToSwitchOn;
            this.switchOffOtherAxis = switchOffOtherAxis;
            this.status = status;
            this.pulseOneTime = false;
            this.index = index;

            this.Logger.LogTrace("1:Method Start");
        }

        #endregion

        #region Methods

        public override void ProcessResponseMessage(IoReadMessage message)
        {
            var checkMessage = message.FormatDataOperation == ShdFormatDataOperation.Data &&
                               message.ValidOutputs && !message.ElevatorMotorOn && !message.CradleMotorOn;

            if (checkMessage && !this.pulseOneTime)
            {
                this.delayTimer = new Timer(this.DelayElapsed, null, PauseInterval, Timeout.Infinite);
                this.pulseOneTime = true;
            }

            this.Logger.LogTrace($"1:Valid Outputs={message.ValidOutputs}:Elevator motor on={message.ElevatorMotorOn}:Cradle motor on={message.CradleMotorOn}");

            base.ProcessResponseMessage(message);
        }

        public override void Start()
        {
            this.pulseOneTime = false;
            this.Logger.LogTrace($"1:Switch off other axis={this.switchOffOtherAxis}");

            if (this.switchOffOtherAxis)
            {
                var messageData = new SwitchAxisFieldMessageData(this.axisToSwitchOn, MessageVerbosity.Info);
                var notificationMessage = new FieldNotificationMessage(
                    messageData,
                    $"Switch on {this.axisToSwitchOn} axis",
                    FieldMessageActor.Any,
                    FieldMessageActor.IoDriver,
                    FieldMessageType.SwitchAxis,
                    MessageStatus.OperationStart,
                    (byte)this.index);
                this.Logger.LogTrace($"3:Start Notification published: {notificationMessage.Type}, {notificationMessage.Status}, {notificationMessage.Destination}");
                this.PublishNotificationEvent(notificationMessage);

                this.ChangeState(new SwitchAxisStartState(this.axisToSwitchOn, this.status, this.index, this.Logger, this));
            }
            else
            {
                var messageData = new SwitchAxisFieldMessageData(this.axisToSwitchOn, MessageVerbosity.Info);
                var notificationMessage = new FieldNotificationMessage(
                    messageData,
                    $"Switch on {this.axisToSwitchOn} axis",
                    FieldMessageActor.Any,
                    FieldMessageActor.IoDriver,
                    FieldMessageType.SwitchAxis,
                    MessageStatus.OperationStart,
                    (byte)this.index);
                this.Logger.LogTrace($"5:Start Notification published: {notificationMessage.Type}, {notificationMessage.Status}, {notificationMessage.Destination}");
                this.PublishNotificationEvent(notificationMessage);

                this.ChangeState(new SwitchAxisSwitchOnMotorState(this.axisToSwitchOn, this.status, this.index, this.Logger, this));
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (this.isDisposed)
            {
                return;
            }

            if (disposing)
            {
                this.delayTimer?.Dispose();

                if (this.CurrentState is System.IDisposable disposableState)
                {
                    disposableState.Dispose();
                }
            }

            this.isDisposed = true;

            base.Dispose(disposing);
        }

        private void DelayElapsed(object state)
        {
            this.Logger.LogTrace("1:Change State to SwitchOnMotorState");
            this.ChangeState(new SwitchAxisSwitchOnMotorState(this.axisToSwitchOn, this.status, this.index, this.Logger, this));
        }

        #endregion
    }
}
