using Ferretto.VW.Common_Utils.Enumerations;

namespace Ferretto.VW.MAS_InverterDriver.StateMachines.VerticalMovingDrawer
{
    public class SwitchOnState : InverterStateBase
    {
        #region Fields

        private readonly Axis movingDrawer;

        private readonly ushort parameterValue;

        #endregion

        #region Constructors

        public SwitchOnState(IInverterStateMachine parentStateMachine, Axis movingDrawer)
        {
            this.parentStateMachine = parentStateMachine;
            this.movingDrawer = movingDrawer;

            var inverterMessage = new InverterMessage(0x00, (short)InverterParameterId.ControlWordParam, this.parameterValue);

            parentStateMachine.EnqueueMessage(inverterMessage);
        }

        #endregion

        #region Methods

        public override void ProcessMessage(InverterMessage message)
        {
            if (message.IsError)
                this.parentStateMachine.ChangeState(new ErrorState(this.parentStateMachine, this.movingDrawer));

            if (!message.IsWriteMessage && message.ParameterId == InverterParameterId.StatusWordParam)
                if (message.ShortPayload == this.parameterValue)
                    this.parentStateMachine.ChangeState(new EnableOperationState(this.parentStateMachine,
                        this.movingDrawer));
        }

        #endregion
    }
}
