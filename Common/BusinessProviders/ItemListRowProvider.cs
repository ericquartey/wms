using System;
using System.Linq;
using System.Threading.Tasks;
using Ferretto.Common.BusinessModels;
using Ferretto.Common.EF;
using Microsoft.EntityFrameworkCore;

namespace Ferretto.Common.BusinessProviders
{
    public class ItemListRowProvider : IItemListRowProvider
    {
        #region Fields

        private readonly DatabaseContext dataContext;
        private readonly EnumerationProvider enumerationProvider;
        private readonly WMS.Scheduler.WebAPI.Contracts.IItemListRowsService itemListRowService;

        #endregion Fields

        #region Constructors

        public ItemListRowProvider(
            DatabaseContext dataContext,
            EnumerationProvider enumerationProvider,
            WMS.Scheduler.WebAPI.Contracts.IItemListRowsService itemListRowService)
        {
            this.dataContext = dataContext;
            this.enumerationProvider = enumerationProvider;
            this.itemListRowService = itemListRowService;
        }

        #endregion Constructors

        #region Methods

        public Task<OperationResult> Add(ItemListRowDetails model)
        {
            throw new NotImplementedException();
        }

        public int Delete(int id)
        {
            throw new NotImplementedException();
        }

        public async Task<OperationResult> ExecuteImmediately(int listId, int areaId, int bayId)
        {
            try
            {
                await this.itemListRowService.ExecuteAsync(new WMS.Scheduler.WebAPI.Contracts.ListRowExecutionRequest { ListId = listId, AreaId = areaId, BayId = bayId });

                return new OperationResult(true);
            }
            catch (Exception ex)
            {
                return new OperationResult(false, description: ex.Message);
            }
        }

        public IQueryable<ItemListRow> GetAll()
        {
            throw new NotImplementedException();
        }

        public int GetAllCount()
        {
            throw new NotImplementedException();
        }

        public Task<ItemListRowDetails> GetById(int id)
        {
            throw new NotImplementedException();
        }

        public IQueryable<ItemListRow> GetByItemListId(int id)
        {
            lock (this.dataContext)
            {
                var itemListRows = this.dataContext.ItemListRows
               .Include(l => l.MaterialStatus)
               .Include(l => l.Item)
               .Where(l => l.ItemListId == id)
               .Select(l => new ItemListRow
               {
                   Id = l.Id,
                   Code = l.Code,
                   RowPriority = l.Priority,
                   ItemDescription = l.Item.Description,
                   RequiredQuantity = l.RequiredQuantity,
                   DispatchedQuantity = l.DispatchedQuantity,
                   ItemListRowStatus = (ItemListRowStatus)l.Status,
                   MaterialStatusDescription = l.MaterialStatus.Description,
                   CreationDate = l.CreationDate
               }).AsNoTracking();

                return itemListRows;
            }
        }

        public int Save(ItemListRowDetails model)
        {
            throw new NotImplementedException();
        }

        public async Task<OperationResult> ScheduleForExecution(int listId, int areaId)
        {
            try
            {
                await this.itemListRowService.ExecuteAsync(new WMS.Scheduler.WebAPI.Contracts.ListRowExecutionRequest { ListId = listId, AreaId = areaId });

                return new OperationResult(true);
            }
            catch (Exception ex)
            {
                return new OperationResult(false, description: ex.Message);
            }
        }

        #endregion Methods
    }
}
