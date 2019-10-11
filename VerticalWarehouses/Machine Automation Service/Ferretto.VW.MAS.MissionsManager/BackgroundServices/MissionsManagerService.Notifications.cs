using System;
using System.Linq;
using System.Threading.Tasks;
using Ferretto.VW.CommonUtils;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;
using Ferretto.VW.MAS.DataLayer;
using Ferretto.VW.MAS.Utils.Enumerations;
using Ferretto.VW.MAS.Utils.Messages;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS.MissionsManager.BackgroundServices
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

                case MessageType.FaultStateChanged:
                case MessageType.RunningStateChanged:
                    this.OnMachineRunningStatusChange(message);
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

        private void OnMachineRunningStatusChange(NotificationMessage message)
        {
            if (message is null)
            {
                return;
            }

            if (message.Data is IStateChangedMessageData messageData)
            {
                StopRequestReason reason = StopRequestReason.NoReason;

                if (message.Type == MessageType.FaultStateChanged && messageData.CurrentState)
                {
                    reason = StopRequestReason.FaultStateChanged;
                }
                else if (message.Type == MessageType.RunningStateChanged && !messageData.CurrentState)
                {
                    reason = StopRequestReason.RunningStateChanged;
                }

                if (reason != StopRequestReason.NoReason)
                {
                    var newMessageData = new ChangeRunningStateMessageData(false, CommandAction.Start, reason);
                    var command = new CommandMessage(
                        newMessageData,
                        message.Description,
                        message.Destination,
                        message.Source,
                        MessageType.ChangeRunningState,
                        message.RequestingBay);

                    if (this.missionsProvider.TryCreateMachineMission(MissionType.ChangeRunningType, command, out var missionId))
                    {
                        this.missionsProvider.StartMachineMission(missionId, command, null);
                    }
                    else
                    {
                        this.Logger.LogDebug("Failed to create Change Running State machine mission");
                        this.NotifyCommandError(command);
                    }
                }
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
