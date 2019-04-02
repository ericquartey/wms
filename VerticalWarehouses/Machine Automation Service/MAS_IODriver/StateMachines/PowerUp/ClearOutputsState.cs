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
            logger.LogDebug("1:Method Start");

            this.logger = logger;
            this.parentStateMachine = parentStateMachine;
            var clearIoMessage = new IoMessage(false);
            clearIoMessage.Force = true;

            this.logger.LogTrace(string.Format("2:{0}", clearIoMessage));

            parentStateMachine.EnqueueMessage(clearIoMessage);

            this.logger.LogDebug("3:Method Start");
        }

        #endregion

        #region Methods

        public override void ProcessMessage(IoMessage message)
        {
            this.logger.LogDebug("1:Method Start");

            this.logger.LogTrace(string.Format("2:{0}:{1}", message.ValidOutputs, message.OutputsCleared));

            if (message.ValidOutputs && message.OutputsCleared)
            {
                this.parentStateMachine.ChangeState(new PulseResetState(this.parentStateMachine, this.logger));
            }

            this.logger.LogDebug("3:Method Start");
        }

        #endregion
    }
}
