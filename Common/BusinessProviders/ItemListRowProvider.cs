using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.BusinessModels;
using Ferretto.Common.Utils.Expressions;

namespace Ferretto.Common.BusinessProviders
{
    public class ItemListRowProvider : IItemListRowProvider
    {
        #region Fields

        private readonly WMS.Data.WebAPI.Contracts.IItemListRowsDataService itemListRowsDataService;

        private readonly WMS.Data.WebAPI.Contracts.IItemListsDataService itemListsDataService;

        private readonly IMaterialStatusProvider materialStatusProvider;

        private readonly IPackageTypeProvider packageTypeProvider;

        #endregion

        #region Constructors

        public ItemListRowProvider(
            IMaterialStatusProvider materialStatusProvider,
            IPackageTypeProvider packageTypeProvider,
            WMS.Data.WebAPI.Contracts.IItemListRowsDataService itemListRowsDataService,
            WMS.Data.WebAPI.Contracts.IItemListsDataService itemListsDataService)
        {
            this.packageTypeProvider = packageTypeProvider;
            this.materialStatusProvider = materialStatusProvider;
            this.itemListRowsDataService = itemListRowsDataService;
            this.itemListsDataService = itemListsDataService;
        }

        #endregion

        #region Methods

        public async Task<IOperationResult<ItemListRowDetails>> CreateAsync(ItemListRowDetails model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            try
            {
                var itemListRow = await this.itemListRowsDataService.CreateAsync(new WMS.Data.WebAPI.Contracts.ItemListRowDetails
                {
                    Id = model.Id,
                    Code = model.Code,
                    RowPriority = model.RowPriority,
                    ItemId = model.ItemId,
                    RequestedQuantity = model.RequestedQuantity,
                    DispatchedQuantity = model.DispatchedQuantity,
                    ItemListRowStatus = (WMS.Data.WebAPI.Contracts.ItemListRowStatus)model.ItemListRowStatus,
                    ItemDescription = model.ItemDescription,
                    CreationDate = model.CreationDate,
                    ItemListCode = model.ItemListCode,
                    ItemListDescription = model.ItemListDescription,
                    ItemListType = (WMS.Data.WebAPI.Contracts.ItemListType)model.ItemListType,
                    CompletionDate = model.CompletionDate,
                    LastExecutionDate = model.LastExecutionDate,
                    LastModificationDate = model.LastModificationDate,
                    Lot = model.Lot,
                    RegistrationNumber = model.RegistrationNumber,
                    Sub1 = model.Sub1,
                    Sub2 = model.Sub2,
                    PackageTypeId = model.PackageTypeId,
                    MaterialStatusId = model.MaterialStatusId,
                    ItemUnitMeasure = model.ItemUnitMeasure,
                });

                model.Id = itemListRow.Id;

                return new OperationResult<ItemListRowDetails>(true);
            }
            catch (Exception ex)
            {
                return new OperationResult<ItemListRowDetails>(ex);
            }
        }

        public async Task<IOperationResult<ItemListRowDetails>> DeleteAsync(int id)
        {
            try
            {
                await this.itemListRowsDataService.DeleteAsync(id);

                return new OperationResult<ItemListRowDetails>(true);
            }
            catch (Exception ex)
            {
                return new OperationResult<ItemListRowDetails>(ex);
            }
        }

