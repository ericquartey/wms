using Ferretto.VW.Common_Utils.Enumerations;

namespace Ferretto.VW.MAS_InverterDriver.StateMachines.HorizontalMovingDrawer
{
    public class OperationModeState : InverterStateBase
    {
        #region Fields

        private readonly Axis movingDrawer;

        private readonly ushort parameterValue;

        #endregion

        #region Constructors

        public OperationModeState(IInverterStateMachine parentStateMachine, Axis movingDrawer)
        {
            this.parentStateMachine = parentStateMachine;
            this.movingDrawer = movingDrawer;

            var inverterMessage = new InverterMessage(0x00, (short)InverterParameterId.ControlWordParam, this.parameterValue);

            parentStateMachine.EnqueueMessage(inverterMessage);
        }

        #endregion

        #region Methods

        public override bool ProcessMessage(InverterMessage message)
        {
            if (message.IsError)
                this.parentStateMachine.ChangeState(new ErrorState(this.parentStateMachine, this.movingDrawer));

            if (!message.IsWriteMessage && message.ParameterId == InverterParameterId.StatusWordParam)
                if (message.ShortPayload == this.parameterValue)
                    this.parentStateMachine.ChangeState(new ReadyToSwitchOnState(this.parentStateMachine,
                        this.movingDrawer));

            return false;
        }

        #endregion
    }
}
