using Ferretto.VW.Common_Utils.Enumerations;
using Ferretto.VW.Common_Utils.Messages;
using Ferretto.VW.Common_Utils.Messages.Data;
using Microsoft.Extensions.Logging;

namespace Ferretto.VW.MAS_IODriver.StateMachines.SwitchAxis
{
    public class SwitchOffMotorState : IoStateBase
    {
        #region Fields

        private readonly ILogger logger;

        private Axis axisToSwitchOn;

        #endregion

        #region Constructors

        /// <inheritdoc />
        public SwitchOffMotorState(Axis axisToSwitchOn, ILogger logger, IIoStateMachine parentStateMachine)
        {
            logger.LogDebug("1:Method Start");

            this.axisToSwitchOn = axisToSwitchOn;
            this.parentStateMachine = parentStateMachine;
            this.logger = logger;
            this.logger.LogTrace($"Constructor");

            var switchOffAxisIoMessage = new IoMessage(false);

            this.logger.LogTrace(string.Format("2:{0}", switchOffAxisIoMessage));

            switch (axisToSwitchOn)
            {
                case Axis.Horizontal:
                    switchOffAxisIoMessage.SwitchElevatorMotor(false);
                    break;

                case Axis.Vertical:
                    switchOffAxisIoMessage.SwitchCradleMotor(false);
                    break;
            }
            parentStateMachine.EnqueueMessage(switchOffAxisIoMessage);

            this.logger.LogDebug("3:Method End");
        }

        #endregion

        #region Methods

        public override void ProcessMessage(IoMessage message)
        {
            this.logger.LogDebug("1:Method Start");

            if (message.ValidOutputs)
            {
                this.logger.LogTrace(string.Format("2:{0}:{1}:{2}", this.axisToSwitchOn, message.CradleMotorOn, message.ElevatorMotorOn));

                if (this.axisToSwitchOn == Axis.Horizontal && message.CradleMotorOn || this.axisToSwitchOn == Axis.Vertical && message.ElevatorMotorOn)
                {
                    var messageData = new CalibrateAxisMessageData(this.axisToSwitchOn, MessageVerbosity.Info);
                    var notificationMessage = new NotificationMessage(
                        messageData,
                        $"Switch off {this.axisToSwitchOn} axis",
                        MessageActor.AutomationService,
                        MessageActor.IODriver,
                        MessageType.SwitchAxis,
                        MessageStatus.OperationEnd,
                        ErrorLevel.NoError,
                        MessageVerbosity.Info);
                    this.logger.LogTrace($"2-Notification published: {notificationMessage.Type}, {notificationMessage.Status}, {notificationMessage.Destination}");
                    this.parentStateMachine.PublishNotificationEvent(notificationMessage);
                    this.logger.LogTrace($"4-Change State to SwitchOnMotorState");
                    this.parentStateMachine.ChangeState(new SwitchOnMotorState(this.axisToSwitchOn, this.logger, this.parentStateMachine));
                }
            }

            this.logger.LogDebug("3:Method End");
        }

        #endregion
    }
}