        public async Task<IOperationResult<ItemListRow>> ExecuteImmediatelyAsync(int listRowId, int areaId, int bayId)
        {
            try
            {
                await this.itemListRowsDataService.ExecuteAsync(
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

        public async Task<IEnumerable<BusinessModels.ItemListRow>> GetAllAsync(
            int skip,
            int take,
            IEnumerable<SortOption> orderBySortOptions = null,
            string whereString = null,
            string searchString = null)
        {
            return (await this.itemListRowsDataService.GetAllAsync(skip, take, whereString, orderBySortOptions.ToQueryString(), searchString))
                .Select(l => new BusinessModels.ItemListRow
                {
                    Id = l.Id,
                    Code = l.Code,
                    DispatchedQuantity = l.DispatchedQuantity,
                    ItemDescription = l.ItemDescription,
                    ItemListRowStatus = (BusinessModels.ItemListRowStatus)l.ItemListRowStatus,
                    ItemUnitMeasure = l.ItemUnitMeasure,
                    MaterialStatusDescription = l.MaterialStatusDescription,
                    RequestedQuantity = l.RequestedQuantity,
                    RowPriority = l.RowPriority,
                    CreationDate = l.CreationDate,
                    CanDelete = l.CanDelete,
                });
        }

        public async Task<int> GetAllCountAsync(string whereString = null, string searchString = null)
        {
            return await this.itemListRowsDataService.GetAllCountAsync(whereString, searchString);
        }

        public async Task<ItemListRowDetails> GetByIdAsync(int id)
        {
            var itemListRow = await this.itemListRowsDataService.GetByIdAsync(id);

            var materialStatusChoices = await this.materialStatusProvider.GetAllAsync();
            var packageTypeChoices = await this.packageTypeProvider.GetAllAsync();

            return new ItemListRowDetails
            {
                Id = itemListRow.Id,
                Code = itemListRow.Code,
                RowPriority = itemListRow.RowPriority,
                ItemId = itemListRow.ItemId,
                RequestedQuantity = itemListRow.RequestedQuantity,
                DispatchedQuantity = itemListRow.DispatchedQuantity,
                ItemListRowStatus = (BusinessModels.ItemListRowStatus)itemListRow.ItemListRowStatus,
                ItemDescription = itemListRow.ItemDescription,
                CreationDate = itemListRow.CreationDate,
                ItemListCode = itemListRow.ItemListCode,
                ItemListDescription = itemListRow.ItemListDescription,
                ItemListType = (ItemListType)itemListRow.ItemListType,
                CompletionDate = itemListRow.CompletionDate,
                LastExecutionDate = itemListRow.LastExecutionDate,
                LastModificationDate = itemListRow.LastModificationDate,
                Lot = itemListRow.Lot,
                RegistrationNumber = itemListRow.RegistrationNumber,
                Sub1 = itemListRow.Sub1,
                Sub2 = itemListRow.Sub2,
                PackageTypeId = itemListRow.PackageTypeId,
                MaterialStatusId = itemListRow.MaterialStatusId,
                ItemUnitMeasure = itemListRow.ItemUnitMeasure,
                MaterialStatusChoices = materialStatusChoices,
                PackageTypeChoices = packageTypeChoices,
                ItemListId = itemListRow.ItemListId,
                CanDelete = itemListRow.CanDelete,
            };
        }

        public async Task<IEnumerable<BusinessModels.ItemListRow>> GetByItemListIdAsync(int id)
        {
            return (await this.itemListsDataService.GetRowsAsync(id))
                .Select(l => new BusinessModels.ItemListRow
                {
                    Id = l.Id,
                    Code = l.Code,
                    RowPriority = l.RowPriority,
                    ItemDescription = l.ItemDescription,
                    RequestedQuantity = l.RequestedQuantity,
                    DispatchedQuantity = l.DispatchedQuantity,
                    ItemListRowStatus = (BusinessModels.ItemListRowStatus)l.ItemListRowStatus,
                    MaterialStatusDescription = l.MaterialStatusDescription,
                    CreationDate = l.CreationDate,
                    ItemUnitMeasure = l.ItemUnitMeasure,
                    CanDelete = l.CanDelete,
                });
        }

        public async Task<IEnumerable<object>> GetUniqueValuesAsync(string propertyName)
        {
            return await this.itemListRowsDataService.GetUniqueValuesAsync(propertyName);
        }

        public async Task<IOperationResult<ItemListRow>> ScheduleForExecutionAsync(int listRowId, int areaId)
        {
            try
            {
                await this.itemListRowsDataService
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

        public async Task<IOperationResult<ItemListRowDetails>> UpdateAsync(ItemListRowDetails model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            try
            {
                await this.itemListRowsDataService.UpdateAsync(new WMS.Data.WebAPI.Contracts.ItemListRowDetails
                {
                    Id = model.Id,
                    Code = model.Code,
                    RowPriority = model.RowPriority,
                    ItemId = model.ItemId,
                    RequestedQuantity = model.RequestedQuantity,
                    DispatchedQuantity = model.DispatchedQuantity,
                    ItemListRowStatus = (WMS.Data.WebAPI.Contracts.ItemListRowStatus)model.ItemListRowStatus,
                    ItemDescription = model.ItemDescription,
                    CreationDate = model.CreationDate,
                    ItemListCode = model.ItemListCode,
                    ItemListDescription = model.ItemListDescription,
                    ItemListType = (WMS.Data.WebAPI.Contracts.ItemListType)model.ItemListType,
                    CompletionDate = model.CompletionDate,
                    LastExecutionDate = model.LastExecutionDate,
                    LastModificationDate = model.LastModificationDate,
                    Lot = model.Lot,
                    RegistrationNumber = model.RegistrationNumber,
                    Sub1 = model.Sub1,
                    Sub2 = model.Sub2,
                    PackageTypeId = model.PackageTypeId,
                    MaterialStatusId = model.MaterialStatusId,
                    ItemUnitMeasure = model.ItemUnitMeasure,
                    ItemListId = model.ItemListId,
                });

                return new OperationResult<ItemListRowDetails>(true);
            }
            catch (Exception ex)
            {
                return new OperationResult<ItemListRowDetails>(ex);
            }
        }

        #endregion
    }
}
