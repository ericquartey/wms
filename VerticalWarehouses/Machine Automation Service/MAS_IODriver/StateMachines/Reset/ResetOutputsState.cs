using Microsoft.Extensions.Logging;

namespace Ferretto.VW.MAS_IODriver.StateMachines.Reset
{
    public class ResetOutputsState : IoStateBase
    {
        #region Fields

        private readonly ILogger logger;

        #endregion

        #region Constructors

        public ResetOutputsState(IIoStateMachine parentStateMachine, ILogger logger)
        {
            this.parentStateMachine = parentStateMachine;
            this.logger = logger;
            var resetIoMessage = new IoMessage(false);
            resetIoMessage.Force = true;

            parentStateMachine.EnqueueMessage(resetIoMessage);
        }

        #endregion

        #region Methods

        public override void ProcessMessage(IoMessage message)
        {
            if (message.ValidOutputs && message.OutputsCleared)
            {
                this.parentStateMachine.ChangeState(new EndState(this.parentStateMachine, this.logger));
            }
        }

        #endregion
    }
}
