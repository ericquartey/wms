using System;
using System.Diagnostics.Contracts;
using System.Threading.Tasks;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;

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
                notification.Destination is MessageActor.MissionManager;
        }

        protected override async Task OnNotificationReceivedAsync(NotificationMessage message, IServiceProvider serviceProvider)
        {
            Contract.Requires(message != null);

            switch (message.Type)
            {
                //case MessageType.MissionOperationCompleted:
                //    await this.OnOperationComplete(message.Data as MissionOperationCompletedMessageData);
                //    break;

                case MessageType.AssignedMissionOperationChanged when message.Data is AssignedMissionOperationChangedMessageData:
                    await this.OnOperationChangedAsync(message);
                    break;

                case MessageType.DataLayerReady:
                    this.dataLayerIsReady = true;
                    break;
            }
        }

        #endregion
    }
}
