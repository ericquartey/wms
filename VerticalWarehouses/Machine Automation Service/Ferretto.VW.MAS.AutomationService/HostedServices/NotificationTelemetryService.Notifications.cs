using System;
using System.Diagnostics.Contracts;
using System.Threading.Tasks;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;

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

                case MessageType.MoveLoadingUnit when message.Data is MoveLoadingUnitMessageData:
                    await this.OnMoveLoadingUnitAsync(message);
                    break;
            }
        }

        #endregion
    }
}
