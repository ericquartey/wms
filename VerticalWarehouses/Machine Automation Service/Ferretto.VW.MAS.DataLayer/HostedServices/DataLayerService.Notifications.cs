using System;
using System.Threading.Tasks;
using Ferretto.VW.CommonUtils.Messages;
using Microsoft.Extensions.DependencyInjection;

namespace Ferretto.VW.MAS.DataLayer
{
    internal partial class DataLayerService
    {
        #region Methods

        protected override bool FilterNotification(NotificationMessage notification)
        {
            return true;
        }

        protected override Task OnNotificationReceivedAsync(NotificationMessage notification, IServiceProvider serviceProvider)
        {
            serviceProvider
                .GetRequiredService<ILogEntriesProvider>()
                .Add(notification);

            return Task.CompletedTask;
        }

        #endregion
    }
}
