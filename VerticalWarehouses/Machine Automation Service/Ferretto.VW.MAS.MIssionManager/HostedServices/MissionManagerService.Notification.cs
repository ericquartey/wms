using System;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Threading.Tasks;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataLayer;
using Ferretto.WMS.Data.WebAPI.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Ferretto.VW.MAS.MissionManager
{
    internal partial class MissionManagerService
    {
        #region Methods

        protected override bool FilterNotification(NotificationMessage notification)
        {
            return true;
        }

        protected override Task OnNotificationReceivedAsync(NotificationMessage message, IServiceProvider serviceProvider)
        {
            switch (message.Type)
            {
                case MessageType.MissionOperationCompleted:
                    this.OnMissionOperationCompleted(message.Data as MissionOperationCompletedMessageData);
                    break;

                case MessageType.BayOperationalStatusChanged:
                    this.OnBayOperationalStatusChanged(message.Data as BayOperationalStatusChangedMessageData);
                    break;

                case MessageType.NewMissionAvailable:
                    this.OnNewMissionAvailable();
                    break;

                case MessageType.MachineMode:
                    this.OnMachineModeChanged();
                    break;

                case MessageType.DataLayerReady:
                    this.OnDataLayerReady();
                    break;
            }

            return Task.CompletedTask;
        }

        private void OnBayOperationalStatusChanged(BayOperationalStatusChangedMessageData data)
        {
            this.bayStatusChangedEvent.Set();
        }

        private void OnDataLayerReady()
        {
            if (this.configuration.IsWmsEnabled())
            {
                this.scheduleMissionsOnBaysTask.Start();
            }
        }

        private void OnEntityChanged(object sender, EntityChangedEventArgs e)
        {
            if (e.EntityType == nameof(MissionOperation))
            {
                this.OnNewMissionAvailable();
            }
        }

        private void OnMachineModeChanged()
        {
            this.bayStatusChangedEvent.Set();
        }

        private void OnMissionOperationCompleted(MissionOperationCompletedMessageData e)
        {
            Contract.Requires(e != null);

            using (var scope = this.ServiceScopeFactory.CreateScope())
            {
                var bayProvider = scope.ServiceProvider.GetRequiredService<IBaysProvider>();

                var bay = bayProvider
                    .GetAll()
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
            this.bayStatusChangedEvent.Set();
        }

        #endregion
    }
}
