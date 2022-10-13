using System;
using System.Threading.Tasks;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataLayer;
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
                notification.Destination is MessageActor.MissionManager
                ||
                notification.Destination is MessageActor.AutomationService;
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

                case MessageType.MachineMode:
                    await this.OnMachineModeChangedAsync();
                    break;

                case MessageType.NewWmsMissionAvailable:
                    await this.OnNewWmsMissionAvailable();
                    break;

                case MessageType.MoveLoadingUnit when message.Status is MessageStatus.OperationEnd:
                    await this.OnMissionOperationCompletedAsync();
                    break;

                case MessageType.DataLayerReady:
                    await this.OnDataLayerReadyAsync(serviceProvider);
                    break;

                case MessageType.WmsEnableChanged:
                    await this.OnWmsEnableChanged(serviceProvider);
                    break;

                case MessageType.SaveToWms:
                    await this.OnSaveToWms(message.Description, serviceProvider);
                    break;
            }
        }

        private async Task OnBayOperationalStatusChangedAsync()
        {
            await this.RetrieveNewWmsMissionsAsync();
        }

        private async Task OnDataLayerReadyAsync(IServiceProvider serviceProvider)
        {
            if (this.dataLayerIsReady)
            {
                return;
            }
            this.Logger.LogTrace("OnDataLayerReady start");
            this.dataLayerIsReady = true;

            await this.OnWmsEnableChanged(serviceProvider);

            this.RetrieveMachineId();

            await this.RetrieveNewWmsMissionsAsync();
            this.Logger.LogTrace("OnDataLayerReady end");
        }

        private async Task OnMachineModeChangedAsync()
        {
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

        private async Task OnSaveToWms(string loadUnitId, IServiceProvider serviceProvider)
        {
            var loadinUnitsDataProvider = serviceProvider.GetRequiredService<ILoadingUnitsDataProvider>();
            if (int.TryParse(loadUnitId, out var id))
            {
                await loadinUnitsDataProvider.SaveToWmsAsync(id);
            }
        }

        #endregion
    }
}
