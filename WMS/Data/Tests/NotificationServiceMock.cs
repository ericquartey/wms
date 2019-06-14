using System.Collections.Generic;
using Ferretto.WMS.Data.Core.Hubs;
using Ferretto.WMS.Data.Core.Providers;
using Ferretto.WMS.Data.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace Ferretto.WMS.Data.WebAPI.Tests
{
    public class NotificationServiceMock : NotificationService
    {
        #region Constructors

        public NotificationServiceMock(IHubContext<DataHub, IDataHub> hubContext)
            : base(hubContext)
        {
        }

        #endregion

        #region Properties

        public ISet<Notification> SentNotifications => this.Notifications;

        #endregion
    }
}
