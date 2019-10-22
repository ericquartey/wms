using System;
using System.Threading.Tasks;
using Ferretto.VW.CommonUtils.Messages;

namespace Ferretto.VW.MAS.MissionManager.BackgroundService
{
    internal partial class MissionManagerService
    {
        #region Methods

        protected override bool FilterNotification(NotificationMessage notification)
        {
            return false;
        }

        protected override Task OnNotificationReceivedAsync(NotificationMessage message, IServiceProvider serviceProvider)
        {
            return Task.CompletedTask;
        }

        #endregion
    }
}
