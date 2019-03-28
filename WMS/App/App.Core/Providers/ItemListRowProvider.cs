using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.Utils.Expressions;
using Ferretto.WMS.App.Core.Extensions;
using Ferretto.WMS.App.Core.Interfaces;
using Ferretto.WMS.App.Core.Models;

namespace Ferretto.WMS.App.Core.Providers
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
                var itemListRow = await this.itemListRowsDataService.CreateAsync(new Data.WebAPI.Contracts.ItemListRowDetails
                {
                    Code = model.Code,
                    Priority = model.RowPriority,
                    ItemId = model.ItemId,
                    ItemListId = model.ItemListId,
                    RequestedQuantity = model.RequestedQuantity,
                    DispatchedQuantity = model.DispatchedQuantity,
                    Status = (WMS.Data.WebAPI.Contracts.ItemListRowStatus)model.Status,
                    ItemListCode = model.ItemListCode,
                    Lot = model.Lot,
                    RegistrationNumber = model.RegistrationNumber,
                    Sub1 = model.Sub1,
                    Sub2 = model.Sub2,
                    PackageTypeId = model.PackageTypeId,
                    MaterialStatusId = model.MaterialStatusId,
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
                await this.itemListRowsDataService.ExecuteAsync(listRowId, areaId, bayId);

                return new OperationResult<ItemListRow>(true);
            }
            catch (Exception ex)
            {
                return new OperationResult<ItemListRow>(ex);
            }
        }

        public async Task<IEnumerable<ItemListRow>> GetAllAsync(
            int skip,
            int take,
            IEnumerable<SortOption> orderBySortOptions = null,
            string whereString = null,
            string searchString = null)
        {
            return (await this.itemListRowsDataService.GetAllAsync(skip, take, whereString, orderBySortOptions.ToQueryString(), searchString))
                .Select(l => new ItemListRow
                {
                    Id = l.Id,
                    Code = l.Code,
                    DispatchedQuantity = l.DispatchedQuantity,
                    ItemDescription = l.ItemDescription,
                    Status = (ItemListRowStatus)l.Status,
                    ItemUnitMeasure = l.ItemUnitMeasure,
                    MaterialStatusDescription = l.MaterialStatusDescription,
                    RequestedQuantity = l.RequestedQuantity,
                    RowPriority = l.Priority,
                    CreationDate = l.CreationDate,
                    Policies = l.GetPolicies(),
                });
        }

        public async Task<int> GetAllCountAsync(string whereString = null, string searchString = null)
        {
            return await this.itemListRowsDataService.GetAllCountAsync(whereString, searchString);
        }

        public async Task<ItemListRowDetails> GetByIdAsync(int id)
        {
            var row = await this.itemListRowsDataService.GetByIdAsync(id);

            var enumeration = new ItemListRowDetails();
            await this.AddEnumerationsAsync(enumeration);

            return new ItemListRowDetails
            {
                Id = row.Id,
                Code = row.Code,
                RowPriority = row.Priority,
                ItemId = row.ItemId,
                RequestedQuantity = row.RequestedQuantity,
                DispatchedQuantity = row.DispatchedQuantity,
                Status = (ItemListRowStatus)row.Status,
                ItemDescription = row.ItemDescription,
                CreationDate = row.CreationDate,
                ItemListCode = row.ItemListCode,
                ItemListDescription = row.ItemListDescription,
                ItemListType = (ItemListType)row.ItemListType,
                CompletionDate = row.CompletionDate,
                LastExecutionDate = row.LastExecutionDate,
                LastModificationDate = row.LastModificationDate,
                Lot = row.Lot,
                RegistrationNumber = row.RegistrationNumber,
                Sub1 = row.Sub1,
                Sub2 = row.Sub2,
                PackageTypeId = row.PackageTypeId,
                MaterialStatusId = row.MaterialStatusId,
                ItemUnitMeasure = row.ItemUnitMeasure,
                MaterialStatusChoices = enumeration.MaterialStatusChoices,
                PackageTypeChoices = enumeration.PackageTypeChoices,
                ItemListId = row.ItemListId,
                Policies = row.GetPolicies(),
            };
        }

        public async Task<IEnumerable<ItemListRow>> GetByItemListIdAsync(int id)
        {
            return (await this.itemListsDataService.GetRowsAsync(id))
                .Select(l => new ItemListRow
                {
                    Id = l.Id,
                    Code = l.Code,
                    RowPriority = l.Priority,
                    ItemDescription = l.ItemDescription,
                    RequestedQuantity = l.RequestedQuantity,
                    DispatchedQuantity = l.DispatchedQuantity,
                    Status = (ItemListRowStatus)l.Status,
                    MaterialStatusDescription = l.MaterialStatusDescription,
                    CreationDate = l.CreationDate,
                    ItemUnitMeasure = l.ItemUnitMeasure,
                    Policies = l.GetPolicies(),
                });
        }

        public async Task<ItemListRowDetails> GetNewAsync(int idList)
        {
            var row = new ItemListRowDetails();
            row.ItemListId = idList;
            await this.AddEnumerationsAsync(row);
            return row;
        }

        public async Task<IEnumerable<object>> GetUniqueValuesAsync(string propertyName)
        {
            return await this.itemListRowsDataService.GetUniqueValuesAsync(propertyName);
        }

        public async Task<IOperationResult<ItemListRow>> ScheduleForExecutionAsync(int listRowId, int areaId)
        {
            try
            {
                await this.itemListRowsDataService.ExecuteAsync(listRowId, areaId, bayId: null);

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
                    Priority = model.RowPriority,
                    ItemId = model.ItemId,
                    RequestedQuantity = model.RequestedQuantity,
                    DispatchedQuantity = model.DispatchedQuantity,
                    Status = (WMS.Data.WebAPI.Contracts.ItemListRowStatus)model.Status,
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

        private async Task AddEnumerationsAsync(ItemListRowDetails row)
        {
            if (row != null)
            {
                row.MaterialStatusChoices = await this.materialStatusProvider.GetAllAsync();
                row.PackageTypeChoices = await this.packageTypeProvider.GetAllAsync();
            }
        }

        #endregion
    }
}
