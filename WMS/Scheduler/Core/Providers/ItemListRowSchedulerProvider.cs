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

        public async Task<IOperationResult<SchedulerRequest>> PrepareForExecutionAsync(int id, int areaId, int? bayId)
        {
            await this.databaseContext.SaveChangesAsync();

            // TODO: implement method
            return new SuccessOperationResult<SchedulerRequest>(null);
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

        public async Task<IOperationResult<ItemListRow>> SuspendAsync(int id)
        {
            await this.GetByIdAsync(id);
            throw new System.NotImplementedException();
        }

        #endregion
    }
}
