using System;
using System.Linq;
using System.Threading.Tasks;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.BusinessModels;
using Ferretto.Common.EF;
using Microsoft.EntityFrameworkCore;

namespace Ferretto.Common.BusinessProviders
{
    public class ItemListRowProvider : IItemListRowProvider
    {
        #region Fields

        private readonly IDatabaseContextService dataContextService;

        private readonly WMS.Data.WebAPI.Contracts.IItemListRowsDataService itemListRowDataService;

        private readonly IMaterialStatusProvider materialStatusProvider;

        private readonly IPackageTypeProvider packageTypeProvider;

        #endregion

        #region Constructors

        public ItemListRowProvider(
            IDatabaseContextService dataContextService,
            IMaterialStatusProvider materialStatusProvider,
            IPackageTypeProvider packageTypeProvider,
            WMS.Data.WebAPI.Contracts.IItemListRowsDataService itemListRowDataService)
        {
            this.dataContextService = dataContextService;
            this.itemListRowDataService = itemListRowDataService;
            this.packageTypeProvider = packageTypeProvider;
            this.materialStatusProvider = materialStatusProvider;
        }

        #endregion

        #region Methods

        public Task<IOperationResult<ItemListRowDetails>> AddAsync(ItemListRowDetails model) => throw new NotSupportedException();

        public Task<int> DeleteAsync(int id) => throw new NotSupportedException();

        public async Task<IOperationResult<ItemListRow>> ExecuteImmediatelyAsync(int listRowId, int areaId, int bayId)
        {
            try
            {
                await this.itemListRowDataService.ExecuteAsync(
                    new WMS.Data.WebAPI.Contracts.ListRowExecutionRequest
                    {
                        ListRowId = listRowId,
                        AreaId = areaId,
                        BayId = bayId
                    });

                return new OperationResult<ItemListRow>(true);
            }
            catch (Exception ex)
            {
                return new OperationResult<ItemListRow>(ex);
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

            itemListRowDetails.MaterialStatusChoices = await this.materialStatusProvider.GetAllAsync();
            itemListRowDetails.PackageTypeChoices = await this.packageTypeProvider.GetAllAsync();

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

        public ItemListRowDetails GetNew()
        {
            throw new NotImplementedException();
        }

        public async Task<IOperationResult<ItemListRowDetails>> SaveAsync(ItemListRowDetails model)
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

                    return new OperationResult<ItemListRowDetails>(changedEntityCount > 0);
                }
            }
            catch (Exception ex)
            {
                return new OperationResult<ItemListRowDetails>(ex);
            }
        }

        public async Task<IOperationResult<ItemListRow>> ScheduleForExecutionAsync(int listRowId, int areaId)
        {
            try
            {
                await this.itemListRowDataService
                    .ExecuteAsync(new WMS.Data.WebAPI.Contracts.ListRowExecutionRequest
                    {
                        ListRowId = listRowId,
                        AreaId = areaId
                    });

                return new OperationResult<ItemListRow>(true);
            }
            catch (Exception ex)
            {
                return new OperationResult<ItemListRow>(ex);
            }
        }

        #endregion
    }
}
