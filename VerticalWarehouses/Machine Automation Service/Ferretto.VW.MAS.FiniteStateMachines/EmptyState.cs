using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.Utils.Messages;
using Microsoft.Extensions.Logging;
// ReSharper disable ArrangeThisQualifier

namespace Ferretto.VW.MAS.FiniteStateMachines
{
    public class EmptyState : StateBase
    {


        #region Constructors

        public EmptyState(ILogger<FiniteStateMachines> logger)
            : base(null, BayNumber.None, logger)
        {
            logger.LogTrace("1:Method Start");
        }

        #endregion



        #region Methods

        public override void ProcessCommandMessage(CommandMessage message)
        {
            this.Logger.LogTrace("1:Method Start");
        }

        public override void ProcessFieldNotificationMessage(FieldNotificationMessage message)
        {
            this.Logger.LogTrace("1:Method Start");
        }

        public override void ProcessNotificationMessage(NotificationMessage message)
        {
            this.Logger.LogTrace("1:Method Start");
        }

        public override void Start()
        {
            this.Logger.LogTrace("1:Method Start");
        }

        public override void Stop(StopRequestReason reason = StopRequestReason.Stop)
        {
            this.Logger.LogTrace("1:Method Start");
        }

        #endregion
    }
}
