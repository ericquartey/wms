using Ferretto.VW.Common_Utils.Enumerations;
using Ferretto.VW.Common_Utils.Messages;
using Ferretto.VW.Common_Utils.Messages.Data;
using Microsoft.Extensions.Logging;

namespace Ferretto.VW.MAS_IODriver.StateMachines.SwitchAxis
{
    public class SwitchOnMotorState : IoStateBase
    {
        #region Fields

        private Axis axisToSwitchOn;

        private ILogger logger;

        #endregion

        #region Constructors

        public SwitchOnMotorState(Axis axisToSwitchOn, ILogger logger, IIoStateMachine parentStateMachine)
        {
            logger.LogDebug("1:Method Start");

            this.axisToSwitchOn = axisToSwitchOn;
            this.parentStateMachine = parentStateMachine;
            this.logger = logger;

            var switchOnAxisIoMessage = new IoMessage(false);

            this.logger.LogTrace($"2:Switch on axis io={switchOnAxisIoMessage}");

            switch (axisToSwitchOn)
            {
                case Axis.Horizontal:
                    switchOnAxisIoMessage.SwitchCradleMotor(true);
                    break;

                case Axis.Vertical:
                    switchOnAxisIoMessage.SwitchElevatorMotor(true);
                    break;
            }

            parentStateMachine.EnqueueMessage(switchOnAxisIoMessage);

            this.logger.LogDebug("3:Method End");
        }

        #endregion

        #region Methods

        public override void ProcessMessage(IoMessage message)
        {
            this.logger.LogDebug("1:Method Start");

            if (message.ValidOutputs)
            {
                this.logger.LogTrace($"2:Axis to switch on={this.axisToSwitchOn}:Cradle motor on={message.CradleMotorOn}:Elevator motor on={message.ElevatorMotorOn}");

                if (this.axisToSwitchOn == Axis.Horizontal && message.CradleMotorOn || this.axisToSwitchOn == Axis.Vertical && message.ElevatorMotorOn)
                {
                    var messageData = new CalibrateAxisMessageData(this.axisToSwitchOn, MessageVerbosity.Info);
                    var notificationMessage = new NotificationMessage(
                        messageData,
                        $"Switch on {this.axisToSwitchOn} axis",
                        MessageActor.AutomationService,
                        MessageActor.IODriver,
                        MessageType.SwitchAxis,
                        MessageStatus.OperationEnd,
                        ErrorLevel.NoError,
                        MessageVerbosity.Info);
                    this.logger.LogTrace($"2-Notification published: {notificationMessage.Type}, {notificationMessage.Status}, {notificationMessage.Destination}");
                    this.parentStateMachine.PublishNotificationEvent(notificationMessage);
                    this.logger.LogTrace($"3-Change State to EndState");
                    this.parentStateMachine.ChangeState(new EndState(this.axisToSwitchOn, this.logger, this.parentStateMachine));
                }
            }

            this.logger.LogDebug("3:Method End");
        }

        #endregion
    }
}
