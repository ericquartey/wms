using Ferretto.VW.Common_Utils.Enumerations;
using Ferretto.VW.MAS_IODriver.StateMachines.PowerUp;

namespace Ferretto.VW.MAS_IODriver.StateMachines.SwitchAxis
{
    public class SwitchOffMotorState : IoStateBase
    {
        #region Fields

        private Axis axisToSwitchOn;

        #endregion

        #region Constructors

        /// <inheritdoc />
        public SwitchOffMotorState(Axis axisToSwitchOn, IIoStateMachine parentStateMachine)
        {
            this.axisToSwitchOn = axisToSwitchOn;

            this.parentStateMachine = parentStateMachine;
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
                    this.parentStateMachine.ChangeState(new SwitchOnMotorState(this.axisToSwitchOn, this.parentStateMachine));
                }
            }
        }

        #endregion
    }
}
