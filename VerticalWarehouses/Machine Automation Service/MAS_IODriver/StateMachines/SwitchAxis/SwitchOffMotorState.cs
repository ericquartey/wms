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
            this.axisToSwitchOn = axisToSwitchOn;
            this.parentStateMachine = parentStateMachine;
            this.logger = logger;

            //TEMP this.logger?.LogTrace($"{DateTime.Now}: Thread:{Thread.CurrentThread.ManagedThreadId} - SwitchOffMotorState:Ctor");

            var switchOffAxisIoMessage = new IoMessage(false);

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
        }

        #endregion

        #region Methods

        public override void ProcessMessage(IoMessage message)
        {
            if (message.ValidOutputs)
            {
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
                    this.parentStateMachine.PublishNotificationEvent(notificationMessage);

                    this.parentStateMachine.ChangeState(new SwitchOnMotorState(this.axisToSwitchOn, this.logger, this.parentStateMachine));
                }
            }
        }

        #endregion
    }
}
