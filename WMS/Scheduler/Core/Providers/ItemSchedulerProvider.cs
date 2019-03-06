using System.Linq;
using System.Threading.Tasks;
using Ferretto.Common.EF;
using Ferretto.WMS.Scheduler.Core.Interfaces;
using Ferretto.WMS.Scheduler.Core.Models;
using Microsoft.EntityFrameworkCore;

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

        #region Methods

        public async Task<Item> GetByIdAsync(int itemId)
        {
            return await this.databaseContext.Items
               .Select(i => new Item
               {
                   Id = i.Id,
                   ManagementType = (ItemManagementType)i.ManagementType,
               })
               .SingleAsync(i => i.Id == itemId);
        }

        #endregion
    }
}
