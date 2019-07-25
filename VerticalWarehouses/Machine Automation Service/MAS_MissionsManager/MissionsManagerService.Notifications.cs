using System;
using System.Linq;
using System.Threading.Tasks;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;
using Ferretto.WMS.Data.WebAPI.Contracts;
using Microsoft.Extensions.Logging;
using Prism.Events;

namespace Ferretto.VW.MAS.MissionsManager
{
    public partial class MissionsManagerService
    {
        #region Fields

        private readonly IEventAggregator eventAggregator;

        #endregion

        #region Methods

        protected override bool FilterNotification(NotificationMessage notification)
        {
            return
                notification.Destination == MessageActor.MissionsManager
                ||
                notification.Destination == MessageActor.Any;
        }

        protected override async Task OnNotificationReceivedAsync(NotificationMessage message)
        {
            switch (message.Type)
            {
                case MessageType.MissionOperationCompleted:
                    await this.OnMissionOperationCompleted(message.Data as IMissionOperationCompletedMessageData);
                    break;

                case MessageType.BayOperationalStatusChanged:
                    this.OnBayOperationalStatusChanged(message.Data as IBayOperationalStatusChangedMessageData);
                    break;

                case MessageType.NewMissionAvailable:
                    await this.OnNewMissionAvailable();
                    break;

                case MessageType.DataLayerReady:
                    await this.OnDataLayerReady();
                    break;
            }
        }

        private void OnBayOperationalStatusChanged(IBayOperationalStatusChangedMessageData e)
        {
            if (e == null)
            {
                throw new ArgumentNullException(nameof(e));
            }

            var bay = this.bays.SingleOrDefault(b => b.Id == e.BayId);
            if (bay != null)
            {
                bay.Status = e.BayStatus;

                this.Logger.LogDebug($"Bay #{bay.Id}: the bay is now {bay.Status}.");

                this.bayStatusChangedEvent.Set();
            }
        }

        private async Task OnDataLayerReady()
        {
            await this.RefreshPendingMissionsQueue();

            this.missionManagementTask.Start();
        }

        private async Task OnMissionOperationCompleted(IMissionOperationCompletedMessageData e)
        {
            if (e == null)
            {
                throw new ArgumentNullException(nameof(e));
            }

            var bay = this.bays
                .Where(b => b.CurrentMissionOperation != null)
                .SingleOrDefault(b => b.CurrentMissionOperation.Id == e.MissionOperationId);

            if (bay != null)
            {
                bay.Status = BayStatus.Idle;
                bay.CurrentMissionOperation.Status = MissionOperationStatus.Completed;
                bay.CurrentMissionOperation = null;
            }
            else
            {
                this.Logger.LogWarning($"No bay with mission operation id={e.MissionOperationId} was found.");
            }

            this.Logger.LogDebug($"MM NotificationCycle: Bay {bay.Id} status set to Available");

            await this.RefreshPendingMissionsQueue();

            this.bayStatusChangedEvent.Set();
        }

        private async Task OnNewMissionAvailable()
        {
            await this.RefreshPendingMissionsQueue();

            this.newMissionArrivedResetEvent.Set();
        }

        #endregion
    }
}
