using System;
using System.Linq;
using System.Threading.Tasks;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataLayer.Providers.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS.MissionsManager
{
    internal partial class MissionsManagerService
    {
        #region Methods

        protected override bool FilterNotification(NotificationMessage notification)
        {
            return
                notification.Destination == MessageActor.MissionsManager
                ||
                notification.Destination == MessageActor.Any;
        }

        protected override Task OnNotificationReceivedAsync(NotificationMessage message)
        {
            switch (message.Type)
            {
                case MessageType.MissionOperationCompleted:
                    this.OnMissionOperationCompleted(message.Data as MissionOperationCompletedMessageData);
                    break;

                case MessageType.BayOperationalStatusChanged:
                    this.OnBayOperationalStatusChanged();
                    break;

                case MessageType.NewMissionAvailable:
                    this.OnNewMissionAvailable();
                    break;

                case MessageType.DataLayerReady:
                    this.OnDataLayerReady();
                    break;
            }

            return Task.CompletedTask;
        }

        private void OnBayOperationalStatusChanged()
        {
            this.bayStatusChangedEvent.Set();
        }

        private void OnDataLayerReady()
        {
            this.missionManagementTask.Start();
        }

        private void OnMissionOperationCompleted(MissionOperationCompletedMessageData e)
        {
            if (e == null)
            {
                throw new ArgumentNullException(nameof(e));
            }

            using (var scope = this.serviceScopeFactory.CreateScope())
            {
                var bayProvider = scope.ServiceProvider.GetRequiredService<IBaysProvider>();

                var bay = bayProvider.GetAll()
                    .Where(b => b.CurrentMissionOperationId.HasValue && b.CurrentMissionId.HasValue)
                    .SingleOrDefault(b => b.CurrentMissionOperationId == e.MissionOperationId);

                if (bay != null)
                {
                    bayProvider.AssignMissionOperation(bay.Index, bay.CurrentMissionId.Value, null);
                    this.Logger.LogDebug($"Bay#{bay.Number}: operation competed.");

                    this.bayStatusChangedEvent.Set();
                }
                else
                {
                    this.Logger.LogWarning($"No bay with mission operation id={e.MissionOperationId} was found.");
                }
            }
        }

        private void OnNewMissionAvailable()
        {
            this.newMissionArrivedResetEvent.Set();
        }

        #endregion
    }
}
