using System;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Threading.Tasks;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataLayer;
using Ferretto.VW.MAS.Utils.Events;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Ferretto.VW.MAS.MissionManager
{
    internal sealed partial class WmsMissionProxyService
    {
        #region Methods

        protected override bool FilterNotification(NotificationMessage notification)
        {
            return
                notification.Destination is MessageActor.Any
                ||
                notification.Destination is MessageActor.MissionManager;
        }

        protected override async Task OnNotificationReceivedAsync(NotificationMessage message, IServiceProvider serviceProvider)
        {
            switch (message.Type)
            {
                case MessageType.MissionOperationCompleted:
                    await this.OnMissionOperationCompletedAsync();
                    break;

                case MessageType.BayOperationalStatusChanged:
                    await this.OnBayOperationalStatusChangedAsync();
                    break;

                case MessageType.NewWmsMissionAvailable:
                    await this.OnNewWmsMissionAvailable();
                    break;

                case MessageType.DataLayerReady:
                    await this.OnDataLayerReadyAsync();
                    break;
            }
        }

        private async Task OnBayOperationalStatusChangedAsync()
        {
            await this.RetrieveNewWmsMissionsAsync();
        }

        private async Task OnDataLayerReadyAsync()
        {
            this.dataLayerIsReady = true;
            await this.RetrieveNewWmsMissionsAsync();
        }

        private async Task OnMissionOperationCompletedAsync()
        {
            await this.RetrieveNewWmsMissionsAsync();
        }

        private async Task OnNewWmsMissionAvailable()
        {
            await this.RetrieveNewWmsMissionsAsync();
        }

        #endregion
    }
}
