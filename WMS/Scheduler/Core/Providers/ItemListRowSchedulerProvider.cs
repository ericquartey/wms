using System.Threading.Tasks;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.EF;
using Ferretto.WMS.Scheduler.Core.Interfaces;
using Ferretto.WMS.Scheduler.Core.Models;

namespace Ferretto.WMS.Scheduler.Core.Providers
{
    internal class ItemListRowSchedulerProvider : IItemListRowSchedulerProvider
    {
        #region Fields

        private readonly DatabaseContext databaseContext;

        #endregion

        #region Constructors

        public ItemListRowSchedulerProvider(DatabaseContext databaseContext)
        {
            this.databaseContext = databaseContext;
        }

        #endregion

        #region Methods

        public Task<IOperationResult<ItemListRow>> PrepareForExecutionAsync(ListRowExecutionRequest model)
        {
            throw new System.NotSupportedException();
        }

        public async Task<IOperationResult<ItemListRow>> UpdateAsync(ItemListRow model)
        {
            if (model == null)
            {
                throw new System.ArgumentNullException(nameof(model));
            }

            var existingRow = this.databaseContext.ItemListRows.Find(model.Id);
            this.databaseContext.Entry(existingRow).CurrentValues.SetValues(model);

            await this.databaseContext.SaveChangesAsync();

            return new SuccessOperationResult<ItemListRow>(model);
        }

        #endregion
    }
}
