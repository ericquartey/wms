using System;
using System.Diagnostics.Contracts;
using System.Threading.Tasks;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Microsoft.Extensions.Logging;

namespace Ferretto.VW.MAS.MissionManager
{
    internal sealed partial class MissionSchedulingService
    {
        #region Methods

        protected override bool FilterNotification(NotificationMessage notification)
        {
            Contract.Requires(notification != null);

            return
                notification.Destination is MessageActor.Any
                ||
                notification.Destination is MessageActor.MissionManager
                ||
                notification.Destination is MessageActor.AutomationService;
        }

        protected override async Task OnNotificationReceivedAsync(NotificationMessage message, IServiceProvider serviceProvider)
        {
            Contract.Requires(message != null);

            switch (message.Type)
            {
                case MessageType.MissionOperationCompleted:
                    await this.OnOperationComplete(message.Data as MissionOperationCompletedMessageData, serviceProvider);
                    break;

                case MessageType.BayOperationalStatusChanged:
                    await this.OnBayOperationalStatusChangedAsync(serviceProvider);
                    break;

                case MessageType.Homing:
                    await this.OnHoming(message, serviceProvider);
                    break;

                case MessageType.NewMachineMissionAvailable:
                    await this.OnNewMachineMissionAvailableAsync(serviceProvider);
                    break;

                case MessageType.MachineMode:
                    await this.OnMachineModeChangedAsync(serviceProvider);
                    break;

                case MessageType.MoveLoadingUnit when message.Status is MessageStatus.OperationEnd || message.Status is MessageStatus.OperationWaitResume:
                    await this.OnLoadingUnitMovedAsync(message, serviceProvider);
                    break;

                case MessageType.DataLayerReady:
                    await this.OnDataLayerReadyAsync(serviceProvider);
                    break;

                case MessageType.ShutterPositioning:
                    await this.OnShutterPositioning(message, serviceProvider);
                    break;
            }
        }

        #endregion
    }
}
