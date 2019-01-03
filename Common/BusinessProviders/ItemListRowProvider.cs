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

        private readonly IDatabaseContextService dataContext;
        private readonly EnumerationProvider enumerationProvider;
        private readonly WMS.Scheduler.WebAPI.Contracts.IItemListRowsService itemListRowService;

        #endregion Fields

        #region Constructors

        public ItemListRowProvider(
            IDatabaseContextService dataContext,
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

        public async Task<OperationResult> ExecuteImmediately(int listRowId, int areaId, int bayId)
        {
            try
            {
                await this.itemListRowService.ExecuteAsync(new WMS.Scheduler.WebAPI.Contracts.ListRowExecutionRequest { ListRowId = listRowId, AreaId = areaId, BayId = bayId });

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

        public async Task<ItemListRowDetails> GetById(int id)
        {
            var itemListRowDetails = await this.dataContext.Current.ItemListRows
                .Include(lr => lr.ItemList)
                .Where(lr => lr.Id == id)
                .Select(lr => new ItemListRowDetails
                {
                    Id = lr.Id,
                    Code = lr.Code,
                    RowPriority = lr.Priority,
                    ItemId = lr.Item.Id,
                    RequiredQuantity = lr.RequiredQuantity,
                    DispatchedQuantity = lr.DispatchedQuantity,
                    ItemListRowStatus = (ItemListRowStatus)lr.Status,
                    ItemDescription = lr.Item.Description,
                    CreationDate = lr.CreationDate,
                    ItemListCode = lr.ItemList.Code,
                    ItemListDescription = lr.ItemList.Description,
                    ItemListType = (ItemListType)lr.ItemList.ItemListType,
                    ItemListStatus = (ItemListStatus)lr.ItemList.Status,
                    CompletionDate = lr.CompletionDate,
                    LastExecutionDate = lr.LastExecutionDate,
                    LastModificationDate = lr.LastModificationDate,
                    Lot = lr.Lot,
                    RegistrationNumber = lr.RegistrationNumber,
                    Sub1 = lr.Sub1,
                    Sub2 = lr.Sub2,
                    PackageTypeId = lr.PackageTypeId,
                    MaterialStatusId = lr.MaterialStatusId
                }).SingleAsync();

            itemListRowDetails.MaterialStatusChoices = this.enumerationProvider.GetAllMaterialStatuses();
            itemListRowDetails.PackageTypeChoices = this.enumerationProvider.GetAllPackageTypes();
            itemListRowDetails.ItemListTypeChoices = ((ItemListType[])
                Enum.GetValues(typeof(ItemListType)))
                .Select(i => new Enumeration((int)i, i.ToString())).ToList();

            return itemListRowDetails;
        }

        public IQueryable<ItemListRow> GetByItemListId(int id)
        {
            var itemListRows = this.dataContext.Current.ItemListRows
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

        public int Save(ItemListRowDetails model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }
            var dataContext = this.dataContext.Current;
            lock (dataContext)
            {
                var existingModel = dataContext.ItemListRows.Find(model.Id);

                dataContext.Entry(existingModel).CurrentValues.SetValues(model);

                return dataContext.SaveChanges();
            }
        }

        public async Task<OperationResult> ScheduleForExecution(int listRowId, int areaId)
        {
            try
            {
                await this.itemListRowService.ExecuteAsync(new WMS.Scheduler.WebAPI.Contracts.ListRowExecutionRequest { ListRowId = listRowId, AreaId = areaId });

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
