using System;
using System.Linq;
using System.Threading.Tasks;
using Ferretto.Common.BusinessModels;
using Ferretto.Common.EF;
using Ferretto.WMS.Scheduler.WebAPI.Contracts;
using Microsoft.EntityFrameworkCore;

namespace Ferretto.Common.BusinessProviders
{
    public class ItemListRowProvider : IItemListRowProvider
    {
        #region Fields

        private readonly IDatabaseContextService dataContextService;

        private readonly EnumerationProvider enumerationProvider;

        private readonly IItemListRowsSchedulerService itemListRowsSchedulerService;

        #endregion Fields

        #region Constructors

        public ItemListRowProvider(
            IDatabaseContextService dataContextService,
            EnumerationProvider enumerationProvider,
            IItemListRowsSchedulerService itemListRowsSchedulerService)
        {
            this.dataContextService = dataContextService;
            this.enumerationProvider = enumerationProvider;
            this.itemListRowsSchedulerService = itemListRowsSchedulerService;
        }

        #endregion Constructors

        #region Methods

        public Task<OperationResult> AddAsync(ItemListRowDetails model) => throw new NotSupportedException();

        public Task<int> DeleteAsync(int id) => throw new NotSupportedException();

        public async Task<OperationResult> ExecuteImmediatelyAsync(int listRowId, int areaId, int bayId)
        {
            try
            {
                await this.itemListRowsSchedulerService.ExecuteAsync(
                    new ListRowExecutionRequest
                    {
                        ListRowId = listRowId,
                        AreaId = areaId,
                        BayId = bayId
                    });

                return new OperationResult(true);
            }
            catch (Exception ex)
            {
                return new OperationResult(ex);
            }
        }

        public IQueryable<BusinessModels.ItemListRow> GetAll() => throw new NotSupportedException();

        public int GetAllCount() => throw new NotSupportedException();

        public async Task<ItemListRowDetails> GetByIdAsync(int id)
        {
            var itemListRowDetails = await this.dataContextService.Current.ItemListRows
                .Include(lr => lr.ItemList)
                .Include(lr => lr.Item)
                .ThenInclude(i => i.MeasureUnit)
                .Where(lr => lr.Id == id)
                .Select(lr => new ItemListRowDetails
                {
                    Id = lr.Id,
                    Code = lr.Code,
                    RowPriority = lr.Priority,
                    ItemId = lr.Item.Id,
                    RequiredQuantity = lr.RequiredQuantity,
                    DispatchedQuantity = lr.DispatchedQuantity,
                    ItemListRowStatus = (BusinessModels.ItemListRowStatus)lr.Status,
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
                    MaterialStatusId = lr.MaterialStatusId,
                    ItemUnitMeasure = lr.Item.MeasureUnit.Description
                }).SingleAsync();

            itemListRowDetails.MaterialStatusChoices = this.enumerationProvider.GetAllMaterialStatuses();
            itemListRowDetails.PackageTypeChoices = this.enumerationProvider.GetAllPackageTypes();

            return itemListRowDetails;
        }

        public IQueryable<BusinessModels.ItemListRow> GetByItemListId(int id)
        {
            var itemListRows = this.dataContextService.Current.ItemListRows
                .Include(l => l.MaterialStatus)
                .Include(l => l.Item)
                .ThenInclude(i => i.MeasureUnit)
                .Where(l => l.ItemListId == id)
                .Select(l => new BusinessModels.ItemListRow
                {
                    Id = l.Id,
                    Code = l.Code,
                    RowPriority = l.Priority,
                    ItemDescription = l.Item.Description,
                    RequiredQuantity = l.RequiredQuantity,
                    DispatchedQuantity = l.DispatchedQuantity,
                    ItemListRowStatus = (BusinessModels.ItemListRowStatus)l.Status,
                    MaterialStatusDescription = l.MaterialStatus.Description,
                    CreationDate = l.CreationDate,
                    ItemUnitMeasure = l.Item.MeasureUnit.Description
                }).AsNoTracking();

            return itemListRows;
        }

        public async Task<OperationResult> SaveAsync(ItemListRowDetails model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            try
            {
                using (var dataContext = this.dataContextService.Current)
                {
                    var existingModel = dataContext.ItemListRows.Find(model.Id);

                    dataContext.Entry(existingModel).CurrentValues.SetValues(model);

                    var changedEntityCount = await dataContext.SaveChangesAsync();

                    return new OperationResult(changedEntityCount > 0);
                }
            }
            catch (Exception ex)
            {
                return new OperationResult(ex);
            }
        }

        public async Task<OperationResult> ScheduleForExecutionAsync(int listRowId, int areaId)
        {
            try
            {
                await this.itemListRowsSchedulerService
                    .ExecuteAsync(new ListRowExecutionRequest
                    {
                        ListRowId = listRowId,
                        AreaId = areaId
                    });

                return new OperationResult(true);
            }
            catch (Exception ex)
            {
                return new OperationResult(ex);
            }
        }

        #endregion Methods
    }
}
