using System;
using System.Linq;
using System.Threading.Tasks;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataLayer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Ferretto.VW.MAS.MissionManager
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
            if (this.configuration.IsWmsEnabled())
            {
                this.missionManagementTask.Start();
            }
        }

        private void OnMissionOperationCompleted(MissionOperationCompletedMessageData e)
        {
            if (e == null)
            {
                throw new ArgumentNullException(nameof(e));
            }

            using (var scope = this.ServiceScopeFactory.CreateScope())
            {
                var bayProvider = scope.ServiceProvider.GetRequiredService<IBaysProvider>();

                var bay = bayProvider.GetAll()
                                     .Where(b => b.CurrentMissionOperationId.HasValue && b.CurrentMissionId.HasValue)
                                     .SingleOrDefault(b => b.CurrentMissionOperationId == e.MissionOperationId);

                if (bay != null && bay.CurrentMissionId != null)
                {
                    bayProvider.AssignMissionOperation(bay.Number, bay.CurrentMissionId.Value, null);

                    LoggerExtensions.LogDebug(this.Logger, $"Bay#{bay.Number}: operation competed.");

                    this.bayStatusChangedEvent.Set();
                }
                else
                {
                    LoggerExtensions.LogWarning(this.Logger, $"No bay with mission operation id={e.MissionOperationId} was found.");
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
