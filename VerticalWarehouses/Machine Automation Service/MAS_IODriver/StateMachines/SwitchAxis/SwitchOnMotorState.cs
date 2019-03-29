using Ferretto.VW.Common_Utils.Enumerations;
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
                    this.parentStateMachine.ChangeState(new EndState(this.axisToSwitchOn, this.logger, this.parentStateMachine));
                }
            }

            this.logger.LogDebug("3:Method End");
        }

        #endregion
    }
}
