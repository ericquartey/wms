﻿using System.Threading;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.Utils.Enumerations;
using Ferretto.VW.MAS.Utils.Messages;
using Ferretto.VW.MAS.Utils.Messages.FieldData;
using Ferretto.VW.MAS.Utils.Utilities;
using Microsoft.Extensions.Logging;
using Prism.Events;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS.IODriver.StateMachines.SwitchAxis
{
    public class SwitchAxisStateMachine : IoStateMachineBase
    {

        #region Fields

        private const int PAUSE_INTERVAL = 250;

        private readonly Axis axisToSwitchOn;

        private readonly IoIndex index;

        private readonly IoStatus status;

        private readonly bool switchOffOtherAxis;

        private Timer delayTimer;

        private bool disposed;

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
            ILogger logger)
            : base(eventAggregator, logger)
        {
            this.axisToSwitchOn = axisToSwitchOn;
            this.switchOffOtherAxis = switchOffOtherAxis;
            this.IoCommandQueue = ioCommandQueue;
            this.status = status;
            this.pulseOneTime = false;
            this.index = index;

            this.Logger.LogTrace("1:Method Start");
        }

        #endregion

        #region Destructors

        ~SwitchAxisStateMachine()
        {
            this.Dispose(false);
        }

        #endregion



        #region Methods

        private void DelayElapsed(object state)
        {
            this.Logger.LogTrace("1:Change State to SwitchOnMotorState");
            this.ChangeState(new SwitchAxisSwitchOnMotorState(this.axisToSwitchOn, this.status, this.index, this.Logger, this));
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

        public override void ProcessMessage(IoMessage message)
        {
            if (message.ValidOutputs && !message.ElevatorMotorOn && !message.CradleMotorOn)
            {
                this.delayTimer = new Timer(this.DelayElapsed, null, PAUSE_INTERVAL, -1);    //VALUE -1 period means timer does not fire multiple times
            }

            this.Logger.LogTrace($"1:Valid Outputs={message.ValidOutputs}:Elevator motor on={message.ElevatorMotorOn}:Cradle motor on={message.CradleMotorOn}");

            base.ProcessMessage(message);
        }

        public override void ProcessResponseMessage(IoReadMessage message)
        {
            var checkMessage = message.FormatDataOperation == Enumerations.ShdFormatDataOperation.Data &&
                               message.ValidOutputs && !message.ElevatorMotorOn && !message.CradleMotorOn;

            if (checkMessage && !this.pulseOneTime)
            {
                this.delayTimer = new Timer(this.DelayElapsed, null, PAUSE_INTERVAL, -1);    //VALUE -1 period means timer does not fire multiple times
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
                this.Logger.LogTrace("2:Change State to SwitchOffMotorState");
                this.CurrentState = new SwitchAxisStartState(this.axisToSwitchOn, this.status, this.index, this.Logger, this);

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

                this.CurrentState?.Start();
            }
            else
            {
                this.Logger.LogTrace("4:Change State to SwitchOnMotorState");
                this.CurrentState = new SwitchAxisSwitchOnMotorState(this.axisToSwitchOn, this.status, this.index, this.Logger, this);

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

                this.CurrentState?.Start();
            }
        }

        #endregion
    }
}
