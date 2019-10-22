using System;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.MAS.Utils;
using Ferretto.VW.MAS.Utils.Events;
using Ferretto.VW.MAS.Utils.Messages;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Prism.Events;

namespace Ferretto.VW.MAS.MissionManager.BackgroundService
{
    internal partial class MissionManagerService : AutomationBackgroundService<CommandMessage, NotificationMessage, CommandEvent, NotificationEvent>
    {
        #region Constructors

        public MissionManagerService(
            IEventAggregator eventAggregator,
            ILogger<MissionManagerService> logger,
            IServiceScopeFactory serviceScopeFactory)
            : base(eventAggregator, logger, serviceScopeFactory)
        {
        }

        #endregion

        #region Methods

        protected override void NotifyCommandError(CommandMessage notificationData)
        {
            throw new NotImplementedException();
        }

        protected override void NotifyError(NotificationMessage notificationData)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
