using Ferretto.Common.EF;
using Ferretto.WMS.Scheduler.Core.Interfaces;

namespace Ferretto.WMS.Scheduler.Core.Providers
{
    internal class ItemSchedulerProvider : IItemSchedulerProvider
    {
        #region Fields

        private readonly DatabaseContext databaseContext;

        #endregion

        #region Constructors

        public ItemSchedulerProvider(DatabaseContext databaseContext)
        {
            this.databaseContext = databaseContext;
        }

        #endregion
    }
}
