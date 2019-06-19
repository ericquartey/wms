using Ferretto.VW.Common_Utils.Messages;
using Ferretto.VW.MAS_Utils.Messages;
using Microsoft.Extensions.Logging;

namespace Ferretto.VW.MAS_FiniteStateMachines
{
    public class EmptyState : StateBase
    {
        #region Fields

        private readonly ILogger logger;

        #endregion

        #region Constructors

        public EmptyState(ILogger logger)
        {
            logger.LogTrace("1:Method Start");

            this.logger = logger;
        }

        #endregion

        #region Methods

        public override void ProcessCommandMessage(CommandMessage message)
        {
            this.logger.LogTrace("1:Method Start");
        }

        public override void ProcessFieldNotificationMessage(FieldNotificationMessage message)
        {
            this.logger.LogTrace("1:Method Start");
        }

        public override void ProcessNotificationMessage(NotificationMessage message)
        {
            this.logger.LogTrace("1:Method Start");
        }

        public override void Start()
        {
            this.logger.LogTrace("1:Method Start");
        }

        public override void Stop()
        {
            this.logger.LogTrace("1:Method Start");
        }

        #endregion
    }
}
