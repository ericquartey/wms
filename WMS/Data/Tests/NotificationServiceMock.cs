using System.Collections.Generic;
using System.Threading.Tasks;
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

        public ISet<Notification> SentNotifications { get; private set; } = new HashSet<Notification>();

        #endregion

        #region Methods

        public override Task SendNotificationsAsync()
        {
            foreach (var notification in this.Notifications)
            {
                this.SentNotifications.Add(notification);
            }

            this.Notifications.Clear();
            return Task.CompletedTask;
        }

        #endregion
    }
}
