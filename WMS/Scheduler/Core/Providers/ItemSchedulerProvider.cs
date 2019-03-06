using System.Linq;
using System.Threading.Tasks;
using Ferretto.Common.BLL.Interfaces;
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

        public async Task<Item> GetByIdAsync(int id)
        {
            return await this.databaseContext.Items
               .Select(i => new Item
               {
                   Id = i.Id,
                   ManagementType = (ItemManagementType)i.ManagementType,
                   LastPickDate = i.LastPickDate
               })
               .SingleAsync(i => i.Id == id);
        }

        public async Task<IOperationResult<Item>> UpdateAsync(Item model)
        {
            if (model == null)
            {
                throw new System.ArgumentNullException(nameof(model));
            }

            var existingModel = this.databaseContext.Items.Find(model.Id);
            this.databaseContext.Entry(existingModel).CurrentValues.SetValues(model);

            await this.databaseContext.SaveChangesAsync();

            return new SuccessOperationResult<Item>(model);
        }

        #endregion
    }
}
