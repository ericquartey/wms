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

            this.logger.LogTrace(string.Format("2:{0}", switchOnAxisIoMessage));

            switch (axisToSwitchOn)
            {
                case Axis.Horizontal:
                    switchOnAxisIoMessage.SwitchCradleMotor(true);
                    break;

                case Axis.Vertical:
                    switchOnAxisIoMessage.SwitchElevatorMotor(true);
                    break;
            }

            this.logger.LogTrace(string.Format("3:{0}", switchOnAxisIoMessage));

            parentStateMachine.EnqueueMessage(switchOnAxisIoMessage);

            this.logger.LogDebug("4:Method End");
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
                    this.parentStateMachine.ChangeState(new EndState(this.axisToSwitchOn, this.logger, this.parentStateMachine));
                }
            }

            this.logger.LogDebug("3:Method End");
        }

        #endregion
    }
}
