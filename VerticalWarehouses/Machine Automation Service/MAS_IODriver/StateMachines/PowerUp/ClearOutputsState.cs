using Microsoft.Extensions.Logging;

namespace Ferretto.VW.MAS_IODriver.StateMachines.PowerUp
{
    public class ClearOutputsState : IoStateBase
    {
        #region Fields

        private readonly ILogger logger;

        #endregion

        #region Constructors

        public ClearOutputsState(IIoStateMachine parentStateMachine, ILogger logger)
        {
            this.parentStateMachine = parentStateMachine;
            this.logger = logger;
            var clearIoMessage = new IoMessage(false);
            clearIoMessage.Force = true;

            parentStateMachine.EnqueueMessage(clearIoMessage);
        }

        #endregion

        #region Methods

        public override void ProcessMessage(IoMessage message)
        {
            if (message.ValidOutputs && message.OutputsCleared)
            {
                this.parentStateMachine.ChangeState(new PulseResetState(this.parentStateMachine, this.logger));
            }
        }

        #endregion
    }
}
