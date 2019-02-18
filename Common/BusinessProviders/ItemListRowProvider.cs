using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.BusinessModels;
using Ferretto.Common.EF;
using Ferretto.Common.Utils.Expressions;
using Ferretto.WMS.Scheduler.WebAPI.Contracts;
using Microsoft.EntityFrameworkCore;

namespace Ferretto.Common.BusinessProviders
{
    public class ItemListRowProvider : IItemListRowProvider
    {
        #region Fields

        private readonly WMS.Data.WebAPI.Contracts.IItemListRowsDataService itemListRowsDataService;

        private readonly IItemListRowsSchedulerService itemListRowsSchedulerService;

        private readonly WMS.Data.WebAPI.Contracts.IItemListsDataService itemListsDataService;

        private readonly IMaterialStatusProvider materialStatusProvider;

        private readonly IPackageTypeProvider packageTypeProvider;

        #endregion

        #region Constructors

        public ItemListRowProvider(
            IItemListRowsSchedulerService itemListRowsSchedulerService,
            IMaterialStatusProvider materialStatusProvider,
            IPackageTypeProvider packageTypeProvider,
            WMS.Data.WebAPI.Contracts.IItemListRowsDataService itemListRowsDataService,
            WMS.Data.WebAPI.Contracts.IItemListsDataService itemListsDataService)
        {
            this.itemListRowsSchedulerService = itemListRowsSchedulerService;
            this.packageTypeProvider = packageTypeProvider;
            this.materialStatusProvider = materialStatusProvider;
            this.itemListRowsDataService = itemListRowsDataService;
            this.itemListsDataService = itemListsDataService;
        }

        #endregion

        #region Methods

        public async Task<IOperationResult> CreateAsync(ItemListRowDetails model)
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
                    RequiredQuantity = model.RequiredQuantity,
                    DispatchedQuantity = model.DispatchedQuantity,
                    ItemListRowStatus = (WMS.Data.WebAPI.Contracts.ItemListRowStatus)model.ItemListRowStatus,
                    ItemDescription = model.ItemDescription,
                    CreationDate = model.CreationDate,
                    ItemListCode = model.ItemListCode,
                    ItemListDescription = model.ItemListDescription,
                    ItemListType = (WMS.Data.WebAPI.Contracts.ItemListType)model.ItemListType,
                    ItemListStatus = (WMS.Data.WebAPI.Contracts.ItemListStatus)model.ItemListStatus,
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

                return new OperationResult(true);
            }
            catch (Exception ex)
            {
                return new OperationResult(ex);
            }
        }

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

        public async Task<IEnumerable<BusinessModels.ItemListRow>> GetAllAsync(
            int skip = 0,
            int take = 0,
            IEnumerable<SortOption> orderBy = null,
            IExpression whereExpression = null,
            IExpression searchExpression = null)
        {
            var orderByString = orderBy != null ? string.Join(",", orderBy.Select(s => $"{s.PropertyName} {s.Direction}")) : null;

            return (await this.itemListRowsDataService.GetAllAsync(skip, take, whereExpression?.ToString(), orderByString, searchExpression?.ToString()))
                .Select(l => new BusinessModels.ItemListRow
                {
                    Id = l.Id,
                    Code = l.Code,
                    DispatchedQuantity = l.DispatchedQuantity,
                    ItemDescription = l.ItemDescription,
                    ItemListRowStatus = (BusinessModels.ItemListRowStatus)l.ItemListRowStatus,
                    ItemUnitMeasure = l.ItemUnitMeasure,
                    MaterialStatusDescription = l.MaterialStatusDescription,
                    RequiredQuantity = l.RequiredQuantity,
                    RowPriority = l.RowPriority,
                    CreationDate = l.CreationDate
                });
        }

        public async Task<int> GetAllCountAsync(IExpression whereExpression = null, IExpression searchExpression = null)
        {
            return await this.itemListRowsDataService.GetAllCountAsync(whereExpression?.ToString(), searchExpression?.ToString());
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
                RequiredQuantity = itemListRow.RequiredQuantity,
                DispatchedQuantity = itemListRow.DispatchedQuantity,
                ItemListRowStatus = (BusinessModels.ItemListRowStatus)itemListRow.ItemListRowStatus,
                ItemDescription = itemListRow.ItemDescription,
                CreationDate = itemListRow.CreationDate,
                ItemListCode = itemListRow.ItemListCode,
                ItemListDescription = itemListRow.ItemListDescription,
                ItemListType = (ItemListType)itemListRow.ItemListType,
                ItemListStatus = (ItemListStatus)itemListRow.ItemListStatus,
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
                PackageTypeChoices = packageTypeChoices
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
                    RequiredQuantity = l.RequiredQuantity,
                    DispatchedQuantity = l.DispatchedQuantity,
                    ItemListRowStatus = (BusinessModels.ItemListRowStatus)l.ItemListRowStatus,
                    MaterialStatusDescription = l.MaterialStatusDescription,
                    CreationDate = l.CreationDate,
                    ItemUnitMeasure = l.ItemUnitMeasure
                });
        }

        public async Task<IEnumerable<object>> GetUniqueValuesAsync(string propertyName)
        {
            return await this.itemListRowsDataService.GetUniqueValuesAsync(propertyName);
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

        public async Task<IOperationResult> UpdateAsync(ItemListRowDetails model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            try
            {
                await this.itemListRowsDataService.CreateAsync(new WMS.Data.WebAPI.Contracts.ItemListRowDetails
                {
                    Id = model.Id,
                    Code = model.Code,
                    RowPriority = model.RowPriority,
                    ItemId = model.ItemId,
                    RequiredQuantity = model.RequiredQuantity,
                    DispatchedQuantity = model.DispatchedQuantity,
                    ItemListRowStatus = (WMS.Data.WebAPI.Contracts.ItemListRowStatus)model.ItemListRowStatus,
                    ItemDescription = model.ItemDescription,
                    CreationDate = model.CreationDate,
                    ItemListCode = model.ItemListCode,
                    ItemListDescription = model.ItemListDescription,
                    ItemListType = (WMS.Data.WebAPI.Contracts.ItemListType)model.ItemListType,
                    ItemListStatus = (WMS.Data.WebAPI.Contracts.ItemListStatus)model.ItemListStatus,
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

                return new OperationResult(true);
            }
            catch (Exception ex)
            {
                return new OperationResult(ex);
            }
        }

        #endregion
    }
}
