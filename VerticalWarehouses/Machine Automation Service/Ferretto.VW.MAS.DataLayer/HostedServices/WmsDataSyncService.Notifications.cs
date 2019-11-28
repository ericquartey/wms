using System;
using System.Threading.Tasks;
using Ferretto.VW.CommonUtils.Messages;

namespace Ferretto.VW.MAS.DataLayer
{
    internal sealed partial class WmsDataSyncService
    {
        #region Methods

        protected override bool FilterNotification(NotificationMessage notification)
        {
            return !this.initialSyncPerformed;
        }

        protected override async Task OnNotificationReceivedAsync(NotificationMessage message, IServiceProvider serviceProvider)
        {
            if (message.Type is CommonUtils.Messages.Enumerations.MessageType.DataLayerReady)
            {
                await this.OnDataLayerReadyAsync();
            }
        }

        #endregion
    }
}
