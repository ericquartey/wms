using Ferretto.VW.Common_Utils.Messages.Interfaces;

namespace Ferretto.VW.MAS_InverterDriver.StateMachines.HorizontalMovingDrawer
{
    public class SetNewPositionState : InverterStateBase
    {
        #region Fields

        private readonly MovingDrawer movingDrawer;

        private readonly ushort parameterValue;

        #endregion

        #region Constructors

        public SetNewPositionState(IInverterStateMachine parentStateMachine, MovingDrawer movingDrawer)
        {
            this.parentStateMachine = parentStateMachine;
            this.movingDrawer = movingDrawer;

            var inverterMessage = new InverterMessage(0x00, (short)InverterParameterId.ControlWordParam, this.parameterValue);

            parentStateMachine.EnqueueMessage(inverterMessage);
        }

        #endregion

        #region Methods

        public override void NotifyMessage(InverterMessage message)
        {
            if (message.IsError)
                this.parentStateMachine.ChangeState(new ErrorState(this.parentStateMachine, this.movingDrawer));

            if (!message.IsWriteMessage && message.ParameterId == InverterParameterId.StatusWordParam)
                if (message.ShortPayload == this.parameterValue)
                    this.parentStateMachine.ChangeState(new EndState(this.parentStateMachine,
                        this.movingDrawer));
        }

        #endregion
    }
}
