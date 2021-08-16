using System;
using System.Diagnostics.Contracts;
using System.Threading.Tasks;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;
using Ferretto.VW.MAS.DataLayer;

namespace Ferretto.VW.MAS.AutomationService
{
    public partial class NotificationTelemetryService
    {
        #region Methods

        protected override bool FilterNotification(NotificationMessage notification)
        {
            Contract.Requires(notification != null);

            return
                notification.Destination is MessageActor.WebApi
                ||
                notification.Destination is MessageActor.AutomationService
                ||
                notification.Destination is MessageActor.Any;
        }

        protected override async Task OnNotificationReceivedAsync(NotificationMessage message, IServiceProvider serviceProvider)
        {
            Contract.Requires(message != null);

            switch (message.Type)
            {
                case MessageType.DataLayerReady:
                    await this.OnDataLayerReadyAsync();
                    break;

                case MessageType.ErrorStatusChanged when message.Data is ErrorStatusMessageData messsageData:
                    await this.OnErrorStatusChangedAsync(messsageData, serviceProvider);
                    break;

                case MessageType.MachineMode when message.Data is MachineModeMessageData messsageData:
                    await this.OnMachineModeChangedAsync(messsageData);
                    break;

                case MessageType.MachineStateActive when message.Data is MachineStateActiveMessageData messsageData:
                    if (messsageData.CurrentState == "PowerUpStartState")
                    {
                        await this.OnMachineStatePowerUpStartAsync(messsageData);
                    }
                    break;

                case MessageType.MoveLoadingUnit when message.Data is MoveLoadingUnitMessageData:
                    await this.OnMoveLoadingUnitAsync(message);
                    break;

                case MessageType.SensorsChanged when message.Data is SensorsChangedMessageData messageData:
                    await this.OnSensorsChanged(message, messageData);
                    break;

                case MessageType.ServicingSchedule when message.Data is ServicingScheduleMessageData messageData:
                    await this.OnServicingScheduleChangedAsync(messageData, serviceProvider);
                    break;
            }
        }

        #endregion
    }
}
