using Ferretto.Common.EF;
using Ferretto.WMS.Data.Core.Interfaces;

namespace Ferretto.WMS.Data.Core.Providers
{
    internal class BaseProvider
    {
        #region Constructors

        protected BaseProvider(DatabaseContext dataContext, INotificationService notificationService)
        {
            this.DataContext = dataContext;
            this.NotificationService = notificationService;
        }

        #endregion

        #region Properties

        public DatabaseContext DataContext { get; }

        public INotificationService NotificationService { get; }

        #endregion
    }
}
