using Ferretto.VW.Common_Utils.Enumerations;

namespace Ferretto.VW.MAS_IODriver.StateMachines.SwitchAxis
{
    public class SwitchOnMotorState : IoStateBase
    {
        #region Fields

        private Axis axisToSwitchOn;

        #endregion

        #region Constructors

        public SwitchOnMotorState(Axis axisToSwitchOn, IIoStateMachine parentStateMachine)
        {
            this.axisToSwitchOn = axisToSwitchOn;
            this.parentStateMachine = parentStateMachine;

            var switchOnAxisIoMessage = new IoMessage(false);

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
        }

        #endregion

        #region Methods

        public override void ProcessMessage(IoMessage message)
        {
            if (message.ValidOutputs)
            {
                if (this.axisToSwitchOn == Axis.Horizontal && message.CradleMotorOn || this.axisToSwitchOn == Axis.Vertical && message.ElevatorMotorOn)
                {
                    this.parentStateMachine.ChangeState(new EndState(this.axisToSwitchOn, this.parentStateMachine));
                }
            }
        }

        #endregion
    }
}
