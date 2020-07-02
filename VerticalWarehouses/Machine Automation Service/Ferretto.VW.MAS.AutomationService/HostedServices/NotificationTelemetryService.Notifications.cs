using System;
using System.Diagnostics.Contracts;
using System.Threading.Tasks;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;
using Ferretto.VW.MAS.DataLayer;
using Microsoft.Extensions.Logging;
using Ferretto.VW.CommonUtils.Messages.Data;

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

            if (message.ErrorLevel is ErrorLevel.Fatal)
            {
                this.Logger.LogCritical(message.Description);
            }

            switch (message.Type)
            {
                case MessageType.DataLayerReady:
                    await this.OnDataLayerReady(serviceProvider);
                    break;

                case MessageType.MoveLoadingUnit when message.Data is MoveLoadingUnitMessageData messsageData:
                    await this.OnMoveLoadingUnit(messsageData);
                    break;
            }
        }

        #endregion
    }
}
