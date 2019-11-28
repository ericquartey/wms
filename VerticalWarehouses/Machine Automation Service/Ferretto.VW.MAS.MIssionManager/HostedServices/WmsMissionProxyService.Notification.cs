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
            return true;
        }

        protected override async Task OnNotificationReceivedAsync(NotificationMessage message, IServiceProvider serviceProvider)
        {
            switch (message.Type)
            {
                case MessageType.MissionOperationCompleted:
                    // this will be handled by the MissionSchedulingService
                    await this.MOCK_OnWmsMissionOperationCompletedAsync(message.Data as MissionOperationCompletedMessageData);
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

        /// <summary>
        /// MOCK questo va fatto dal MissionSchedulingService (c'è già una copia di questo metodo laggiù)
        /// </summary>
        private void MOCK_NotifyAssignedMissionOperationChanged(
           BayNumber bayNumber,
           int? missionId,
           int? missionOperationId,
           int pendingMissionsCount)
        {
            var data = new AssignedMissionOperationChangedMessageData
            {
                BayNumber = bayNumber,
                MissionId = missionId,
                MissionOperationId = missionOperationId,
                PendingMissionsCount = pendingMissionsCount,
            };

            var notificationMessage = new NotificationMessage(
                data,
                $"Mission operation assigned to bay {bayNumber} has changed.",
                MessageActor.Any,
                MessageActor.MachineManager,
                MessageType.AssignedMissionOperationChanged,
                bayNumber);

            this.EventAggregator
                .GetEvent<NotificationEvent>()
                .Publish(notificationMessage);
        }

        /// <summary>
        /// HACK this method is used for mock purposes only. it will be removed
        /// </summary>
        private async Task MOCK_OnWmsMissionOperationCompletedAsync(MissionOperationCompletedMessageData data)
        {
            if (!this.configuration.IsWmsEnabled() || !this.dataLayerIsReady)
            {
                return;
            }

            // se sono qui è perché è già stato segnalato il completamento al WMS

            Contract.Requires(data != null);

            using (var scope = this.ServiceScopeFactory.CreateScope())
            {
                var bayProvider = scope.ServiceProvider.GetRequiredService<IBaysDataProvider>();

                // seleziono la baia sulla quale è in esecuzione l'operazione
                var bay = bayProvider
                    .GetAll()
                    .Where(b => b.CurrentWmsMissionOperationId.HasValue && b.CurrentMissionId.HasValue)
                    .SingleOrDefault(b => b.CurrentWmsMissionOperationId == data.MissionOperationId);

                if (bay is null)
                {
                    this.Logger.LogWarning($"None of the bays is currently executing operation id={data.MissionOperationId}.");
                }
                else
                {
                    // rimuovo l'assegnazione dell'operazione dalla baia
                    bayProvider.AssignWmsMission(bay.Number, bay.CurrentMissionId.Value, null);

                    // notifico la UI
                    var missionsDataProvider = scope.ServiceProvider.GetRequiredService<IMissionsDataProvider>();
                    var activeMissions = missionsDataProvider.GetAllActiveMissionsByBay(bay.Number);
                    this.MOCK_NotifyAssignedMissionOperationChanged(bay.Number, null, null, activeMissions.Count());

                    this.Logger.LogDebug($"Bay {bay.Number}: operation id={data.MissionOperationId} competed.");

                    // faccio ripartire il giro
                    var missionSchedulingProvider = scope.ServiceProvider.GetRequiredService<IMissionSchedulingProvider>();
                    await (missionSchedulingProvider as MockedMissionSchedulingProvider)?.MOCK_ScheduleMissionsAsync(bay.Number);
                }
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

        private async Task OnNewWmsMissionAvailable()
        {
            await this.RetrieveNewWmsMissionsAsync();
        }

        #endregion
    }
}
