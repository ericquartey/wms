using Microsoft.Extensions.Logging;

namespace Ferretto.VW.MAS.IODriver.StateMachines.SetConfiguration
{
    internal sealed class SetConfigurationErrorState : IoStateBase
    {
        #region Fields

        private readonly IoStatus status;

        #endregion

        #region Constructors

        public SetConfigurationErrorState(
            IIoStateMachine parentStateMachine,
            IoStatus status,
            ILogger logger)
            : base(parentStateMachine, logger)
        {
            this.status = status;

            logger.LogTrace("1:Method Start");
        }

        #endregion

        #region Methods

        public override void ProcessResponseMessage(IoReadMessage message)
        {
            this.Logger.LogTrace($"1: Received Message = {message.ToString()}");
        }

        public override void Start()
        {
            this.Logger.LogTrace("1:Method Start");
        }

        #endregion
    }
}
