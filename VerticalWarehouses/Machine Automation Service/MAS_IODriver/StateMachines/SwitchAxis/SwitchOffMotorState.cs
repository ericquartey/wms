using Ferretto.VW.Common_Utils.Enumerations;
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
                    this.parentStateMachine.ChangeState(new SwitchOnMotorState(this.axisToSwitchOn, this.logger, this.parentStateMachine));
                }
            }

            this.logger.LogDebug("3:Method End");
        }

        #endregion
    }
}
